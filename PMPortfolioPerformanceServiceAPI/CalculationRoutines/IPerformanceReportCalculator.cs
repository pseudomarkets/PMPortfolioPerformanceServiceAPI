using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMCommonEntities.Models.PerformanceReporting;
using PMPortfolioPerformanceServiceAPI.Models;
using PMUnifiedAPI.Models;

namespace PMPortfolioPerformanceServiceAPI.CalculationRoutines
{
    public interface IPerformanceReportCalculator
    {
        public Task<Tuple<int, PortfolioPerformanceReport>> GeneratePortfolioPerformanceReport(Accounts account,
            List<Positions> positions, DataRequestType.RequestType requestType);
    }
}
