using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortfolioPerformanceServiceAPI.Models
{
    public class ServiceConfig
    {
        public string ServiceName { get; set; }
        public string Version { get; set; }
        public string MarketDataServiceUrl { get; set; }
    }
}
