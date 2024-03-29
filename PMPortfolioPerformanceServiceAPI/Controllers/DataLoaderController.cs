﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using PMMarketDataService.DataProvider.Client.Implementation;
using PMPortfolioPerformanceServiceAPI.Models;
using PMUnifiedAPI.Models;
using PseudoMarkets.PerformanceReporting.CalcEngine.Models;
using PseudoMarkets.PerformanceReporting.CalcEngine.StandardReport.Implementations;

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
        private readonly PerformanceReportCalculator _performanceReportCalculator;
        private readonly MarketDataServiceClient _marketDataServiceClient;

        public DataLoaderController(PseudoMarketsDbContext context, MongoClient mongoClient, MarketDataServiceClient marketDataServiceClient)
        {
            _context = context;
            _mongoClient = mongoClient;
            _mongoDatabase = _mongoClient.GetDatabase("PseudoMarketsDB");
            _mongoCollection = _mongoDatabase.GetCollection<BsonDocument>("PortfolioPerformance");
            _marketDataServiceClient = marketDataServiceClient;
            _performanceReportCalculator = new PerformanceReportCalculator(_marketDataServiceClient);
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

                    // Step 2: Iterate through each account and grab positions
                    var positionsByAccount = _context.Positions.Where(x => x.AccountId == account.Id).ToList();

                    // Step 3: Generate performance report
                    var reportObject =
                        await _performanceReportCalculator.GeneratePortfolioPerformanceReport(account,
                            positionsByAccount, DataRequestType.RequestType.HistoricalMarketDataRequest);
                    
                    var performanceReport = reportObject;

                    // Step 4: Insert performance data into Mongo DB
                    var performanceReportAsBson = performanceReport.ToBsonDocument();

                    await _mongoCollection.InsertOneAsync(performanceReportAsBson);
                    accountsProcessed++;
                }

                // Step 5: Return the data load results
                result.AccountsProcessed = accountsProcessed;

                if (accountsProcessed > 0)
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
                    Status = "FAILED"
                };

                return result;
            }
        }

        public class DataLoadResult
        {
            public string Status { get; set; }
            public int AccountsProcessed { get; set; }
        }
    }
}
