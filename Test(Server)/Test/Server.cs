using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace Test
{
    public class Server
    {
        public Server()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            projectDirectoryPass = Path.GetDirectoryName(location);

            logger = new Logger(projectDirectoryPass + "\\data\\Log.txt");
            logger.LogInFile("Creating Server...");

            initCurrencyApiClient();

            if (!File.Exists(projectDirectoryPass + "\\data\\usd_data.json"))
            {
                updateCurrencyRate(431);
                //var task = 
                //Task.WhenAll(task);

                logger.LogInFile("File: usd_data.json created");
            }
            else
            {
                loadExistingCurrencyDataFromFile(431);
            }
            if (!File.Exists(projectDirectoryPass + "\\data\\eur_data.json"))
            {
                updateCurrencyRate(978);
                //var task = 
                //Task.WhenAll(task);

                logger.LogInFile("File: eur_data.json created");
            }
            else
            {
                loadExistingCurrencyDataFromFile(978);
            }
            if (!File.Exists(projectDirectoryPass + "\\data\\rub_data.json"))
            {
                updateCurrencyRate(643);

                logger.LogInFile("File: rub_data.json created");
            }
            else
            {
                loadExistingCurrencyDataFromFile(643);
            }
            if (!File.Exists(projectDirectoryPass + "\\data\\btc_data.json"))
            {
                updateBitcoinRate();

                logger.LogInFile("File: btc_data.json created");
            }
            else
            {
                loadExistingBitcoinDataFromFile();
            }

            checkForNewCurrencyData();
            checkForNewBitcoinData();

            loadToFile();

            Console.WriteLine($"Project directory: {projectDirectoryPass}");

            logger.LogInFile("Server created");
        }

        public bool isCloseReaquested = false;
        private bool isCurrencyDataOld = false;
        private bool isBitcoinDataOld = false;

        private HttpClient currencyApiClient;

        private Logger logger;

        private string curApiUrl = "https://www.nbrb.by/api/exrates";
        private string btcApiUrl = "https://api.coincap.io/v2/assets/bitcoin/history?interval=d1";
        private string projectDirectoryPass;

        private List<RateShort> USDdata = new List<RateShort>();
        private List<RateShort> EURdata = new List<RateShort>();
        private List<RateShort> RUBdata = new List<RateShort>();
        private List<BitcoinRate> BTCdata = new List<BitcoinRate>();

        public void Run()
        {
            var builder = WebApplication.CreateBuilder();
            var app = builder.Build();

            app.Run(async (context) =>
            {
                handleRequest(context);
            });

            app.Run();
        }

        private void handleRequest(HttpContext context)
        {
            string path = context.Request.QueryString.ToString();
            Console.WriteLine(path);

            if (path != null)
            {
                Regex regex1 = new Regex(@"\w*[a-z]{3}&startdate=[0-9]{4}-[0-9]{2}-[0-9]{2}&enddate=[0-9]{4}-[0-9]{2}-[0-9]{2}\w*");

                if (regex1.Match(path).Success)
                {
                    Console.WriteLine("запрос графиков валют");

                    logger.LogInFile("Request for filtered data");

                    Regex years = new Regex(@"\w*[0-9]{4}\w*");
                    Regex mounthsAnddDays = new Regex(@"-[0-9]{2}\w*");

                    MatchCollection matchesY = years.Matches(path);
                    MatchCollection matchesMD = mounthsAnddDays.Matches(path);
                                    
                    dateBuilder dateBuild1 = new dateBuilder();
                    dateBuilder dateBuild2 = new dateBuilder();

                    dateBuild1.checkDate(Int32.Parse(matchesY[0].Value.ToString()), Int32.Parse(matchesMD[0].Value.ToString().Substring(1)), Int32.Parse(matchesMD[1].Value.ToString().Substring(1)));
                    dateBuild2.checkDate(Int32.Parse(matchesY[1].Value.ToString()), Int32.Parse(matchesMD[2].Value.ToString().Substring(1)), Int32.Parse(matchesMD[3].Value.ToString().Substring(1)));

                    if ((dateBuild1.isCorrect()) && (dateBuild2.isCorrect()))
                    {
                        DateTime startDate = dateBuild1.createDate();
                        DateTime endDate = dateBuild2.createDate();

                        Console.WriteLine(startDate);
                        Console.WriteLine(endDate);

                        List<RateShort> tmpCurList = new List<RateShort>();

                        Regex usdParam = new Regex(@"usd");
                        Regex eurParam = new Regex(@"eur");
                        Regex rubParam = new Regex(@"rub");
                        Regex btcParam = new Regex(@"btc");

                        if (usdParam.Match(path).Success)
                        {
                            logger.LogInFile("Preparing filtered USD data for sending...");

                            foreach (RateShort a in USDdata)
                            {
                                if ((a.Date >= startDate) && (a.Date <= endDate))
                                {
                                    tmpCurList.Add(a);
                                }
                            }
                        }    
                        if (eurParam.Match(path).Success)
                        {
                            logger.LogInFile("Preparing filtered EUR data for sending...");

                            foreach (RateShort a in EURdata)
                            {
                                if ((a.Date >= startDate) && (a.Date <= endDate))
                                {
                                    tmpCurList.Add(a);
                                }
                            }
                        }
                        if (rubParam.Match(path).Success)
                        {
                            logger.LogInFile("Preparing filtered RUB data for sending...");

                            foreach (RateShort a in RUBdata)
                            {
                                if ((a.Date >= startDate) && (a.Date <= endDate))
                                {
                                    tmpCurList.Add(a);
                                }
                            }
                        }
                        if (btcParam.Match(path).Success)
                        {
                            logger.LogInFile("Preparing filtered BTC data for sending...");

                            foreach (BitcoinRate btc in BTCdata)
                            {
                                if ((btc.date >= startDate) && (btc.date <= endDate))
                                {
                                    RateShort rh = new RateShort();

                                    rh.Cur_ID = 0;
                                    rh.Cur_OfficialRate = (float)btc.priceUsd;
                                    rh.Date = btc.date;

                                    tmpCurList.Add(rh);
                                }
                            }
                        }

                        context.Response.WriteAsJsonAsync(tmpCurList);

                        logger.LogInFile("Filtered data sent");
                        Console.WriteLine("Filtered data sent");
                    }
                    else
                    {
                        logger.LogInFile("StatusCode 404 sent. Wrong date format");
                        context.Response.StatusCode = 404;
                    }
                }
                else
                {
                    logger.LogInFile("StatusCode 404 sent. Wrong parameters format");
                    context.Response.StatusCode = 404;
                }
            }
            else
            {
                logger.LogInFile("StatusCode 404 sent. Wrong path string");
                context.Response.StatusCode = 404;
            }
        }

        private void updateCurrencyRate(int curId)
        {
            DateTime today = DateTime.Now;

            int startYear   = Int32.Parse(today.Year.ToString());
            int startMounth = Int32.Parse(today.Month.ToString());
            int startDay    = Int32.Parse(today.Day.ToString());

            string curUrl = curApiUrl + $"/rates/dynamics/{curId}?parammode=1&startdate={startYear - 2}-{startMounth}-{startDay}&enddate={today.Year}-{today.Month}-{today.Day}";

            var task = getCurrencyRate(curUrl);
            Task.WaitAll(task);

            if (curId == 431)
            {
                USDdata = JsonConvert.DeserializeObject<List<RateShort>>(task.Result);
                logger.LogInFile("USD data loaded to cashe from API");
            }
            if (curId == 978)
            {
                EURdata = JsonConvert.DeserializeObject<List<RateShort>>(task.Result);
                logger.LogInFile("EUR data loaded to cashe from API");
            }
            if (curId == 643)
            {
                RUBdata = JsonConvert.DeserializeObject<List<RateShort>>(task.Result);
                logger.LogInFile("RUB data loaded to cashe from API");
            }
        }

        private void checkForNewCurrencyData()
        {
            DateTime today = DateTime.Now;

            Console.WriteLine("Updating currency data");

            logger.LogInFile("Updating currency data...");

            int currentYear   = Int32.Parse(today.Year.ToString());
            int currentMounth = Int32.Parse(today.Month.ToString());
            int currentDay    = Int32.Parse(today.Day.ToString());

            int oldYear  = Int32.Parse((USDdata.Last().Date.Year.ToString()));
            int oldMonth = Int32.Parse((USDdata.Last().Date.Month.ToString()));
            int oldDay   = Int32.Parse((USDdata.Last().Date.Day.ToString()));


            if ((USDdata.Last().Date.Day != DateTime.Now.Day) || (USDdata.Last().Date.Month != DateTime.Now.Month) || (USDdata.Last().Date.Year != DateTime.Now.Year))
            {
                string usdUrl = curApiUrl + $"/rates/dynamics/431?&startdate={oldYear}-{oldMonth}-{oldDay + 1}&enddate={currentYear}-{currentMounth}-{currentDay}";
                string eurUrl = curApiUrl + $"/rates/dynamics/978?&startdate={oldYear}-{oldMonth}-{oldDay + 1}&enddate={currentYear}-{currentMounth}-{currentDay}";
                string rubUrl = curApiUrl + $"/rates/dynamics/643?&startdate={oldYear}-{oldMonth}-{oldDay + 1}&enddate={currentYear}-{currentMounth}-{currentDay}";

                Task<string> task1 = getCurrencyRate(usdUrl);
                Task<string> task2 = getCurrencyRate(eurUrl);
                Task<string> task3 = getCurrencyRate(rubUrl);

                Task.WaitAll(task1, task2, task3);

                List<RateShort> tmpCurData;

                tmpCurData = JsonConvert.DeserializeObject<List<RateShort>>((task1.Result));
                USDdata.AddRange(tmpCurData);

                tmpCurData = JsonConvert.DeserializeObject<List<RateShort>>((task2.Result));
                EURdata.AddRange(tmpCurData);

                tmpCurData = JsonConvert.DeserializeObject<List<RateShort>>((task3.Result));
                RUBdata.AddRange(tmpCurData);

                logger.LogInFile("Currrency data updated");

                isCurrencyDataOld = true;

                Console.WriteLine("Currrency data updated");
            }
            else
            {
                logger.LogInFile("Currrency data is up today");
            }
        }

        private void checkForNewBitcoinData()
        {
            Console.WriteLine("Updating bitcoin data");

            logger.LogInFile("Updating currency data...");

            if ((BTCdata.Last().date != DateTime.Now))
            {
                updateBitcoinRate();

                logger.LogInFile("Bitcoin data updated");

                isBitcoinDataOld = true;

                Console.WriteLine("Bitcoin data updated");
            }
            else
            {
                logger.LogInFile("Bitcoin data is up today");
            }
        }

        private async void updateBitcoinRate()
        {
            string btcUrl = btcApiUrl;

            string responceString = await getBitcoinRate(btcUrl);

            StringBuilder sb = new StringBuilder(responceString);
            responceString = responceString.Substring(8, responceString.Length - 35);

            BTCdata = JsonConvert.DeserializeObject<List<BitcoinRate>>(responceString);

            logger.LogInFile("BTC data loaded to cashe from API");
        }

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

        private async Task<string> getBitcoinRate(string apiUrl)
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

        private void loadExistingCurrencyDataFromFile(int cur_id)
        {
            if (cur_id == 431)
            {
                using (StreamReader file = File.OpenText(projectDirectoryPass + "\\data\\usd_data.json"))
                {
                    string jsonString = file.ReadToEnd();
                    USDdata = JsonConvert.DeserializeObject<List<RateShort>>(jsonString);

                    logger.LogInFile("USD data loaded to cashe");
                }
            }
            if (cur_id == 978)
            {
                using (StreamReader file = File.OpenText(projectDirectoryPass + "\\data\\eur_data.json"))
                {
                    string jsonString = file.ReadToEnd();
                    EURdata = JsonConvert.DeserializeObject<List<RateShort>>(jsonString);
                   
                    logger.LogInFile("EUR data loaded to cashe");
                }
            }
            if (cur_id == 643)
            {
                using (StreamReader file = File.OpenText(projectDirectoryPass + "\\data\\rub_data.json"))
                {
                    string jsonString = file.ReadToEnd();
                    RUBdata = JsonConvert.DeserializeObject<List<RateShort>>(jsonString);

                    logger.LogInFile("RUB data loaded to cashe");
                }
            }

            logger.LogInFile("Currency data loaded to cashe");
        }

        private void loadExistingBitcoinDataFromFile()
        {
            using (StreamReader file = File.OpenText(projectDirectoryPass + "\\data\\btc_data.json"))
            {
                string jsonString = file.ReadToEnd();
                BTCdata = JsonConvert.DeserializeObject<List<BitcoinRate>>(jsonString);

                logger.LogInFile("BTC data loaded to cashe");
            }
        }

        private void initCurrencyApiClient()
        {
            currencyApiClient = new HttpClient();

            if (currencyApiClient != null)
            {
                Console.WriteLine("Currency API client initilized.");
            }
        }

        private void loadToFile()
        {
            Console.WriteLine("Saving newly dowloaded data...");

            logger.LogInFile("Saving all data...");

            if (isCurrencyDataOld == true)
            {
                string USDjsonString = JsonConvert.SerializeObject(USDdata);

                using (FileStream fs = new FileStream(projectDirectoryPass + "\\data\\usd_data.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(USDjsonString);

                    logger.LogInFile("USD currency data saved in file");

                    Console.WriteLine($"Data saved");
                }

                string EURjsonString = JsonConvert.SerializeObject(EURdata);

                using (FileStream fs = new FileStream(projectDirectoryPass + "\\data\\eur_data.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(EURjsonString);

                    logger.LogInFile("EUR currency data saved in file");

                    Console.WriteLine($"Data saved");
                }

                string RUBjsonString = JsonConvert.SerializeObject(RUBdata);

                using (FileStream fs = new FileStream(projectDirectoryPass + "\\data\\rub_data.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(RUBjsonString);

                    logger.LogInFile("RUB currency data saved in file");

                    Console.WriteLine($"Data saved");
                }
            }
            else
            {
                logger.LogInFile("Currency data doesn't need to de saved");
                Console.WriteLine($"Currency data doesn't need to de saved");
            }

            if (isBitcoinDataOld == true)
            {
                string BTCjsonString = JsonConvert.SerializeObject(BTCdata);

                using (FileStream fs = new FileStream(projectDirectoryPass + "\\data\\btc_data.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(BTCjsonString);

                    logger.LogInFile("BTC data saved in file");

                    Console.WriteLine($"BTC Data saved");
                }
                
            }
            else
            {
                logger.LogInFile("Bitcoin data doesn't need to de saved");
                Console.WriteLine($"Bitcoin data doesn't need to de saved");
            }
        }
       
        public void handleInput()
        {
            string command = Console.ReadLine();
            
            if(command == "ex")
            {
                isCloseReaquested = true;

                logger.LogInFile("Server shut down");
            }
            else
            {
                Console.WriteLine("Unknown command");
            }
        }      
    }
}
