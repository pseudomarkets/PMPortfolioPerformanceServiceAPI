using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMCommonEntities.Models.PerformanceReporting;
using PMMarketDataService.DataProvider.Client.Interfaces;
using PMUnifiedAPI.Models;
using PseudoMarkets.PerformanceReporting.CalcEngine.Models;
using PseudoMarkets.PerformanceReporting.CalcEngine.StandardReport.Interfaces;

namespace PseudoMarkets.PerformanceReporting.CalcEngine.StandardReport.Implementations
{
    public class PerformanceReportCalculator : IPerformanceReportCalculator
    {
        private readonly IMarketDataServiceClient _marketDataServiceClient;
        
        public PerformanceReportCalculator(IMarketDataServiceClient marketDataServiceClient)
        {
            _marketDataServiceClient = marketDataServiceClient;
        }

        public async Task<PortfolioPerformanceReport> GeneratePortfolioPerformanceReport(Accounts account,
            List<Positions> positions, DataRequestType.RequestType requestType)
        {
            var currentBalance = account.Balance;
            double totalCurrentValue = 0;
            double investedBalance = 0;
            
            var positionPerformanceBag = new ConcurrentBag<PositionPerformance>();

            var calcTasks = positions.Select(async position =>
            {
                var positionInvestedValue = position.Value;
                investedBalance += positionInvestedValue;
                var symbol = position.Symbol;
                var positionQuantity = position.Quantity;

                var originalCostPerShare = positionInvestedValue / positionQuantity;

                double currentPrice = 0;

                switch (requestType)
                {
                    case DataRequestType.RequestType.CurrentMarketDataRequest:
                        var marketData = await _marketDataServiceClient.GetLatestPrice(symbol);
                        currentPrice = marketData.price;
                        break;
                    case DataRequestType.RequestType.HistoricalMarketDataRequest:
                        var historicalData =
                            await _marketDataServiceClient.GetHistoricalData(symbol, DateTime.Today.ToString("yyyyMMdd"));
                        currentPrice = historicalData.ClosingPrice;
                        break;
                    default:
                        currentPrice = 0;
                        break;
                }

                var positionCurrentValue = currentPrice * positionQuantity;
                var positionUgl = (positionCurrentValue) - (positionInvestedValue);

                double positionUglPercentage = 0;
                if (positionUgl > 0)
                {
                    positionUglPercentage = (positionUgl / positionInvestedValue) * 100;
                }
                else
                {
                    positionUglPercentage = (-1 * (positionUgl / positionInvestedValue)) * 100;
                }

                positionPerformanceBag.Add(new PositionPerformance()
                {
                    CurrentPrice = currentPrice,
                    CurrentValue = positionCurrentValue,
                    PositionUgl = positionUgl,
                    PositionUglPercentage = positionUglPercentage,
                    PurchasedPrice = originalCostPerShare,
                    PurchasedQuantity = positionQuantity,
                    PurchasedValue = positionInvestedValue,
                    Symbol = symbol
                });
            });

            await Task.WhenAll(calcTasks);

            totalCurrentValue = positionPerformanceBag.Select(x => x.CurrentValue).Sum();
            
            var portfolioUgl = totalCurrentValue - investedBalance;
            double portfolioUglPercentage = 0;

            if (portfolioUgl > 0)
            {
                portfolioUglPercentage = (portfolioUgl / investedBalance) * 100;
            }
            else
            {
                portfolioUglPercentage = (-1 * (portfolioUglPercentage / investedBalance)) * 100;
            }

            var totalAccountValue = totalCurrentValue + currentBalance;

            // Generate Performance Report object
            return new PortfolioPerformanceReport()
            {
                AccountId = account.Id,
                CurrentCashBalance = currentBalance,
                CurrentInvestmentValue = totalCurrentValue,
                PortfolioPerformance = new List<PositionPerformance>(positionPerformanceBag),
                CurrentTotalAccountValue = totalAccountValue,
                ReportDate = DateTime.Today,
                PortfolioUgl = portfolioUgl,
                PortfolioUglPercentage = portfolioUglPercentage
            };
        }
    }
}
