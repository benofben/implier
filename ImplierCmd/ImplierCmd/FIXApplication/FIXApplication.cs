using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using QuickFix;

using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using ImplierAlgorithm.SpreadMatrix;

namespace ImplierCmd.FIXApplication
{
    internal partial class FixApplication : Application
    {
        SpreadMatrix spreadMatrix;

        // For clock issues between FIX Adapter and client
        private bool isTimerAdjusted;
        
        public void onCreate(SessionID sessionID)
        {
            Console.WriteLine("onCreate " + sessionID);
            spreadMatrix = new SpreadMatrix();
        }

        public void onLogon(SessionID sessionID)
        {
            Console.WriteLine("onLogon " + sessionID);

            String exchange = "CME";
            String symbol = "CL";
            RequestSymbols(exchange, symbol, sessionID);
        }

        public void onLogout(SessionID sessionID)
        {
            Console.WriteLine("onLogout " + sessionID);
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
        }

        public void fromAdmin(Message message, SessionID sessionID)
        {
            if (!isTimerAdjusted)
            {
                SendingTime sendingTime = new SendingTime();
                message.getHeader().getField(sendingTime);
                Utils.AdjustTime(sendingTime.getValue());
                isTimerAdjusted = true;
            }
        }

        public void toApp(Message message, SessionID sessionID)
        {
            //Console.WriteLine("toApp " + message);
        }

        public void fromApp(Message message, SessionID sessionID)
        {
            try
            {
                crack(message, sessionID);
            }
            catch (UnsupportedMessageType exception)
            {
                Console.WriteLine("fromApp " + exception);
                Console.WriteLine("fromApp " + message);
            }
        }

    }
}