using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S2T4Bank.Model
{

    public class LuisInsurance
    {
        public DatetimeLuis[] Datetime { get; set; }
    }


    public class DatetimeLuis
    {
        public string Type { get; set; }
        public string[] Timex { get; set; }
    }


}
