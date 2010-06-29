using System;
using System.Collections.Generic;
using System.Linq;
using Implier.Graph;
using Implier.PureArbitrage;
using Implier.SpreadMatrix;
using QuickFix;

namespace Implier.FIXApplication
{
    internal enum SessionType
    {
        Order = 0,
        Price = 1
    }

    public partial class FixApplication : MessageCracker
    {
        #region Delegaes and Events
        public event MessageRecieveHandler ExecutionReportRecieved;
        #endregion

        #region Methods

        public SpreadMatrixData RequestSymbols(String exchange, String symbol)
        {
            QuickFix42.SecurityDefinitionRequest message = new QuickFix42.SecurityDefinitionRequest(new SecurityReqID(DateTime.Now.ToString()), new SecurityRequestType(SecurityRequestType.REQUEST_LIST_SECURITIES));
            message.setField(new Symbol(symbol));
            message.setField(new SecurityExchange(exchange));

            try
            {
                if (Session.sendToTarget(message, settings.getSessions()[(int)SessionType.Price] as SessionID))
                {
                    SpreadMatrixCollection.Add(exchange, symbol);
                    return SpreadMatrixCollection.Get(exchange, symbol);
                }
                else
                    return null;
            }
            catch (SessionNotFound exception)
            {
                AddText(exception.Message + Environment.NewLine);
                return null;
            }
        }

        public void MarketDataRequest(QuickFix42.SecurityDefinition securityDefinition)
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

            //MaturityMonthYear maturityMonthYear = new MaturityMonthYear();
            //securityDefinition.getField(maturityMonthYear);
            //noRelatedSym.setField(maturityMonthYear);

            SecurityID securityId = new SecurityID();
            securityDefinition.getField(securityId);
            noRelatedSym.setField(securityId);

            marketDataRequest.addGroup(noRelatedSym);

            Session.sendToTarget(marketDataRequest, settings.getSessions()[(int)SessionType.Price] as SessionID);
        }

        public void MarketDataRequestReject(string MDReqID)
        {
            QuickFix42.MarketDataRequestReject marketDataRequestReject = new QuickFix42.MarketDataRequestReject(new MDReqID(MDReqID));

            Session.sendToTarget(marketDataRequestReject, settings.getSessions()[(int)SessionType.Price] as SessionID);
        }

        public static Message NewOrder(IProposal proposal, double quantity)
        {
            Message message = null;
            Account account = new Account("nplatonic");

            //ClOrdID clOrdId = new ClOrdID("ClOrd_" + DateTime.Now.ToString("yyMMdd_HHmmss") );
            ClOrdID clOrdId = new ClOrdID("ClOrd_" + Guid.NewGuid());
            HandlInst handlInst = new HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE_NO_BROKER_INTERVENTION);
            OrdType ordType = new OrdType(OrdType.LIMIT);
            TimeInForce timeInForce = new TimeInForce(TimeInForce.FILL_OR_KILL);
            TransactTime transactTime = new TransactTime();

            Price price = new Price(((Proposal) proposal).EntryPx);
            SecurityExchange securityExchange = new SecurityExchange(((Proposal) proposal).OwnerEntry.SecurityExchange);
            SecurityType securityType = new SecurityType(((Proposal) proposal).OwnerEntry.SecurityType);
            Symbol symbol = new Symbol(((Proposal) proposal).OwnerEntry.Symbol);
            SecurityID securityId = new SecurityID(((Proposal) proposal).OwnerEntry.SecurityID);
            OrderQty orderQty = new OrderQty(quantity);

            Side side = null;
            switch (proposal.Action)
            {
                case EntryType.Bid:
                    side = new Side(Side.SELL);
                    break;
                case EntryType.Offer:
                    side = new Side(Side.BUY);
                    break;
                default:
                    throw new Exception("Undefined entry type.");
            }

            message = new QuickFix42.NewOrderSingle();

            ((QuickFix42.NewOrderSingle) message).set(account);

            ((QuickFix42.NewOrderSingle) message).set(clOrdId);
            ((QuickFix42.NewOrderSingle) message).set(side);
            ((QuickFix42.NewOrderSingle) message).set(transactTime);
            ((QuickFix42.NewOrderSingle) message).set(ordType);

            ((QuickFix42.NewOrderSingle) message).set(price);
            ((QuickFix42.NewOrderSingle) message).set(orderQty);
            ((QuickFix42.NewOrderSingle) message).set(securityId);
            ((QuickFix42.NewOrderSingle) message).set(securityExchange);
            ((QuickFix42.NewOrderSingle) message).set(timeInForce);

            ((QuickFix42.NewOrderSingle) message).set(securityType);

            /*if (((Proposal)proposal).OwnerEntry.SecurityType == SecurityType.FUTURE)
            {
                ContractSide contractSide = proposal.GetSide(0);
                String date = contractSide.SideKey.DateTime.ToString("yyyyMM");
                MaturityMonthYear maturityMonthYear = new MaturityMonthYear(date);
                ((QuickFix42.NewOrderSingle) message).set(maturityMonthYear);
            }*/

            return message;
        }

        public static IEnumerable<Message> NewOrders(Trade trade, double minQuantity)
        {
            //BidType bidType = new BidType(BidType.NON_DISCLOSED_STYLE);

            IEnumerable<Message> messages = trade.Proposals.Select(proposal => NewOrder(proposal, minQuantity));
            
            foreach(Message message in messages)
            {
                if(message is QuickFix43.NewOrderMultileg)
                {
                    QuickFix43.NewOrderMultileg newOrderMultileg = (QuickFix43.NewOrderMultileg) message;

                    QuickFix43.NewOrderMultileg.NoLegs leg = new QuickFix43.NewOrderMultileg.NoLegs();
                    for (uint i = 0; i < newOrderMultileg.getNoLegs().getValue(); i++)
                    {
                        message.getGroup(i + 1, leg);
                        leg.set(new LegSymbol(newOrderMultileg.getSymbol().getValue()));
                    }
                }
            }

            return messages;
        }

        public void RequestOrders(IEnumerable<Message> messages)
        {
            foreach (Message message in messages)
            {
                try
                {
                    Session.sendToTarget(message, settings.getSessions()[(int)SessionType.Order] as SessionID);
                    AddText("newOrder " + message + Environment.NewLine);
                }
                catch (SessionNotFound exception)
                {
                    AddText(exception.Message + Environment.NewLine);
                }
            }
        }

        #endregion

        #region MessageCracker overrides

        public override void onMessage(QuickFix42.ExecutionReport executionReport, SessionID sessionID)
        {
            AddText("executionReport " + executionReport + Environment.NewLine);

            OrderController.Instance.UpdateOrderStatus(executionReport);

            if (ExecutionReportRecieved != null)
                ExecutionReportRecieved(this, executionReport);
        }

        public override void onMessage(QuickFix42.SecurityDefinition securityDefinition, SessionID sessionID)
        {
            AddText("securityDefinition " + securityDefinition + Environment.NewLine);

            try
            {
                SecurityExchange securityExchange = new SecurityExchange();
                securityDefinition.getField(securityExchange);

                Symbol symbol = new Symbol();
                securityDefinition.getField(symbol);

                SpreadMatrixData smd = SpreadMatrixCollection.Get(securityExchange.getValue(), symbol.getValue());

                if (smd != null)
                    MarketDataRequest(securityDefinition);
            }
            catch (Exception exception)
            {
                AddText(exception.Message + Environment.NewLine);
            }
        }

        public override void onMessage(QuickFix42.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh, SessionID sessionID)
        {
            //AddText("marketDataSnapshotFullRefresh " + marketDataSnapshotFullRefresh + Environment.NewLine);

            string exchange = marketDataSnapshotFullRefresh.getSecurityExchange().ToString();
            string symbol = marketDataSnapshotFullRefresh.getSymbol().ToString();

            SpreadMatrixData smd = SpreadMatrixCollection.Get(exchange, symbol);

            if (smd != null)
            {
                MDEntry entry = new MDEntry(marketDataSnapshotFullRefresh, smd.SideController);
                SpreadMatrixCollection.ProcessMessage(exchange, symbol, entry);

                //testing order
                /*if (entry.GroupCount > 0 )
                {
                    Message ord = NewOrder((Proposal) entry.GetGroup(0), 1);
                    AddText("New Order " + ord + Environment.NewLine);
                    Session.sendToTarget(ord, settings.getSessions()[(int) SessionType.Order] as SessionID);
                }*/
            }
        }

        /*public override void onMessage(QuickFix42.MarketDataIncrementalRefresh marketDataIncrementalRefresh, SessionID sessionID)
        {
            //AddText("marketDataIncrementalRefresh " + marketDataIncrementalRefresh + Environment.NewLine);
        }*/
        #endregion
    }
}
