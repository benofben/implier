using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Implier.CommonControls.Windows;
using Implier.FIXApplication;
using QuickFix42;

namespace Implier.SpreadMatrix
{
    internal class SpreadMatrixCollection : SupportableObject
    {
        #region Fields
        readonly DualKeyDictionary<string, string,SpreadMatrixData> matrices = new DualKeyDictionary<string, string, SpreadMatrixData>();
        #endregion

        #region Methods

        internal void ProcessMessage(string exchange, string symbol, MarketDataSnapshotFullRefresh entry)
        {            
            Get(exchange, symbol).Update(entry);
            RaizeChanged(Get(exchange, symbol));
        }

        internal void Add(string exchange, string symbol)
        {
            lock (LockObject)
            {
                SpreadMatrixData spreadMatrixData = new SpreadMatrixData(exchange, symbol);
                matrices.SetValue(exchange, symbol, spreadMatrixData);
                RaizeChanged(spreadMatrixData);
            }
        }
        internal SpreadMatrixData Get(string exchange, string symbol)
        {
            lock (LockObject)
            {
                return matrices.GetValue(exchange, symbol);
            }
        }

        internal void Remove(string exchange, string symbol)
        {
            lock (LockObject)
            {
                SpreadMatrixData spreadMatrixData = matrices.GetValue(exchange, symbol);

                //foreach (SecurityEntry securityEntry in spreadMatrixData.Values)
                //    FixApplication.Current.MarketDataRequestReject(securityEntry.MDReqID);

                spreadMatrixData.Dispose();
                matrices.Remove(exchange, symbol);
                RaizeChanged(spreadMatrixData);
            }
        }

        internal void Clear()
        {
            lock(LockObject)
            {
                foreach (SpreadMatrixData value in matrices.Values)
                {
                    if (value!=null)
                        value.Dispose();
                }
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            //lock object is locked in base class already
            foreach (SpreadMatrixData value in matrices.Values)
            {
                value.Dispose();
            }
        }

        #endregion

        #region Properties
        internal bool IsEmpty
        {
            get 
            { 
                return First == null;
            }
        }

        internal SpreadMatrixData First
        {
            get
            {
                lock (LockObject)
                {
                    IEnumerable<SpreadMatrixData> values = matrices.Values;
                    return values.Count() == 0 ? null : values.First();
                }
            }
        }


        /// <summary>
        /// Do not forget to lock SpreadMatrixCollection.LockObject while operating with values
        /// </summary>
        internal IEnumerable<SpreadMatrixData> Values
        {
            get
            {
                lock (LockObject)
                {
                    return matrices.Values;
                }
            }
        }
        #endregion
    }
}
