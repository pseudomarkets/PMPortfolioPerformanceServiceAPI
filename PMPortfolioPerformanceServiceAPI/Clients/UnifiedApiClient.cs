using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PMCommonApiModels.ResponseModels;

namespace PMPortfolioPerformanceServiceAPI.Clients
{
    public static class UnifiedApiClient
    {
        private static HttpClient httpClient = new HttpClient();

        public static async Task<double> GetLatestPriceAsync(string symbol)
        {
            string endpoint = "https://app.pseudomarkets.live/api/Quotes/SmartQuote/" + symbol;
            var response = await httpClient.GetAsync(endpoint);
            string responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<LatestPriceOutput>(responseString);
            var price = jsonResponse.price;

            return price;
        }
    }
}
