using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImplierCmd.Algorithm
{
    class SpreadMatrix
    {
        //[concat({+/-}, securityID)]
        Dictionary<string, Security> allOutrights = new Dictionary<string, Security>();
        Dictionary<string, Security> allSpreads = new Dictionary<string, Security>();
        Dictionary<string, Security> activeOutrights = new Dictionary<string, Security>();
        Dictionary<string, Security> activeSpreads = new Dictionary<string, Security>();

        public void CreateOutright(string securityID, string maturityMonthYear)
        {
            allOutrights.Add("+" + securityID, new Security(securityID, "+" + maturityMonthYear, "+"));
            allOutrights.Add("-" + securityID, new Security(securityID, "-" + maturityMonthYear, "-"));
        }

        public void CreateSpread(string securityID, string longUnderlyingMaturityMonthYear, string shortUnderlyingMaturityMonthYear)
        {
            allSpreads.Add("+" + securityID, new Security(
                securityID,
                "+" + longUnderlyingMaturityMonthYear,
                "-" + shortUnderlyingMaturityMonthYear, 
                "+"));

            allSpreads.Add("-" + securityID, new Security(
                securityID,
                "-" + longUnderlyingMaturityMonthYear,
                "+" + shortUnderlyingMaturityMonthYear,
                "-"));
        }

        public void Update(string securityID, double bidPrice, double bidSize, double askPrice, double askSize)
        {
            if (allOutrights.ContainsKey("-" + securityID))
                UpdateOutright(securityID, bidPrice, bidSize, askPrice, askSize);
            else if (allSpreads.ContainsKey("-" + securityID))
                UpdateSpread(securityID, bidPrice, bidSize, askPrice, askSize);
            
            CheckForTrades();
        }

        public void UpdateOutright(string securityID, double bidPrice, double bidSize, double askPrice, double askSize)
        {
            if (bidSize > 0)
            {
                if (!activeOutrights.ContainsKey("-" + securityID))
                    activeOutrights.Add("-" + securityID, allOutrights["-" + securityID]);
            }
            else
            {
                if (activeOutrights.ContainsKey("-" + securityID))
                    activeOutrights.Remove("-" + securityID);
            }

            if (askSize > 0)
            {
                if (!activeOutrights.ContainsKey("+" + securityID))
                    activeOutrights.Add("+" + securityID, allOutrights["+" + securityID]);
            }
            else
            {
                if (activeOutrights.ContainsKey("+" + securityID))
                    activeOutrights.Remove("+" + securityID);
            }
             
            UpdateOutright("-" + securityID, bidPrice, bidSize);
            UpdateOutright("+" + securityID, askPrice, askSize);
        }

        private void UpdateOutright(string key, double price, double size)
        {
            allOutrights[key].price = price;
            allOutrights[key].size = size;
        }

        public void UpdateSpread(string securityID, double bidPrice, double bidSize, double askPrice, double askSize)
        {
            if (bidSize > 0)
            {
                if (!activeSpreads.ContainsKey("-" + securityID))
                    activeSpreads.Add("-" + securityID, allSpreads["-" + securityID]);
            }
            else
            {
                if (activeSpreads.ContainsKey("-" + securityID))
                    activeSpreads.Remove("-" + securityID);
            }

            if (askSize > 0)
            {
                if (!activeSpreads.ContainsKey("+" + securityID))
                    activeSpreads.Add("+" + securityID, allSpreads["+" + securityID]);
            }
            else
            {
                if (activeSpreads.ContainsKey("+" + securityID))
                    activeSpreads.Remove("+" + securityID);
            }

            UpdateSpread("-" + securityID, bidPrice, bidSize);
            UpdateSpread("+" + securityID, askPrice, askSize);
        }

        private void UpdateSpread(string key, double price, double size)
        {
            allSpreads[key].price = price;
            allSpreads[key].size = size;
        }

        private void CheckForTrades()
        {
            CheckForTradesStartingWithOutrights();
            CheckForTradesWithSpreadsOnly();
        }

        private void CheckForTradesStartingWithOutrights()
        {
            // Check for trades like Sep10, Sep10/Dec10, Dec10

            // for each active outright, start create a trade
            foreach (KeyValuePair<string, Security> pair in activeOutrights)
            {
                Trade trade = new Trade(pair.Value);
                CheckForTrades(trade);
            }

        }

        private void CheckForTradesWithSpreadsOnly()
        {
            // Check for trades like Sep10/Dec10, Dec10/Jan11, Jan11/Sep10

        }

        private void CheckForTrades(Trade trade)
        {
            if (trade.isHedged)
            {
                Console.WriteLine("Found a trade for " + trade.Cost());
                return;
            }

            //first, try to hedge the trade if it started with an outright
            if(trade.securities[0].legs.Count==1)
            trade.missingSecurity;
        }
    }
}
