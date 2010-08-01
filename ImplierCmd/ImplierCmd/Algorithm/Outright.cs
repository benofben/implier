using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class Outright : Security
    {
        //FIX Tag 200, maturityMonthYear
        public string maturityMonthYear;

        public Outright(string securityID, string maturityMonthYear)
        {
            this.securityID = securityID;
            this.maturityMonthYear = maturityMonthYear;
        }
    }
}
