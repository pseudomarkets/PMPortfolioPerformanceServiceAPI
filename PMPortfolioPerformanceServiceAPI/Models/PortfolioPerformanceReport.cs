using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace PMPortfolioPerformanceServiceAPI.Models
{
    [BsonIgnoreExtraElements]
    public class PortfolioPerformanceReport
    {
        public int AccountId { get; set; }
        public DateTime ReportDate { get; set; }
        public double CurrentCashBalance { get; set; }
        public double CurrentInvestmentValue { get; set; }
        public double CurrentTotalAccountValue { get; set; }
        public double PortfolioUgl { get; set; }
        public double PortfolioUglPercentage { get; set; }
        public List<PositionPerformance> PortfolioPerformance { get; set; }
    }
}
