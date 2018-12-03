using IEXTrading.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IEXTrading.Infrastructure.IEXTradingHandler
{
    public class IEXHandler
    {
        static string url = "https://api.iextrading.com/1.0/"; //This is the base URL, method specific URL is appended to this.
        HttpClient client;

        public IEXHandler()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /****
         * Calls the IEX reference API to get the list of symbols. 
        ****/
        public List<Company> GetSymbols()
        {
            string APIPath = url + "ref-data/symbols";
            string Company_List = "";

            List<Company> Companies = null;

            client.BaseAddress = new Uri(APIPath);
            HttpResponseMessage ResponseMsg = client.GetAsync(APIPath).GetAwaiter().GetResult();
            if (ResponseMsg.IsSuccessStatusCode)
            {
                Company_List = ResponseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!Company_List.Equals(""))
            {
                Companies = JsonConvert.DeserializeObject<List<Company>>(Company_List);
            }
            return Companies;
        }


        public List<Quote> GetQuotes(List<Company> companies)
        {
            string Symbols = "";

            List<Quote> Quote_List = new List<Quote>();
            Dictionary<string, Dictionary<string, Quote>> Quote_Dict = null;
            int Batch_Start = 0;
            int Batch_End = 100;
            int Step_Count = 100;
            List<Company> Batch_Company = null;
            List<CompanyStrategyValue> Value_List = new List<CompanyStrategyValue>();
            while (Batch_End <= companies.Count)
            {
                int Count = 0;
                Symbols = "";
                Batch_Company = new List<Company>();
                Batch_Company = companies.GetRange(Batch_Start, Step_Count);
                foreach (var company in Batch_Company)
                {
                    Count++;
                    Symbols = Symbols + company.symbol + ",";
                }


                string APIPath = url + "stock/market/batch?symbols=" + Symbols + "&types=quote";
                string Quote_Resp = "";

                HttpResponseMessage ResponseMsg = client.GetAsync(APIPath).GetAwaiter().GetResult();
                if (ResponseMsg.IsSuccessStatusCode)
                {
                    Quote_Resp = ResponseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                Quote_Dict = new Dictionary<string, Dictionary<string, Quote>>();
                if (!string.IsNullOrEmpty(Quote_Resp))
                {
                    Quote_Dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Quote>>>(Quote_Resp);
                }

                foreach (var Quote_Item in Quote_Dict)
                {

                    foreach (var Quote in Quote_Item.Value)
                    {
                        if (Quote.Value != null)
                        {
                            Quote_List.Add(Quote.Value);
                        }
                    }
                }

                Batch_Start = Batch_End;
                Batch_End = Batch_End + 100;
                if (Batch_End > companies.Count)
                {
                    Step_Count = Batch_End - companies.Count;
                }
            }
            return Quote_List;
        }

        public List<CompanyStrategyValue> GetTop5Picks(List<Company> companies)
        {
            List<Quote> Quote_List = new List<Quote>();
            CompanyStrategyValue CmpStrat_Value = null;
            Quote_List = GetQuotes(companies);
            List<CompanyStrategyValue> Value_List = new List<CompanyStrategyValue>();
            foreach (var Quote in Quote_List)
            {
                CmpStrat_Value = new CompanyStrategyValue();
                CmpStrat_Value.symbol = Quote.symbol;
                if ((Quote.week52High - Quote.week52Low) != 0)
                {
                    CmpStrat_Value.companyValue = ((Quote.close - Quote.week52Low) / (Quote.week52High - Quote.week52Low));
                }
                Value_List.Add(CmpStrat_Value);
            }
            
            return Value_List.OrderByDescending(a => a.companyValue).Take(5).ToList();
        }

        /****
         * Calls the IEX stock API to get one year's chart for the supplied symbol. 
        ****/
        public List<Equity> GetList(string symbol)
        {
            string APIPath = url + "stock/" + symbol + "/batch?types=chart&range=1y";

            string list = "";
            List<Equity> Equities = new List<Equity>();
            client.BaseAddress = new Uri(APIPath);
            HttpResponseMessage ResponseMsg = client.GetAsync(APIPath).GetAwaiter().GetResult();
            if (ResponseMsg.IsSuccessStatusCode)
            {
                list = ResponseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            if (!list.Equals(""))
            {
                ChartRoot root = JsonConvert.DeserializeObject<ChartRoot>(list, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Equities = root.chart.ToList();
            }
            foreach (Equity Equity in Equities)
            {
                Equity.symbol = symbol;
            }

            return Equities;
        }
    }
}
