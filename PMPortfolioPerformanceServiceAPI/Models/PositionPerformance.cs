using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortfolioPerformanceServiceAPI.Models
{
    public class PositionPerformance
    {
        public string Symbol { get; set; }
        public double CurrentPrice { get; set; }
        public double PurchasedPrice { get; set; }
        public double PurchasedQuantity { get; set; }
        public double PurchasedValue { get; set; }
        public double CurrentValue { get; set; }
        public double PositionUgl { get; set; }
        public double PositionUglPercentage { get; set; }
    }
}
