using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    abstract class Security
    {
        //FIX Tag 48, SecurityID
        public string securityID;

        //FIX Tag 271, MDEntryPx
        public double bidPrice;
        public double askPrice;

        //FIX Tag 271, MDEntrySize
        public double bidSize;
        public double askSize;

        public void Update(double bidPrice, double bidSize, double askPrice, double askSize)
        {
            this.bidPrice = bidPrice;
            this.bidSize = bidSize;
            this.askPrice = askPrice;
            this.askSize = askSize;
        }
    }
}
