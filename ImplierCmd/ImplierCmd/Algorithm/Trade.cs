using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class Trade
    {
        public List<Security> legs;
        
        // false = sell, true = buy
        public List<bool> side;

        public Trade()
        {
            legs = new List<Security>();
            side = new List<bool>();
        }

        public Trade(List<Security> legs, List<bool> side)
        {
            this.legs = legs;
            this.side = side;
        }

        //returns how much the trade makes or loses
        public double Cost()
        {
            return 0;
        }

        //true if the trade represents a hedged position
        public bool IsHedged()
        {
            return false;
        }

        // returns the MaturityMonthYear currently missing for the trade
        // if this is a spread only trade, it will only return one missing leg
        public {string, bool} IsMissing()
        {
            return null;
        }
    }
}
