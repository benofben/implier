using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class Trade
    {
        public List<Security> securities;

        // refreshed by the IsHedged function
        // Concat({+/-}+MaturityMonthYear)
        public string missingSecurity;

        //true if the trade represents a hedged position
        public bool isHedged;

        public Trade(Security security)
        {
            this.securities = new List<Security>();
            securities.Add(security);

            // if we only have one security, the trade cannot be hedged
            /////////////////////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!
            isHedged = true;
            /////////////////////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!

            // if outright
            if (security.legs.Count == 1)
            {
                if(security.legs[0].StartsWith("-"))
                    missingSecurity=security.legs[0].Replace('-','+');
                else if(security.legs[0].StartsWith("+"))
                    missingSecurity=security.legs[0].Replace('+','-');
            }

            // if spread
            if (security.legs.Count == 2)
            {
                if (security.legs[1].StartsWith("-"))
                    missingSecurity = security.legs[0].Replace('-', '+');
                else if (security.legs[1].StartsWith("+"))
                    missingSecurity = security.legs[0].Replace('+', '-');
            }
        }

        /*
        public Trade(List<Security> legs)
        {
            this.legs = new List<Security>();
            foreach(Security leg in legs)
                this.legs.Add(leg);
        }*/

        //returns how much the trade makes or loses
        public double Cost()
        {
            return 0;
        }
    }
}
