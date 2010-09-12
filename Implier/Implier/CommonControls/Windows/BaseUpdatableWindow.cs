using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Implier.FIXApplication;
using Implier.Graph;

namespace Implier.CommonControls.Windows
{
    internal delegate void UpdateWindowHandler(object sender, DisposableBaseObject obj);
    internal delegate void InternalUpdateHandler(BaseUpdatableWindow wnd);

    internal abstract class BaseUpdatableWindow : Window, IUpdatableObject
    {
        #region Fields
        Timer delay;
        public const int Tick = 30;
        ISupportableObject messageProvider = null;
        List<DisposableBaseObject> changedList = new List<DisposableBaseObject>();

        private readonly object lockObject = new object();
        private readonly object winLockObject = new object();
        
        #endregion

        #region Properties
        
        public ISupportableObject MessageProvider
        {
            get { return messageProvider; }
            set
            {
                if (messageProvider!=null)
                    messageProvider.DNUUnregisterUpdatableObject(this);

                messageProvider = value;

                if (messageProvider != null)
                {
                    messageProvider.DNURegisterUpdatableObject(this);
                    ForceTotalUpdate();
                }
                else
                    Close();
            }
        }

        #endregion

        #region Methods

        public abstract void ForceTotalUpdate();

        public void Changed(DisposableBaseObject obj)
        {
            //Dispatcher.BeginInvoke(DispatcherPriority.Background, setChanged, this, obj);
            lock (lockObject)
            {
                changedList.Add(obj);
            }
        }

        private readonly InternalUpdateHandler update = delegate(BaseUpdatableWindow buw)
        {
            List<DisposableBaseObject> copy = null;
            lock (buw.lockObject)
            {
                copy = buw.changedList;
                buw.changedList = new List<DisposableBaseObject>();
            }
            lock (buw.winLockObject)
            {
                if (buw.delay != null) //window could be disposed in another thread
                {
                    //buw.delay.Stop();
                    buw.DoUpdate(copy.Distinct());
                    buw.delay.Start();
                }
            }
        };

        protected abstract void DoUpdate(IEnumerable<DisposableBaseObject> changed);

        void Dispose()
        {
            MessageProvider = null;
            lock (lockObject)
            {
                changedList = new List<DisposableBaseObject>();
                //delay.Stop();
                lock (winLockObject)
                {
                    delay.Elapsed -= OnTimedEvent;
                    delay.Dispose();
                    delay = null;
                }
            }
            Loaded -= Window_Loaded;
        }
        #endregion

        #region Delegates and Events

        private readonly UpdateWindowHandler setChanged = delegate(object sender, DisposableBaseObject obj)
        {
            BaseUpdatableWindow wnd = (BaseUpdatableWindow)sender;
            lock (wnd.lockObject)
            {
                wnd.changedList.Add(obj);
            }
        };

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += Window_Loaded;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            lock (winLockObject)
            {
                if (delay != null)
                {
                    if (changedList.Count > 0)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, update, this);
                    }
                    else
                    {
                        delay.Start();
                    }
                }
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            delay = new Timer {Interval = Tick, AutoReset = false};
            delay.Elapsed += OnTimedEvent;
            //delay.Enabled = true;
            delay.Start();
        }

        #endregion
    }
}
