using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class SpreadMatrix
    {
        // every security that we have a security definition for
        Dictionary<string, Outright> allOutrights;
        Dictionary<string, Spread> allSpreads; 
        
        // active - every security with a non-zero bidSize or askSize

        // activeOutright[maturityMonthYear]=outright
        Dictionary<string, Outright> activeOutrights;

        /* In the case of spreads each security has two entries in the dictionary, 
         * one for the long and one for the short leg.
         */
        // activeSpreads[maturityMonthYear][securityID]=spread
        Dictionary<string, Dictionary<string, Spread>> activeSpreads;

        public SpreadMatrix()
        {
            allOutrights = new Dictionary<string, Outright>();
            allSpreads = new Dictionary<string, Spread>(); 
            
            activeOutrights = new Dictionary<string, Outright>();
            activeSpreads = new Dictionary<string, Dictionary<string, Spread>>();
        }

        public void CreateOutright(string securityID, string maturityMonthYear)
        {
            Outright outright = new Outright(securityID, maturityMonthYear);
            allOutrights.Add(securityID, outright);
        }

        public void CreateSpread(string securityID, string longUnderlyingMaturityMonthYear, string shortUnderlyingMaturityMonthYear)
        {
            Spread spread = new Spread(securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear);
            allSpreads.Add(securityID, spread);
        }

        public void Update(string securityID, double bidPrice, double bidSize, double askPrice, double askSize)
        {
            if(allOutrights.ContainsKey(securityID))
                UpdateOutright(securityID, bidPrice, bidSize, askPrice, askSize);
            else if(allSpreads.ContainsKey(securityID))
                UpdateSpread(securityID, bidPrice, bidSize, askPrice, askSize);

            CheckForOpportunities();
        }

        public void UpdateOutright(string securityID, double bidPrice, double bidSize, double askPrice, double askSize)
        {
            if (bidPrice == 0 && bidSize == 0 && askPrice == 0 && askSize == 0)
                activeOutrights.Remove(allOutrights[securityID].maturityMonthYear);
            else if(!activeOutrights.ContainsKey(allOutrights[securityID].maturityMonthYear))
                activeOutrights.Add(allOutrights[securityID].maturityMonthYear, allOutrights[securityID]);

            allOutrights[securityID].Update(bidPrice, bidSize, askPrice, askSize);
        }

        public void UpdateSpread(string securityID, double bidPrice, double bidSize, double askPrice, double askSize)
        {
            if (bidPrice == 0 && bidSize == 0 && askPrice == 0 && askSize == 0)
            {
                // remove the long entry for securityID
                if (activeSpreads.ContainsKey(allSpreads[securityID].longUnderlyingMaturityMonthYear) &&
                    activeSpreads[allSpreads[securityID].longUnderlyingMaturityMonthYear].ContainsKey(securityID))
                {
                    activeSpreads[allSpreads[securityID].longUnderlyingMaturityMonthYear].Remove(securityID);
                }

                // if allSpread[longUnderlyingMaturityMonthYear] is empty, remove it
                if (activeSpreads.ContainsKey(allSpreads[securityID].longUnderlyingMaturityMonthYear) &&
                    activeSpreads[allSpreads[securityID].longUnderlyingMaturityMonthYear].Count == 0)
                {
                    activeSpreads.Remove(allSpreads[securityID].longUnderlyingMaturityMonthYear);
                }

                // remove the short entry for securityID
                if (activeSpreads.ContainsKey(allSpreads[securityID].shortUnderlyingMaturityMonthYear) &&
                    activeSpreads[allSpreads[securityID].shortUnderlyingMaturityMonthYear].ContainsKey(securityID))
                {
                    activeSpreads[allSpreads[securityID].shortUnderlyingMaturityMonthYear].Remove(securityID);
                }

                // if allSpread[shortUnderlyingMaturityMonthYear] is empty, remove it
                if (activeSpreads.ContainsKey(allSpreads[securityID].shortUnderlyingMaturityMonthYear) &&
                    activeSpreads[allSpreads[securityID].shortUnderlyingMaturityMonthYear].Count == 0)
                {
                    activeSpreads.Remove(allSpreads[securityID].shortUnderlyingMaturityMonthYear);
                }
            }
            else
            {
                // check if it's on the active list
                // if not, add it

                // make sure the longMaturityMonthYear key exists, if not add it
                if(!activeSpreads.ContainsKey(allSpreads[securityID].longUnderlyingMaturityMonthYear))
                    activeSpreads.Add(allSpreads[securityID].longUnderlyingMaturityMonthYear, new Dictionary<string, Spread>());

                // make sure the longMaturityMonthYear key exists, if not add it
                if(!activeSpreads.ContainsKey(allSpreads[securityID].shortUnderlyingMaturityMonthYear))
                    activeSpreads.Add(allSpreads[securityID].shortUnderlyingMaturityMonthYear, new Dictionary<string, Spread>());

                // make sure the securityID exists under long, if not add it
                if(!activeSpreads[allSpreads[securityID].longUnderlyingMaturityMonthYear].ContainsKey(securityID))
                    activeSpreads[allSpreads[securityID].longUnderlyingMaturityMonthYear].Add(securityID, allSpreads[securityID]);

                // make sure the securityID exists under short, if not add it
                if (!activeSpreads[allSpreads[securityID].shortUnderlyingMaturityMonthYear].ContainsKey(securityID))
                    activeSpreads[allSpreads[securityID].shortUnderlyingMaturityMonthYear].Add(securityID, allSpreads[securityID]);
            }

            allSpreads[securityID].Update(bidPrice, bidSize, askPrice, askSize);
        }

        private void CheckForOpportunities()
        {
            CheckForTradesStartingWithOutrights();
            CheckForTradesWithSpreadsOnly();
        }

        private void CheckForTradesStartingWithOutrights()
        {
            // Check for trades like Sep10, Sep10/Dec10, Dec10
            foreach (KeyValuePair<string, Outright> pair in activeOutrights)
            {
                if (pair.Value.bidSize > 0)
                {
                    Trade trade = new Trade();
                    trade.legs.Add(pair.Value);
                    trade.side.Add(false);
                    CheckForTradesStartingWithOutrights(trade);
                }

                if (pair.Value.askSize > 0)
                {
                    Trade trade = new Trade();
                    trade.legs.Add(pair.Value);
                    trade.side.Add(true);
                    CheckForTradesStartingWithOutrights(trade);
                }
            }
        }

        private void CheckForTradesStartingWithOutrights(Trade trade)
        {
            if (trade.IsHedged())
                Console.WriteLine("Cost: " + trade.Cost());

            string maturityMonthYear = trade.IsMissing();

            Trade newTrade = new Trade(trade.legs, trade.side);
            newTrade.legs.Add(activeOutrights[maturityMonthYear])

            //first, create a trade that closes the graph.  eg if we have Sep10, Sep10/Dec10 then pick the outright Dec10
            //if(trade
        }

        private void CheckForTradesWithSpreadsOnly()
        {
            // Check for trades like Sep10/Dec10, Dec10/Jan11, Jan11/Sep10

        }
    }
}
