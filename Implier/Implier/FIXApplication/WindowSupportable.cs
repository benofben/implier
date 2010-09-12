using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Implier.CommonControls.Windows;
using Implier.Graph;

namespace Implier.FIXApplication
{
    internal class SupportableObject : DisposableBaseObject, ISupportableObject
    {
        readonly List<IUpdatableObject> updatableObjects = new List<IUpdatableObject>();

        public IEnumerable<IUpdatableObject> UpdatableObjects
        {
            get { return updatableObjects; }
        }

        public void DNURegisterUpdatableObject(IUpdatableObject obj)
        {
            updatableObjects.Add(obj);
        }

        public void DNUUnregisterUpdatableObject(IUpdatableObject obj)
        {
            updatableObjects.Remove(obj);
        }

        public IUpdatableObject SearchForUpdatableObject(Func<IUpdatableObject, bool> func)
        {
            IEnumerable<IUpdatableObject> found = updatableObjects.Where(func);
            return found.Count() > 0 ? found.First() : null;
        }

        public void RaizeChanged(DisposableBaseObject changed)
        {
            foreach (IUpdatableObject uo in updatableObjects)
            {
                uo.Changed(changed);
            }
        }

        protected override void DoDispose()
        {
            while (updatableObjects.Count > 0)
            {
                IUpdatableObject buo = updatableObjects[0];
                buo.MessageProvider = null;
            }
        }
    }


    /*internal class WindowSupportableObject : SupportableObject
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
            base.DoDispose();

            while (UpdatableObjects.Count() > 0)
            {
                IUpdatableObject uo = UpdatableObjects.ToArray()[0];
                if (uo is BaseUpdatableWindow)
                    ((BaseUpdatableWindow) uo).Close();
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
        

    }*/
    
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
