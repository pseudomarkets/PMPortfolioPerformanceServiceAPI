namespace PseudoMarkets.PerformanceReporting.CalcEngine.Models
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
