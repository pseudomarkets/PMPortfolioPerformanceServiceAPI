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
using PMCommonEntities.Models.PerformanceReporting;
using PMPortfolioPerformanceServiceAPI.CalculationRoutines;
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

                    // Step 2: Iterate through each account and grab positions
                    var positionsByAccount = _context.Positions.Where(x => x.AccountId == account.Id).ToList();

                    // Step 3: Generate performance report
                    var reportObject =
                        await PerformanceReportCalculator.GeneratePortfolioPerformanceReport(account,
                            positionsByAccount);

                    positionsProcessed += reportObject.Item1;
                    var performanceReport = reportObject.Item2;

                    // Step 4: Insert performance data into Mongo DB
                    var performanceReportAsBson = performanceReport.ToBsonDocument();

                    _mongoCollection.InsertOne(performanceReportAsBson);
                    accountsProcessed++;
                }

                // Step 5: Return the data load results
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
