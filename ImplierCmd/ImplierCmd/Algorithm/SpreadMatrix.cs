using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class SpreadMatrix
    {
        Dictionary<string, Outright> outrights;
        Dictionary<string, Spread> spreads;

        public SpreadMatrix()
        {
            outrights = new Dictionary<string,Outright>();
            spreads = new Dictionary<string,Spread>();
        }

        public void CreateOutright(string securityID, string maturityMonthYear)
        {
            Console.WriteLine("Creating outright: " + maturityMonthYear);
            Outright outright = new Outright(securityID, maturityMonthYear);
            outrights.Add(securityID, outright);
        }

        public void Update(string securityID, double bidPrice, double bidSize, double askPrice, double askSize)
        {
            if (outrights.ContainsKey(securityID))
            {
                Console.WriteLine("Updating price for outright " + securityID);
                outrights[securityID].Update(bidPrice, bidSize, askPrice, askSize);
            }
            else if (spreads.ContainsKey(securityID))
            {
                Console.WriteLine("Updating price for spread " + securityID);
                spreads[securityID].Update(bidPrice, bidSize, askPrice, askSize);
            }            
        }

        public void CreateSpread(string securityID, string longUnderlyingMaturityMonthYear, string shortUnderlyingMaturityMonthYear)
        {
            Console.WriteLine("Creating spread: " + longUnderlyingMaturityMonthYear + " " + shortUnderlyingMaturityMonthYear);
            Spread spread = new Spread(securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear);
            spreads.Add(securityID, spread);
        }
    }
}
