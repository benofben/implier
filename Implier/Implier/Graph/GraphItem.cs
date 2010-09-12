using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implier.Graph
{
    internal interface IUpdatableObject
    {
        ISupportableObject MessageProvider { get; set; }
        void Changed(DisposableBaseObject obj);
        void ForceTotalUpdate();
    }

    internal interface ISupportableObject
    {
        IEnumerable<IUpdatableObject> UpdatableObjects { get; }
        void DNURegisterUpdatableObject(IUpdatableObject obj);
        void DNUUnregisterUpdatableObject(IUpdatableObject obj);
        IUpdatableObject SearchForUpdatableObject(Func<IUpdatableObject, bool> func);
        void RaizeChanged(DisposableBaseObject changed);
    }
     
    internal abstract class DisposableBaseObject : IDisposable
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
