using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IEXTrading.Models
{
    public class CompanyStrategyValue
    {

        public string symbol { get; set; }

        public float? companyValue { get; set; }

        public CompanyStrategyValue() { }

        public CompanyStrategyValue(string symbol,float? companyValue)
        {
            this.symbol = symbol;
            this.companyValue = companyValue;
        }
    }
}
