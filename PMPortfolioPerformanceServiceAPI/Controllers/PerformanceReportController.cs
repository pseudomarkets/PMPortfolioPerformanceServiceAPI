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

        public PerformanceReportController(MongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            _mongoDatabase = _mongoClient.GetDatabase("PseudoMarketsDB");
            _mongoCollection = _mongoDatabase.GetCollection<BsonDocument>("PortfolioPerformance");
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
                        AccountId = accountId
                    };
                    return report;
                }
            }
            catch (Exception e)
            {
                return new PortfolioPerformanceReport()
                {
                    AccountId = accountId
                };
            }
        }

    }
}
