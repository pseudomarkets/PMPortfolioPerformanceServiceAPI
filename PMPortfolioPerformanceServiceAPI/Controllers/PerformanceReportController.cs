using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PMCommonEntities.Models.PerformanceReporting;
using PMPortfolioPerformanceServiceAPI.CalculationRoutines;
using PMPortfolioPerformanceServiceAPI.Models;

namespace PMPortfolioPerformanceServiceAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PerformanceReportController : ControllerBase
    {

        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<BsonDocument> _mongoCollection;
        private readonly PseudoMarketsDbContext _context;

        public PerformanceReportController(PseudoMarketsDbContext context, MongoClient mongoClient)
        {
            _context = context;
            _mongoClient = mongoClient;
            _mongoDatabase = _mongoClient.GetDatabase("PseudoMarketsDB");
            _mongoCollection = _mongoDatabase.GetCollection<BsonDocument>("PortfolioPerformance");
        }

        [Route("GetCurrentPerformance/{accountId}")]
        [HttpGet]
        public async Task<PortfolioPerformanceReport> GetCurrentPerformanceForAccount(int accountId)
        {
            try
            {
                var account = _context.Accounts.FirstOrDefault(x => x.Id == accountId);
                if (account != null)
                {
                    var positions = _context.Positions.Where(x => x.AccountId == account.Id).ToList();

                    var reportObject =
                        await PerformanceReportCalculator.GeneratePortfolioPerformanceReport(account, positions);

                    return reportObject.Item2;
                }
                else
                {
                    return new PortfolioPerformanceReport()
                    {
                        AccountId = -1
                    };
                }
            }
            catch (Exception e)
            {
                return new PortfolioPerformanceReport()
                {
                    AccountId = -1
                };
            }
        }

        // GET: api/PerformanceReport/GetPerformanceReport
        [Route("GetPerformanceReport/{accountId}/{date}")]
        [HttpGet]
        public PortfolioPerformanceReport GetPerformanceReportForAccount(int accountId, string date)
        {
            try
            {
                var performanceReportDate = DateTime.ParseExact(date, "yyyyMMdd",
                    CultureInfo.InvariantCulture);
                var accountFilter = Builders<BsonDocument>.Filter.Eq("AccountId", accountId);
                var dateFilter = Builders<BsonDocument>.Filter.Eq("ReportDate", performanceReportDate);
                var performanceDoc = _mongoCollection.Find(accountFilter & dateFilter).FirstOrDefault();

                if (performanceDoc != null)
                {
                    var performanceReport = BsonSerializer.Deserialize<PortfolioPerformanceReport>(performanceDoc);
                    return performanceReport;
                }
                else
                {
                    PortfolioPerformanceReport report = new PortfolioPerformanceReport()
                    {
                        AccountId = -1
                    };
                    return report;
                }
            }
            catch (Exception e)
            {
                return new PortfolioPerformanceReport()
                {
                    AccountId = -1
                };
            }
        }

    }
}
