using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Implier.CommonControls.Windows;
using Implier.Graph;

namespace Implier.FIXApplication
{
    public class WindowSupportableObject: DisposableBaseObject
    {
        readonly List<BaseUpdatableWindow> windows = new List<BaseUpdatableWindow>();
        internal void DNURegisterWindow(BaseUpdatableWindow wnd)
        {
            windows.Add(wnd);
        }
        internal void DNUUnregisterWindow(BaseUpdatableWindow wnd)
        {
            windows.Remove(wnd);
        }
        
        protected override void DoDispose()
        {
            while (windows.Count > 0)
            {
                BaseUpdatableWindow buw = windows[0];
                buw.MessageProvider = null;
                buw.Close();
            }
        }

        internal BaseUpdatableWindow SearchForWindow(Func<BaseUpdatableWindow,bool> func)
        {
            IEnumerable<BaseUpdatableWindow> found = windows.Where(func);
            return found.Count() > 0 ? found.First() : null;
        }

        internal IEnumerable<BaseUpdatableWindow> Windows
        {
            get { return windows; }
        }

        protected void RaizeChanged(DisposableBaseObject changed)
        {
            foreach (BaseUpdatableWindow window in Windows)
            {
                window.Changed(changed);
            }
        }

    }

    //interface IBaseUpdateableWindow
    //{
    //    IWindowSupportable MessageProvider 
    //    { 
    //        get { return messageProvider}            
    //        set
    //        {
    //            if (messageProvider != null)
    //                messageProvider.DNUUnregisterWindow(this);
    //            messageProvider = value;
    //            if (messageProvider != null)
    //                messageProvider.DNURegisterWindow(this);
    //        }
    //    }
    //    void Dispose()
    //    {
    //        if (MessageProvider!=null)
    //            MessageProvider = null;
    //    }

    //}
}
