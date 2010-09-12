using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Implier.Graph;
using Implier.SpreadMatrix;

namespace Implier.FIXApplication
{
    class LogModeController : Singleton<LogModeController>, IUpdatableObject
    {
        #region Fields
        ISupportableObject messageProvider = null;
        readonly object lockObject = new object();
        List<DisposableBaseObject> changedList = new List<DisposableBaseObject>();
        Thread thread;
        static string path = Constants.ImplierLogPath;
        #endregion

        private LogModeController() { }

        #region Methods
        public ISupportableObject MessageProvider
        {
            get { return messageProvider; }
            set
            {
                if (messageProvider != null)
                    messageProvider.DNUUnregisterUpdatableObject(this);

                messageProvider = value;

                if (messageProvider != null)
                {
                    messageProvider.DNURegisterUpdatableObject(this);
                    ForceTotalUpdate();
                }
            }
        }

        public void Changed(DisposableBaseObject obj)
        {
            lock (lockObject)
            {
                changedList.Add(obj);
            }
        }

        protected void Update()
        {
            using (TextWriter tw = File.AppendText(path))
            {
                while (true)
                {
                    List<DisposableBaseObject> copy = null;
                    lock (lockObject)
                    {
                        copy = changedList.Distinct().ToList();
                        changedList = new List<DisposableBaseObject>();
                    }

                    foreach (DisposableBaseObject obj in copy)
                    {
                        SpreadMatrixData smd = (SpreadMatrixData) obj;
                        if (!smd.IsDisposed)
                        {
                            List<Trade> trades = smd.RunSimpleTest();

                            foreach (Trade trade in trades)
                            {
                                if (trade.CostWODeal <= 0)
                                    continue;

                                StringBuilder builder = new StringBuilder();
                                builder.AppendFormat("{0}\t | ", DateTime.UtcNow);
                                builder.AppendFormat("Legs: {0}\t | ", trade.Path.Count());
                                builder.AppendFormat("Quantity: {0}\t | ", trade.Quantity);
                                builder.AppendFormat("CostWODeal: {0}\t | ", trade.CostWODeal);
                                builder.AppendFormat("Cost: {0}", trade.Cost);

                                tw.WriteLine(builder.ToString());
                            }
                        }
                    }

                    Thread.Sleep(5);
                }
            }
        }

        public void ForceTotalUpdate()
        {
            SpreadMatrixCollection smc = (SpreadMatrixCollection)MessageProvider;
            lock (smc.LockObject)
            {
                foreach (SpreadMatrixData value in smc.Values)
                {
                    Changed(value);
                }
            }
        }

        void StartThread()
        {
            MessageProvider = FixApplication.Current.SpreadMatrixCollection;
            thread = new Thread(new ThreadStart(Update));
            thread.Start();
        }

        void StopThread()
        {
            MessageProvider = null;
            thread.Abort();
            thread = null;
        }

        public static void Start()
        {
            Instance.StartThread();
        }

        public static void Stop()
        {
            Instance.StopThread();
        }

        #endregion
    }
}
