using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class Spread : Security
    {
        //FIX Tag 313, UnderlyingMaturityMonthYear
        public string longUnderlyingMaturityMonthYear;
        public string shortUnderlyingMaturityMonthYear;

        public Spread(string securityID, string longUnderlyingMaturityMonthYear, string shortUnderlyingMaturityMonthYear)
        {
            this.securityID = securityID;
            this.longUnderlyingMaturityMonthYear = longUnderlyingMaturityMonthYear;
            this.shortUnderlyingMaturityMonthYear = shortUnderlyingMaturityMonthYear;
        }

    }
}
