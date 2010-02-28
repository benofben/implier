using System;
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
            consoleText = "toAdmin " + message.ToString() + Environment.NewLine + consoleText;
            //form.Invoke(form.consoleDelegate);
        }

        public void toApp(Message message, SessionID sessionID)
        {
            consoleText = "toApp " + message.ToString() + Environment.NewLine + consoleText;
            form.Invoke(form.consoleDelegate);
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
            consoleText = "securityDefinition " + securityDefinition.ToString() + Environment.NewLine + consoleText;
            //form.Invoke(form.consoleDelegate);

            QuickFix42.MarketDataRequest marketDataRequest = new QuickFix42.MarketDataRequest(new MDReqID(DateTime.Now.ToString()), new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES), new MarketDepth(1));
            marketDataRequest.setField(new MDUpdateType(MDUpdateType.FULL_REFRESH));
            marketDataRequest.setField(new AggregatedBook(true));
            
            SecurityExchange securityExchange = new SecurityExchange();
            securityDefinition.getField(securityExchange);
            marketDataRequest.setField(securityExchange);

            QuickFix42.MarketDataRequest.NoRelatedSym noRelatedSym = new QuickFix42.MarketDataRequest.NoRelatedSym();
            
            Symbol symbol = new Symbol();
            securityDefinition.getField(symbol);
            noRelatedSym.setField(symbol);

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
                consoleText = exception.Message + Environment.NewLine + consoleText;
                form.Invoke(form.consoleDelegate);
            }
        }

        public override void onMessage(QuickFix42.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh, SessionID sessionID)
        {
            consoleText = "marketDataSnapshotFullRefresh " + marketDataSnapshotFullRefresh.ToString() + Environment.NewLine + consoleText;
            form.Invoke(form.consoleDelegate);
        }

        public override void onMessage(QuickFix42.MarketDataIncrementalRefresh marketDataIncrementalRefresh, SessionID sessionID)
        {
            consoleText = "marketDataIncrementalRefresh " + marketDataIncrementalRefresh.ToString() + Environment.NewLine + consoleText;
            form.Invoke(form.consoleDelegate);
        }
    }
}