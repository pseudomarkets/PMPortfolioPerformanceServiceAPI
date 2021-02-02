using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMCommonEntities.Models.PerformanceReporting;
using PMMarketDataService.DataProvider.Client.Implementation;
using PMPortfolioPerformanceServiceAPI.Models;
using PMUnifiedAPI.Models;

namespace PMPortfolioPerformanceServiceAPI.CalculationRoutines
{
    public class PerformanceReportCalculator : IPerformanceReportCalculator
    {
        private readonly MarketDataServiceClient _marketDataServiceClient;
        public PerformanceReportCalculator(MarketDataServiceClient marketDataServiceClient)
        {
            _marketDataServiceClient = marketDataServiceClient;
        }

        public async Task<Tuple<int, PortfolioPerformanceReport>> GeneratePortfolioPerformanceReport(Accounts account,
            List<Positions> positions, DataRequestType.RequestType requestType)
        {
            int positionsProcessed = 0;

            var currentBalance = account.Balance;
            double totalCurrentValue = 0;
            double investedBalance = 0;

            List<PositionPerformance> positionPerformances = new List<PositionPerformance>();

            // Iterate through each position to perform position level performance calculations
            foreach (Positions position in positions)
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
                totalCurrentValue += positionCurrentValue;

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

                positionPerformances.Add(new PositionPerformance()
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

                positionsProcessed++;
            }

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
            PortfolioPerformanceReport performanceReport = new PortfolioPerformanceReport()
            {
                AccountId = account.Id,
                CurrentCashBalance = currentBalance,
                CurrentInvestmentValue = totalCurrentValue,
                PortfolioPerformance = positionPerformances,
                CurrentTotalAccountValue = totalAccountValue,
                ReportDate = DateTime.Today,
                PortfolioUgl = portfolioUgl,
                PortfolioUglPercentage = portfolioUglPercentage
            };

            return new Tuple<int, PortfolioPerformanceReport>(positionsProcessed,
                performanceReport);
        }
    }
}
