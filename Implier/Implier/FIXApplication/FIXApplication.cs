using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Implier.SpreadMatrix;
using QuickFix;

namespace Implier.FIXApplication
{
    internal partial class FixApplication : Application
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

        private bool isTimerAdjusted;

        readonly SocketInitiator initiator;

        private SessionSettings settings;
        private FileStoreFactory storeFactory;
        private FileLogFactory logFactory;
        private MessageFactory messageFactory;
        private int logonCount = 0;

        //SpreadMatrixData SMData = null;
        SpreadMatrixCollection spreadMatrixCollection = new SpreadMatrixCollection();

        #endregion

        #region Delegaes and Events
        public delegate void MessageRecieveHandler(object sender, Message message);
        public delegate void AddTextHandler(object sender, string text);
        public event AddTextHandler OnTextAdded;
        public event EventHandler Logon;
        public event EventHandler Logout;
        #endregion

        #region Properties

        public int LogonCount
        {
            get { return logonCount; }
        }

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

        private int PriceSessionIndex{ get; set; }
        private int OrderSessionIndex{ get; set; }

        internal SpreadMatrixCollection SpreadMatrixCollection
        {
            get { return spreadMatrixCollection; }
        }

        //internal SpreadMatrixData SpreadMatrixData
        //{
        //    get
        //    {
        //        return spreadMatrixCollection.IsEmpty ? null : spreadMatrixCollection.First;
        //    }
        //}
        public static FixApplication Current
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        public FixApplication(String configFile)
        {
            try
            {
                logonCount = 0;
                Current = this;
                settings = new SessionSettings(configFile);
                storeFactory = new FileStoreFactory(settings);
                logFactory = new FileLogFactory(settings);
                messageFactory = new DefaultMessageFactory();
                SetSessionIndexes(configFile);

                initiator = new SocketInitiator(this, storeFactory, settings, logFactory, messageFactory);
                initiator.start();
                AddText("Created initiator." + Environment.NewLine);
            }
            catch (Exception exception)
            {
                AddText(exception.Message + Environment.NewLine);
                throw;
            }           
        }
        #endregion

        #region Methods

        private void SetSessionIndexes(String configFile)
        {
            List<string> targetCompIDs = GetSessionTargetCompIDs(configFile);
            int priceIndex = 0;
            int orderIndex = 1;
            
            PriceSessionIndex = settings.getSessions().Cast<SessionID>().ToArray().ToList().FindIndex(
                sessionId => sessionId.getTargetCompID().Equals(targetCompIDs[priceIndex]));

            OrderSessionIndex = settings.getSessions().Cast<SessionID>().ToArray().ToList().FindIndex(
                sessionId => sessionId.getTargetCompID().Equals(targetCompIDs[orderIndex]));
        }

        private List<string> GetSessionTargetCompIDs(String configFile)
        {
            List<string> targetCompIDs = new List<string>();

             using(TextReader tr = new StreamReader(configFile))
             {
                 while (tr.Peek() >= 0)
                 {
                     string line = tr.ReadLine();

                     if (line.Contains("TargetCompID"))
                         targetCompIDs.Add(line.Split('=')[1]);
                 }
             }
            return targetCompIDs;
        }

        private void AddText(string text)
        {
            lock (lockObject)
            {
                consoleText.Append(text);
            }
            if (OnTextAdded != null)
                OnTextAdded(this, text);
        }
        
        public void StopFix()
        {
            try
            {
                initiator.stop();
                initiator.Dispose();
                AddText("Stopped initiator." + Environment.NewLine);
                SpreadMatrixCollection.Clear();
                AddText("Matrix is cleared." + Environment.NewLine);
                Current = null;
                logonCount = 0;
            }
            catch (Exception exception)
            {
                AddText(exception.Message + Environment.NewLine);
                throw;
            }
        }

        public SessionID GetOrderSession()
        {
            return settings.getSessions()[OrderSessionIndex] as SessionID;
        }

        public SessionID GetPriceSession()
        {
            return settings.getSessions()[PriceSessionIndex] as SessionID;
        }

        #endregion 

        #region QuickFix.Application implementation
        public void onCreate(SessionID sessionID)
        {
            AddText("onCreate " + sessionID + Environment.NewLine);
        }

        public void onLogon(SessionID sessionID)
        {
            AddText("onLogon " + sessionID + Environment.NewLine);

            if(logonCount<2)
                logonCount ++;

            if (Logon != null)
                Logon(this, new EventArgs<SessionID>(sessionID));
        }

        public void onLogout(SessionID sessionID)
        {
            AddText("onLogout " + sessionID + Environment.NewLine);
            
            if(logonCount>0)
                logonCount --;

            if (Logout != null)
                Logout(this, new EventArgs<SessionID>(sessionID));
        }

        public void RequestLogon()
        {
            foreach (SessionID sessionId in settings.getSessions()) 
                RequestLogon(sessionId);
        }

        public void RequestLogon(SessionID sessionId)
        {
            QuickFix42.Logon logon = new QuickFix42.Logon(new EncryptMethod(EncryptMethod.NONE), new HeartBtInt(30));
            logon.set(new ResetSeqNumFlag(true));

            Session.sendToTarget(logon, sessionId);
        }

        public void RequestLogout(SessionID sessionId)
        {
            QuickFix42.Logout logout = new QuickFix42.Logout();
            Session.sendToTarget(logout, sessionId);
        }

        public void toAdmin(Message message, SessionID sessionID)
        {
            // This is only for the TT dev environment.  The production FIX Adapter does not require a password
            MsgType msgType = new MsgType();
            message.getHeader().getField(msgType);

            TargetCompID targetCompID = new TargetCompID();
            message.getHeader().getField(targetCompID);

            if (msgType.ToString() == MsgType.Logon && 
                (targetCompID.ToString() == "TTDEV9P" || targetCompID.ToString() == "TTDEV9O"))
            {
                const string password = "12345678";
                RawData rawData = new RawData(password);
                message.getHeader().setField(rawData);
            }
            // End TT Dev environment case

            //AddText("toAdmin " + message + Environment.NewLine);
        }

        public void fromAdmin(Message message, SessionID sessionID)
        {
            if(!isTimerAdjusted)
            {
                SendingTime sendingTime = new SendingTime();
                message.getHeader().getField(sendingTime);
                Utils.AdjustTime(sendingTime.getValue());
                isTimerAdjusted = true;
            }

            //AddText("fromAdmin " + message + Environment.NewLine);
        }

        public void toApp(Message message, SessionID sessionID)
        {
            //AddText("toApp " + message + Environment.NewLine);
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
                throw;
            }
        }
        #endregion
    }
}