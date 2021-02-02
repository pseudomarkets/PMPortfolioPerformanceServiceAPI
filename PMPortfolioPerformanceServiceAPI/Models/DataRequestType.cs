using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortfolioPerformanceServiceAPI.Models
{
    public class DataRequestType
    {
        public enum RequestType : int
        {
            CurrentMarketDataRequest = 0,
            HistoricalMarketDataRequest = 1
        }
    }
}
