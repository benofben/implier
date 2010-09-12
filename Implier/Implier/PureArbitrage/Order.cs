using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Implier.FIXApplication;
using Implier.Graph;
using Implier.SpreadMatrix;
using QuickFix;
using Message = QuickFix.Message;

namespace Implier.PureArbitrage
{
    internal class OrderController : DisposableBaseObject
    {
        #region Fields
        Dictionary<string, Order> orders = new Dictionary<string, Order>();
        private static OrderController instance = null;
        #endregion

        #region Properties
        public static OrderController Instance
        {
            get { return instance ?? (instance = new OrderController()); }
        }

        public IEnumerable<Order> Orders
        {
            get
            {
                lock (LockObject)
                {
                    return orders.Values.ToList();
                }
            }
        }
        #endregion

        #region Constructor
        protected OrderController() : base() { }
        #endregion

        #region Methods
        public void Register(Order order)
        {
            lock (LockObject)
            {
                if(!orders.ContainsKey(order.ClOrdID))
                    orders.Add(order.ClOrdID, order);
            }
        }

        public void UnRegister(Order order)
        {
            lock (LockObject)
            {
                if (orders.ContainsKey(order.ClOrdID))
                    orders.Remove(order.ClOrdID);
            }
        }

        public void UpdateOrderStatus(QuickFix42.ExecutionReport executionReport)
        {
            lock (LockObject)
            {
                // ClOrdID is not presented when order was changed outside of Fix Adapter
                if (!executionReport.isSetClOrdID())
                    return;

                String ClOrdID = executionReport.getClOrdID().getValue();

                if(orders.ContainsKey(ClOrdID))
                {
                    Order order = orders[ClOrdID];

                    switch(executionReport.getOrdStatus().getValue())
                    {
                        case OrdStatus.NEW:
                            order.OrderStatus = OrderStatus.New;
                            break;
                        case OrdStatus.PARTIALLY_FILLED:
                            order.OrderStatus = OrderStatus.PartiallyFilled;
                            break;
                        case OrdStatus.FILLED:
                            order.OrderStatus = OrderStatus.Filled;
                            break;
                        case OrdStatus.CANCELED:
                            order.OrderStatus = OrderStatus.Canceled;
                            break;
                        case OrdStatus.REJECTED:
                            order.OrderStatus = OrderStatus.Rejected;
                            break;
                        default:
                            throw new Exception("Undefined order state" + executionReport.getOrdStatus().getValue());
                        
                    }
                    order.AddExecutionReport(executionReport);
                }
            }
        }

        public void RequestOrders(IEnumerable<Order> orderList)
        {
            lock (LockObject)
            {
                FixApplication.Current.RequestOrders(orderList.Select(order => order.OrderMessage).ToList());
            }
        }

        protected override void DoDispose()
        {
            orders.Clear();
            //throw new NotImplementedException();
        }
        #endregion
    }

    internal enum OrderStatus
    {
        Init,
        Sending,
        New,
        PartiallyFilled,
        Filled,
        Canceled,
        Replaced,
        PendingCancel,
        Rejected,
        Restated
    }

    internal partial class Order : DisposableBaseObject
    {
        #region Fields
        SecurityEntry weakCopy;
        MDEntryGroup currentMDGroup;
        List<QuickFix42.ExecutionReport> executionReports = new List<QuickFix42.ExecutionReport>();
        OrderStatus orderStatus = OrderStatus.Init;
        #endregion

        #region Properties
        public int ExecutionReportCount
        {
            get
            {
                lock (LockObject)
                {
                    return executionReports.Count();
                }
            }
        }

        public OrderStatus OrderStatus
        {
            get
            {
                lock (LockObject)
                {
                    return orderStatus;
                }
            }
            set
            {
                lock (LockObject)
                {
                    orderStatus = value;
                }
            }
        }

        public Message OrderMessage { get; private set; }
        public string ClOrdID { get { return ((QuickFix42.NewOrderSingle)OrderMessage).getClOrdID().getValue(); } }
        #endregion

        #region Constructor
        public Order(IProposal proposal, double quantity)
        {
            int propIndex = -1;
            SecurityEntry ownerEntry = ((MDEntryGroup) proposal).OwnerEntry;
            propIndex = ownerEntry.GetGroupIndex((MDEntryGroup)proposal);

            weakCopy = ((Proposal) proposal).OwnerEntry.WeakClone();
            currentMDGroup = weakCopy.GetGroup((uint) propIndex);

            OrderMessage = FixApplication.Current.NewOrder(proposal, quantity);
        }

        public Order(MDEntryGroup entryGroup, double quantity)
        {
            int propIndex = -1;
            SecurityEntry ownerEntry = entryGroup.OwnerEntry;
            propIndex = ownerEntry.GetGroupIndex(entryGroup);

            weakCopy = entryGroup.OwnerEntry.WeakClone();
            currentMDGroup = weakCopy.GetGroup((uint)propIndex);

            OrderMessage = FixApplication.Current.NewOrder(entryGroup, quantity);
        }

        #endregion

        #region Methods
        public QuickFix42.ExecutionReport GetExecutionReport(int index)
        {
            lock (LockObject)
            {
                return executionReports[index];
            }
        }

        public void AddExecutionReport(QuickFix42.ExecutionReport executionReport)
        {
            lock (LockObject)
            {
                executionReports.Add(executionReport);    
            }
        }

        protected override void DoDispose()
        {
            weakCopy.Dispose();
            executionReports.Clear();
        }
        #endregion
    }
}
