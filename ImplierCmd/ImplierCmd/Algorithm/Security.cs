using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class Security
    {
        //FIX Tag 48, SecurityID
        public string securityID;

        //FIX Tag 271, MDEntryPx
        public double price;

        //FIX Tag 271, MDEntrySize
        public double size;
        
        // + if buy, - if sell
        public string side;

        //concat({+/-}, maturityMonthYear)
        public List<string> legs = new List<string>();

        // Constructor for outright
        public Security(string securityID, string leg, string side)
        {
            this.securityID = securityID;
            this.legs.Add(leg);
            this.side = side;
        }

        // Constructor for spread
        public Security(string securityID, string leg1, string leg2, string side)
        {
            this.securityID = securityID;
            this.legs.Add(leg1);
            this.legs.Add(leg2);
            this.side = side;
        }

        public void Update(double price, double size)
        {
            this.price = price;
            this.size = size;
        }
    }
}
