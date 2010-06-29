using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implier.Graph
{
    public abstract class DisposableBaseObject : IDisposable
    {
        #region Fields
        private object lockObject = new object();
        private bool isDisposed = false;
        #endregion

        #region Properties

        public object LockObject
        {
            get { return lockObject; }
        }

        public bool IsDisposed
        {
            get 
            {
                lock (lockObject)
                {
                    return isDisposed;
                }
            }
        }

        #endregion

        #region Methods


        public void Dispose()
        {
            lock (LockObject)
            {
                if (!isDisposed)
                {
                    DoDispose();
                    isDisposed = true;
                }
            }
        }

        protected abstract void DoDispose();

        #endregion
    }

    public class InvalidArgumentExcetipn : Exception
    {
        public InvalidArgumentExcetipn(string message)
            :base(message)
        {

        }
    }

}
