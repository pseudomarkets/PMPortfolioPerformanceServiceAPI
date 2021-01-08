using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using PMPortfolioPerformanceServiceAPI.Clients;
using PMPortfolioPerformanceServiceAPI.Models;
using PMUnifiedAPI.Models;

namespace PMPortfolioPerformanceServiceAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DataLoaderController : ControllerBase
    {

        private readonly IMongoClient _mongoClient;
        private readonly PseudoMarketsDbContext _context;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<BsonDocument> _mongoCollection;

        public DataLoaderController(PseudoMarketsDbContext context, MongoClient mongoClient)
        {
            _context = context;
            _mongoClient = mongoClient;
            _mongoDatabase = _mongoClient.GetDatabase("PseudoMarketsDB");
            _mongoCollection = _mongoDatabase.GetCollection<BsonDocument>("PortfolioPerformance");
        }

        // GET: api/DataLoader
        [Route("LoadPerformanceData")]
        [HttpGet]
        public async Task<DataLoadResult> PerformDataLoad()
        {
            try
            {
                // Step 0: Prepare data load response object
                DataLoadResult result = new DataLoadResult();
                int accountsProcessed = 0;
                int positionsProcessed = 0;

                // Step 1: Fetch all accounts in SQL Server Accounts table
                var accounts = _context.Accounts.ToList();

                foreach (Accounts account in accounts)
                {
                    List<PositionPerformance> positionPerformances = new List<PositionPerformance>();

                    // Step 2: Iterate through each account and grab positions and invested balance
                    var positionsByAccount = _context.Positions.Where(x => x.AccountId == account.Id);

                    var currentBalance = account.Balance;
                    double totalCurrentValue = 0;
                    double investedBalance = 0;

                    // Step 3: Iterate through each position to perform position level performance calculations
                    foreach (Positions position in positionsByAccount)
                    {
                        var positionInvestedValue = position.Value;
                        investedBalance += positionInvestedValue;
                        var symbol = position.Symbol;
                        var positionQuantity = position.Quantity;

                        var originalCostPerShare = positionInvestedValue / positionQuantity;

                        var currentPrice = await UnifiedApiClient.GetLatestPriceAsync(symbol);

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

                    // Step 4: Generate Performance Report object
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

                    // Step 5: Insert performance data into Mongo DB
                    var performanceReportAsBson = performanceReport.ToBsonDocument();

                    _mongoCollection.InsertOne(performanceReportAsBson);
                    accountsProcessed++;

                }

                // Step 6: Return the data load results
                result.AccountsProcessed = accountsProcessed;
                result.PositionsProcessed = positionsProcessed;

                if (accountsProcessed > 0 && positionsProcessed > 0)
                {
                    result.Status = "OK";
                }
                else
                {
                    result.Status = "PARTIAL LOAD";
                }

                return result;
            }
            catch (Exception e)
            {
                DataLoadResult result = new DataLoadResult()
                {
                    AccountsProcessed = 0,
                    PositionsProcessed = 0,
                    Status = "FAILED"
                };

                return result;
            }
        }

        public class DataLoadResult
        {
            public string Status { get; set; }
            public int PositionsProcessed { get; set; }
            public int AccountsProcessed { get; set; }
        }
    }
}
