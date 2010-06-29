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

    public abstract partial class BaseUpdatableWindow : Window
    {
        #region Fields
        Timer delay;
        public const int Tick = 30;
        private WindowSupportableObject messageProvider = null;
        readonly List<DisposableBaseObject> changedList = new List<DisposableBaseObject>();

        private readonly object lockObject = new object();
        
        #endregion

        #region Properties

        internal WindowSupportableObject MessageProvider
        {
            get { return messageProvider; }
            set
            {
                if (messageProvider!=null)
                    messageProvider.DNUUnregisterWindow(this);
                messageProvider = value;
                if (messageProvider != null)
                {
                    messageProvider.DNURegisterWindow(this);
                    ForceTotalUpdate();
                }
            }
        }

        #endregion

        #region Methods

        internal abstract void ForceTotalUpdate();

        internal void Changed(DisposableBaseObject obj)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, setChanged, this, obj);
        }

        private readonly InternalUpdateHandler update = delegate(BaseUpdatableWindow buw)
        {
            lock (buw.lockObject)
            {
                if (buw.delay != null)//window could be disposed in another thread
                {
                    buw.delay.Stop();
                    buw.DoUpdate(
                        buw.changedList.Distinct());
                    buw.changedList.Clear();
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
                delay.Stop();
                delay.Elapsed -= OnTimedEvent;
                delay.Dispose();
                delay = null;
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
            if (changedList.Count>0)
            {
                delay.Stop();
                Dispatcher.BeginInvoke(DispatcherPriority.Background, update, this);
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            delay = new Timer {Interval = Tick, AutoReset = true};
            delay.Elapsed += OnTimedEvent;
            delay.Enabled = true;
            delay.Start();
        }

        #endregion
    }
}
