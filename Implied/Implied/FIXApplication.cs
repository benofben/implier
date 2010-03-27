using System;
using System.IO;
using System.Threading;
using QuickFix;

namespace Implied
{
    public class FIXApplication : MessageCracker, QuickFix.Application
    {
        public String consoleText; 
        
        SocketInitiator initiator;
        FormImplied form;
        SessionID sessionID;
        TextWriter tw;

        public FIXApplication()
        {
        }

        public FIXApplication(FormImplied parentForm)
        {
            form = parentForm;

            try
            {
                SessionSettings settings = new SessionSettings("ImpliedFIX.cfg");
                FileStoreFactory storeFactory = new FileStoreFactory(settings);
                FileLogFactory logFactory = new FileLogFactory(settings);
                MessageFactory messageFactory = new DefaultMessageFactory();
                initiator = new SocketInitiator(this, storeFactory, settings, logFactory, messageFactory);
                initiator.start();
                consoleText = "Created initiator." + Environment.NewLine;
            }
            catch (Exception exception)
            {
                consoleText = exception.Message + Environment.NewLine;
            }

            tw = new StreamWriter("symbols.txt");
           
        }

        public void StopFIX()
        {
            try
            {
                initiator.stop();
                consoleText = "Stopped initiator." + Environment.NewLine + consoleText;
                form.Invoke(form.consoleDelegate);
            }
            catch (Exception exception)
            {
                consoleText = exception.Message + Environment.NewLine + consoleText;
                form.Invoke(form.consoleDelegate);
            }

            tw.Close();
        }

        public void onCreate(SessionID sessionID)
        {
            this.sessionID = sessionID;
        }

        public void onLogon(SessionID sessionID)
        {
            consoleText = "onLogon " + sessionID.toString() + Environment.NewLine + consoleText;
            //form.Invoke(form.consoleDelegate);
        }

        public void onLogout(SessionID sessionID)
        {
            consoleText = "onLogout " + sessionID.toString() + Environment.NewLine + consoleText;
            form.Invoke(form.consoleDelegate);
        }

        public void toAdmin(Message message, SessionID sessionID)
        {
            ///////////this might not work --- it's never been tested
            QuickFix.MsgType msgType = new QuickFix.MsgType();
            message.getHeader().getField(msgType);
            if (msgType.ToString() == QuickFix.MsgType.LOGON)
            {
                string password = "12345678";
                QuickFix.RawData rawData = new RawData(password);
                message.getHeader().setField(rawData);
            }


            consoleText = "toAdmin " + message.ToString() + Environment.NewLine + consoleText;
            form.Invoke(form.consoleDelegate);
        }

        public void toApp(Message message, SessionID sessionID)
        {
            //consoleText = "toApp " + message.ToString() + Environment.NewLine + consoleText;
            //form.Invoke(form.consoleDelegate);
        }

        public void fromAdmin(Message message, SessionID sessionID)
        {
            consoleText = "fromAdmin " + message.ToString() + Environment.NewLine + consoleText;
            form.Invoke(form.consoleDelegate);
        }

        public void fromApp(Message message, SessionID sessionID)
        {
            try
            {
                crack(message, sessionID);
            }
            catch (QuickFix.UnsupportedMessageType exception)
            {
                consoleText = "fromApp " + exception + Environment.NewLine + consoleText;
                consoleText = "fromApp " + message.ToString() + Environment.NewLine + consoleText;
                form.Invoke(form.consoleDelegate);
            }
        }

        public void requestSymbols(String exchange, String symbol)
        {
            QuickFix42.SecurityDefinitionRequest message = new QuickFix42.SecurityDefinitionRequest(new SecurityReqID(DateTime.Now.ToString()), new SecurityRequestType(SecurityRequestType.REQUEST_LIST_SECURITIES));
            message.setField(new Symbol(symbol));
            message.setField(new SecurityExchange(exchange));

            try
            {
                Session.sendToTarget(message, sessionID);
            }
            catch (SessionNotFound exception) 
            {
                consoleText = exception.Message + Environment.NewLine + consoleText;
                form.Invoke(form.consoleDelegate);            
            }
        }

        public override void onMessage(QuickFix42.SecurityDefinition securityDefinition, SessionID sessionID)
        {
            tw.WriteLine(securityDefinition.ToString());
            //consoleText = "securityDefinition " + securityDefinition.ToString() + Environment.NewLine + consoleText;
            //form.Invoke(form.consoleDelegate);

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

            SecurityID securityID = new SecurityID();
            securityDefinition.getField(securityID);
            noRelatedSym.setField(securityID);
            
            marketDataRequest.addGroup(noRelatedSym);

            try
            {
                Session.sendToTarget(marketDataRequest, sessionID);
            }
            catch (SessionNotFound exception)
            {
                //consoleText = exception.Message + Environment.NewLine + consoleText;
                //form.Invoke(form.consoleDelegate);
            }
        }

        public override void onMessage(QuickFix42.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh, SessionID sessionID)
        {
            //consoleText = "marketDataSnapshotFullRefresh " + marketDataSnapshotFullRefresh.ToString() + Environment.NewLine + consoleText;
            //form.Invoke(form.consoleDelegate);
        }

        public override void onMessage(QuickFix42.MarketDataIncrementalRefresh marketDataIncrementalRefresh, SessionID sessionID)
        {
            //consoleText = "marketDataIncrementalRefresh " + marketDataIncrementalRefresh.ToString() + Environment.NewLine + consoleText;
            //form.Invoke(form.consoleDelegate);
        }
    }
}