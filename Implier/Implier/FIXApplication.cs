using System;
using System.IO;
using System.Text;
using QuickFix;

namespace Implier
{
   
    public class FixApplication : MessageCracker, Application
    {
        #region Fields
        /// <summary>
        /// accumulator for messages
        /// </summary>
        private readonly StringBuilder consoleText = new StringBuilder(); 
        /// <summary>
        /// sync object for thread-safe access to consoleText
        /// </summary>
        private readonly object lockObject = new object();

        readonly SocketInitiator initiator;
        SessionID sessionId;
        readonly TextWriter tw;
        #endregion

        #region Delegaes and Events
        public delegate void AddTextHandler(object sender, string text);
        public event AddTextHandler OnTextAdded;
        #endregion

        #region Properties

        public string ConsoleText
        {
            get 
            {
                lock (lockObject)
                {
                    return consoleText.ToString();
                }
            }
        }

        #endregion

        #region Constructor
        public FixApplication(String configFile)
        {
            try
            {
                SessionSettings settings = new SessionSettings(configFile);
                FileStoreFactory storeFactory = new FileStoreFactory(settings);
                FileLogFactory logFactory = new FileLogFactory(settings);
                MessageFactory messageFactory = new DefaultMessageFactory();
                initiator = new SocketInitiator(this, storeFactory, settings, logFactory, messageFactory);
                initiator.start();
                AddText("Created initiator." + Environment.NewLine);
            }
            catch (Exception exception)
            {
                AddText(exception.Message + Environment.NewLine);
            }

            tw = new StreamWriter("symbols.txt");
           
        }
        #endregion

        #region Methods

        private void AddText(string text)
        {
            lock (lockObject)
            {
                consoleText.Append(text);
            }
            if (OnTextAdded != null)
                OnTextAdded(this, text);
        }

        public void RequestSymbols(String exchange, String symbol)
        {
            QuickFix42.SecurityDefinitionRequest message = new QuickFix42.SecurityDefinitionRequest(new SecurityReqID(DateTime.Now.ToString()), new SecurityRequestType(SecurityRequestType.REQUEST_LIST_SECURITIES));
            message.setField(new Symbol(symbol));
            message.setField(new SecurityExchange(exchange));

            try
            {
                Session.sendToTarget(message, sessionId);
            }
            catch (SessionNotFound exception)
            {
                AddText(exception.Message + Environment.NewLine);
            }
        }

        public void StopFix()
        {
            try
            {
                initiator.stop();
                initiator.Dispose();
                AddText("Stopped initiator." + Environment.NewLine);
            }
            catch (Exception exception)
            {
                AddText(exception.Message + Environment.NewLine);
            }

            tw.Close();
        }
        #endregion 

        #region QuickFix.Application implementation
        public void onCreate(SessionID sessionID)
        {
            sessionId = sessionID;
        }

        public void onLogon(SessionID sessionID)
        {
            AddText("onLogon " + sessionID + Environment.NewLine);
        }

        public void onLogout(SessionID sessionID)
        {
            AddText("onLogout " + sessionID + Environment.NewLine);
        }

        public void toAdmin(Message message, SessionID sessionID)
        {
            // This is only for the TT dev environment.  The production FIX Adapter does not require a password
            MsgType msgType = new MsgType();
            message.getHeader().getField(msgType);
            if (msgType.ToString() == MsgType.Logon)
            {
                const string password = "12345678";
                RawData rawData = new RawData(password);
                message.getHeader().setField(rawData);
            }

            AddText("toAdmin " + message + Environment.NewLine);
        }

        public void fromAdmin(Message message, SessionID sessionID)
        {
            AddText("fromAdmin " + message + Environment.NewLine);
        }

        public void toApp(Message message, SessionID sessionID)
        {
            AddText("toApp " + message + Environment.NewLine);
        }

        public void fromApp(Message message, SessionID sessionID)
        {
            try
            {
                crack(message, sessionID);
            }
            catch (UnsupportedMessageType exception)
            {
                AddText("fromApp " + exception + Environment.NewLine);
                AddText("fromApp " + message + Environment.NewLine);
            }
        }
        #endregion

        #region MessageCracker overrides
        public override void onMessage(QuickFix42.SecurityDefinition securityDefinition, SessionID sessionID)
        {
            tw.WriteLine(securityDefinition.ToString());
            AddText("securityDefinition " + securityDefinition + Environment.NewLine);

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

            SecurityID securityId = new SecurityID();
            securityDefinition.getField(securityId);
            noRelatedSym.setField(securityId);
            
            marketDataRequest.addGroup(noRelatedSym);

            try
            {
                Session.sendToTarget(marketDataRequest, sessionID);
            }
            catch (SessionNotFound exception)
            {
                AddText(exception.Message + Environment.NewLine);
            }

        }

        public override void onMessage(QuickFix42.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh, SessionID sessionID)
        {
            AddText("marketDataSnapshotFullRefresh " + marketDataSnapshotFullRefresh + Environment.NewLine);
        }

        public override void onMessage(QuickFix42.MarketDataIncrementalRefresh marketDataIncrementalRefresh, SessionID sessionID)
        {
            AddText("marketDataIncrementalRefresh " + marketDataIncrementalRefresh + Environment.NewLine);
        }
        #endregion
    }
}