using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

namespace Client.Models
{
    public class currencyRate
    {
        private HttpClient currencyApiClient;
        private List<RateShort> CurRate;
        private Logger logger;
        private string requestUrl = "https://localhost:5001";
        private Dictionary<int, string> curIndToCurId;

        private async Task<string> getCurrencyRate(string apiUrl)
        {
            using (HttpResponseMessage response = await currencyApiClient.GetAsync(apiUrl))
            {
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    return responseBody;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        } 

        public bool isRateListEmpty()
        {
            return CurRate == null || CurRate.Count == 0;
        }

        public List<RateShort> getCurrencyRateList()
        {
            return CurRate;
        }

        public async Task updateCurrencyRate(int currencyIndex, DateTime starDate, DateTime endDate)
        {
            DateBuilder dateBuild = new DateBuilder();
            
            dateBuild.FormateDate(starDate);
            string startYear  = dateBuild.GetFormattedYear();
            string startMouth = dateBuild.GetFormattedMonth();
            string startDay   = dateBuild.GetFormattedDay();

            dateBuild.FormateDate(endDate);
            string endYear   = dateBuild.GetFormattedYear();
            string endMouth  = dateBuild.GetFormattedMonth();
            string endDay    = dateBuild.GetFormattedDay();

            string curUrl = $"{requestUrl}/rate?{curIndToCurId[currencyIndex]}&startdate={startYear}-{startMouth}-{startDay}&enddate={endYear}-{endMouth}-{endDay}";

            logger.LogInFile($"Request for {curIndToCurId[currencyIndex]} data: {curUrl}");

            string responce = await getCurrencyRate(curUrl);

            logger.LogInFile("Responce content accepted");

            CurRate = JsonConvert.DeserializeObject<List<RateShort>>(responce);

        }

        public currencyRate()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string projectDirectoryPass = Path.GetDirectoryName(location);

            logger = new Logger(projectDirectoryPass + "\\data\\Log.txt");

            currencyApiClient = new HttpClient();

            CurRate = new List<RateShort>();
            curIndToCurId = new Dictionary<int, string>();

            curIndToCurId.Add(0, "usd");
            curIndToCurId.Add(1, "eur");
            curIndToCurId.Add(2, "rub");
            curIndToCurId.Add(3, "btc");

            logger.LogInFile("Client created");
        }
    }
}
