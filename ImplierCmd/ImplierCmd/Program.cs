using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImplierCmd.FIXApplication;
using ImplierCmd.Algorithm;
using QuickFix;

namespace ImplierCmd
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            String configFile = "SchneiderBen.cfg";
            
            try
            {
                SessionSettings settings = new SessionSettings(configFile);
                FixApplication application = new FixApplication();
                FileStoreFactory storeFactory = new FileStoreFactory(settings);
                ScreenLogFactory logFactory = new ScreenLogFactory(settings);
                MessageFactory messageFactory = new DefaultMessageFactory();
                SocketInitiator initiator = new SocketInitiator(application, storeFactory, settings, logFactory, messageFactory);

                initiator.start();

                Console.WriteLine("press <enter> to quit");
                Console.Read();
                initiator.stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
