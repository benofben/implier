using System;
using System.Collections.Generic;
using System.Linq;
using QuickFix;
using QuickFix42;

namespace ImplierCmd.FIXApplication
{
    internal partial class FixApplication : QuickFix.MessageCracker
    {
        #region Methods

        public void RequestSymbols(String exchange, String symbol, SessionID sessionID)
        {
            QuickFix42.SecurityDefinitionRequest message = new QuickFix42.SecurityDefinitionRequest(new SecurityReqID(DateTime.Now.ToString()), new SecurityRequestType(SecurityRequestType.REQUEST_LIST_SECURITIES));
            message.setField(new Symbol(symbol));
            message.setField(new SecurityExchange(exchange));
            
            //TT Custom Field: RequestTickTable
            //message.setField(new BooleanField(17000, true));

            try
            {
                Session.sendToTarget(message, sessionID);
            }
            catch (SessionNotFound exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void MarketDataRequest(QuickFix42.SecurityDefinition securityDefinition, SessionID sessionID)
        {
            QuickFix42.MarketDataRequest marketDataRequest =
                new QuickFix42.MarketDataRequest(new MDReqID(DateTime.Now.ToString()),
                new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES),
                new MarketDepth(1));
            marketDataRequest.setField(new MDUpdateType(MDUpdateType.FULL_REFRESH));
            marketDataRequest.setField(new AggregatedBook(true));

            QuickFix42.MarketDataRequest.NoMDEntryTypes marketDataEntyGroupBid = new QuickFix42.MarketDataRequest.NoMDEntryTypes();
            marketDataEntyGroupBid.set(new MDEntryType(MDEntryType.BID));
            marketDataRequest.addGroup(marketDataEntyGroupBid);

            QuickFix42.MarketDataRequest.NoMDEntryTypes marketDataEntyGroupOffer = new QuickFix42.MarketDataRequest.NoMDEntryTypes();
            marketDataEntyGroupOffer.set(new MDEntryType(MDEntryType.OFFER));
            marketDataRequest.addGroup(marketDataEntyGroupOffer);

            /** Create Component Block NoRelatedSym */
            QuickFix42.MarketDataRequest.NoRelatedSym noRelatedSym = new QuickFix42.MarketDataRequest.NoRelatedSym();

            SecurityExchange securityExchange = new SecurityExchange();
            securityDefinition.getField(securityExchange);
            noRelatedSym.setField(securityExchange);

            SecurityType securityType = new SecurityType();
            securityDefinition.getField(securityType);
            noRelatedSym.setField(securityType);

            Symbol symbol = new Symbol();
            securityDefinition.getField(symbol);
            noRelatedSym.setField(symbol);

            SecurityID securityId = new SecurityID();
            securityDefinition.getField(securityId);
            noRelatedSym.setField(securityId);                    
            
            marketDataRequest.addGroup(noRelatedSym);

            Session.sendToTarget(marketDataRequest, sessionID);
        }

        #endregion

        #region MessageCracker overrides

        public override void onMessage(QuickFix42.SecurityDefinition securityDefinition, SessionID sessionID)
        {
            //Console.WriteLine("securityDefinition " + securityDefinition);
            
            try
            {
                SecurityType securityType = new SecurityType();
                securityDefinition.getField(securityType);

                SecurityID securityID = new SecurityID();
                securityDefinition.getField(securityID);
     
                //Create the object in the spreadmatrix;

                if(securityType.getValue() == SecurityType.MULTILEGINSTRUMENT)
                {
                    string longUnderlyingMaturityMonthYear=null;
                    string shortUnderlyingMaturityMonthYear=null;

                    NoRelatedSym noRelatedSym = securityDefinition.getNoRelatedSym();
                    uint SubContractCount = (uint)noRelatedSym.getValue();

                    if (SubContractCount != 2)
                    {
                        //Console.WriteLine("I don't know how to handle an MLEG with " + SubContractCount + " legs.");
                        return;
                    }

                    SecurityDefinition.NoRelatedSym group = new SecurityDefinition.NoRelatedSym();
                    for (uint i = 0; i < SubContractCount; i++)
                    {
                        securityDefinition.getGroup(i + 1, group);

                        UnderlyingMaturityMonthYear underlyingMaturityMonthYear = new UnderlyingMaturityMonthYear();
                        group.getField(underlyingMaturityMonthYear);

                        Side side = new Side();
                        group.getField(side);

                        if (side.getValue() == QuickFix.Side.SELL)
                        {
                            shortUnderlyingMaturityMonthYear = underlyingMaturityMonthYear.getValue();
                        }
                        else if (side.getValue() == QuickFix.Side.BUY)
                        {
                            longUnderlyingMaturityMonthYear = underlyingMaturityMonthYear.getValue();
                        }
                        else
                        {
                            Console.WriteLine("Unsupport MLEG side: " + side.getValue());
                            return;
                        }
                    }

                    spreadMatrix.CreateSpread(securityID.getValue(), longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear);
                }
                else if (securityType.getValue() == SecurityType.FUTURE)
                {
                    MaturityMonthYear maturityMonthYear = new MaturityMonthYear();
                    securityDefinition.getField(maturityMonthYear);

                    spreadMatrix.CreateOutright(securityID.ToString(), maturityMonthYear.ToString());
                }
                else
                {
                    Console.WriteLine("Unsupport security type: " + securityType.getField());
                    return;
                }

                MarketDataRequest(securityDefinition, sessionID);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public override void onMessage(QuickFix42.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh, SessionID sessionID)
        {
            //Console.WriteLine("marketDataSnapshotFullRefresh " + marketDataSnapshotFullRefresh);

            uint numberOfEntries = (uint)marketDataSnapshotFullRefresh.getNoMDEntries().getValue();
            if (numberOfEntries > 2)
            {
                Console.WriteLine("I don't know what to do with more than two price entries. I got " + numberOfEntries + ".");
                return;
            }

            string securityID = marketDataSnapshotFullRefresh.getSecurityID().getValue();
            double bidPrice = 0;
            double bidSize = 0;
            double askPrice = 0;
            double askSize = 0;
            MarketDataSnapshotFullRefresh.NoMDEntries group = new MarketDataSnapshotFullRefresh.NoMDEntries();
            for (uint i = 0; i < numberOfEntries; i++)
            {
                marketDataSnapshotFullRefresh.getGroup(i + 1, group);
                if (group.getMDEntryType().getValue() == MDEntryType.BID)
                {
                    bidPrice = group.getMDEntryPx().getValue();
                    bidSize = group.getMDEntrySize().getValue();
                }
                else if (group.getMDEntryType().getValue() == MDEntryType.BID)
                {
                    askPrice = group.getMDEntryPx().getValue();
                    askSize = group.getMDEntrySize().getValue();
                }
            }

            spreadMatrix.Update(securityID, bidPrice, bidSize, askPrice, askSize);
        }

        public override void onMessage(QuickFix42.MarketDataRequestReject marketDataRequestReject, SessionID sessionID)
        {
            Console.WriteLine("marketDataRequestReject " + marketDataRequestReject);
        }

        #endregion
    }
}
