using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuickFix;

namespace Implier.PureArbitrage
{
    internal partial class Order
    {
        #region Properties
        public string SecurityType { get { return ((QuickFix42.NewOrderSingle)OrderMessage).getSecurityType().getValue(); } }
        public string Price { get { return ((QuickFix42.NewOrderSingle)OrderMessage).getPrice().getValue().ToString("F2"); } }
        public string Quantity { get { return ((QuickFix42.NewOrderSingle)OrderMessage).getOrderQty().getValue().ToString("F0"); } }
        public string SideText { get { return ((QuickFix42.NewOrderSingle)OrderMessage).getSide().getValue() == Side.BUY ? "Buy" : "Sell"; } }
        public string SecurityAltID { get { return weakCopy.SecurityAltID; } }
        public string Dates
        {
            get
            {
                return weakCopy.GetDatePair().Date1 == weakCopy.GetDatePair().Date2
                    ? weakCopy.GetDatePair().Date1.ToString("MMMyy")
                    : weakCopy.GetDatePair().Date1.ToString("MMMyy") + "/" + weakCopy.GetDatePair().Date2.ToString("MMMyy");
            }
        }
        public string EntryType { get { return weakCopy.GetGroup((uint)curGroupIndex).EntryType == MDEntryType.BID ? "Bid" : "Ask"; } }
        public string TotalQuantity { get { return weakCopy.GetGroup((uint)curGroupIndex).EntrySize.ToString("F2"); } }
        public string OrderStatusText { get { return Enum.GetName(typeof(OrderStatus), OrderStatus); } }

        public string BidQuantity { get { return weakCopy.GetBidGroup() != null ? weakCopy.GetBidGroup().EntrySize.ToString("F0") : "NaN"; } }
        public string AskQuantity { get { return weakCopy.GetAskGroup() != null ? weakCopy.GetAskGroup().EntrySize.ToString("F0") : "NaN"; } }

        public Brush BidCellForeground
        {
            get
            {
                return
                    new SolidColorBrush(weakCopy.GetGroup((uint) curGroupIndex).EntryType == MDEntryType.BID
                                            ? Colors.DarkBlue
                                            : Colors.LightGray);
            }
        }

        public Brush AskCellForeground
        {
            get
            {
                return
                    new SolidColorBrush(weakCopy.GetGroup((uint)curGroupIndex).EntryType == MDEntryType.OFFER
                                            ? Colors.DarkRed
                                            : Colors.LightGray);
            }
        }

        public string BidPrice { get { return weakCopy.GetBidGroup() != null ? weakCopy.GetBidGroup().EntryPx.ToString("F2") : "NaN"; } }
        public string AskPrice { get { return weakCopy.GetAskGroup() != null ? weakCopy.GetAskGroup().EntryPx.ToString("F2") : "NaN"; } }
        #endregion
    }

    /// <summary>
    /// Interaction logic for PureArbitrageMLEGGrid.xaml
    /// </summary>
    internal partial class PureArbitrageMLEGGrid : Window
    {
        public PureArbitrageMLEGGrid()
        {
            InitializeComponent();
        }

        private ObservableCollection<Order> GetData(IEnumerable<Order> orders)
        {
            ObservableCollection<Order> observableCollection =
                new ObservableCollection<Order>(orders);
            return observableCollection;
        }

        public void FillDataGrid(IEnumerable<Order> orders)
        {
            ObservableCollection<Order> orderdata = GetData(orders);

            //Bind the DataGrid to the order data
            dataGrid.DataContext = orderdata;
        }
    }
}
