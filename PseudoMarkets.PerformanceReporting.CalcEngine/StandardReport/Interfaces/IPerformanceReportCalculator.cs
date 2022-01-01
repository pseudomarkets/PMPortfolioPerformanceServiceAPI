using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PMCommonEntities.Models.PerformanceReporting;
using PMUnifiedAPI.Models;
using PseudoMarkets.PerformanceReporting.CalcEngine.Models;

namespace PseudoMarkets.PerformanceReporting.CalcEngine.StandardReport.Interfaces
{
    public interface IPerformanceReportCalculator
    {
        Task<PortfolioPerformanceReport> GeneratePortfolioPerformanceReport(Accounts account,
            List<Positions> positions, DataRequestType.RequestType requestType);
    }
}
