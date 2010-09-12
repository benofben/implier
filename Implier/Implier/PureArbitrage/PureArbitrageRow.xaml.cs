using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Implier.FIXApplication;
using QuickFix;

namespace Implier.PureArbitrage
{
    [Flags]
    public enum PureArbitrageRowFields
    {
        Exchange,
        Symbol,
        MaxQuantity,
        Price,
        Legs,
        CostPerLeg,
        ProfitPerTrade,
        BuyQuantity,
        Total
    }

    /// <summary>
    /// Interaction logic for PureArbitrageRow.xaml
    /// </summary>
    public partial class PureArbitrageRow : UserControl
    {
        //private enum Fields
        //{
        //    Exchange = 0,
        //    Symbol = 
        //}

        //static List<KeyValuePair<Type, string>> scheme = new List<KeyValuePair<Type, string>>();

        //List<object> values = new List<object>();

        private string exchange = "";
        private string symbol = "";
        private double maxQuantity = 0;
        private double price = 0;
        private int legs = 0;
        private double costPerLeg = 0;
        private double buyQuantity = 0;

        public event RoutedEventHandler RemoveClick;

        #region Properties

        public string Trade { get; set; }
        internal IEnumerable<Order> Orders { get; set; }

        public string Exchange
        {
            get { return exchange; }
            set
            {
                if (exchange == value)
                    return;
                exchange = value;
                txtExchange.Text = value;
            }
        }

        public string Symbol
        {
            get { return symbol; }
            set
            {
                if (symbol == value)
                    return;
                symbol = value;
                txtSymbol.Text = value;
            }
        }

        public double MaxQuantity
        {
            get { return maxQuantity; }
            set
            {
                if (maxQuantity == value)
                    return;
                maxQuantity = value;
                txtMaxQuantity.Text = value.ToString("F0");
            }
        }

        public double Price
        {
            get { return price; }
            set
            {
                if (price == value)
                    return;
                price = value;
                txtPrice.Text = value.ToString(Constants.FloatingPointFormat);
                SetProfitPerTrade();
            }
        }

        public int Legs
        {
            get { return legs; }
            set
            {
                if (legs == value)
                    return;
                legs = value;
                btnLegs.Content = value.ToString();
                SetProfitPerTrade();
            }
        }

        public double CostPerLeg
        {
            get { return costPerLeg; }
            set
            {
                if (costPerLeg == value)
                    return;
                costPerLeg = value;
                txtCostPerLeg.Text = value.ToString(Constants.FloatingPointFormat);
                SetProfitPerTrade();
            }
        }

        public double BuyQuantity
        {
            get { return buyQuantity; }
            set
            {
                if (buyQuantity == value)
                    return;
                buyQuantity = value;
                txtBuyQuantity.Text = value.ToString("F0");
                SetTotal();
            }
        }

        public double ProfitPerTrade
        {
            get { return price - legs * costPerLeg; }
        }

        public double Total
        {
            get { return ProfitPerTrade * BuyQuantity; }
        }

        #endregion

        public PureArbitrageRow()
        {
            InitializeComponent();

            Exchange = "";
            Symbol = "";
            MaxQuantity = 0;
            Price = 0;
            Legs = 0;
            CostPerLeg = 0;
            BuyQuantity = 0;
            Trade = "";
        }

        void SetProfitPerTrade()
        {
            txtProfitPerTrade.Text = ProfitPerTrade.ToString(Constants.FloatingPointFormat);
            SetTotal();
        }

        void SetTotal()
        {
            txtTotal.Text = Total.ToString(Constants.FloatingPointFormat);
        }

        private void txtCostPerLeg_TextChanged(object sender, TextChangedEventArgs e)
        {
            Double value;
            Double.TryParse(((TextBox) sender).Text, out value);
            CostPerLeg = value;
        }

        private void txtBuyQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            int.TryParse(((TextBox) sender).Text, out value);
            BuyQuantity = value;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtCostPerLeg.TextChanged +=  txtCostPerLeg_TextChanged;
            txtBuyQuantity.TextChanged += txtBuyQuantity_TextChanged;
            btnLegs.Click += btnLegs_Click;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            txtCostPerLeg.TextChanged -= txtCostPerLeg_TextChanged;
            txtBuyQuantity.TextChanged -= txtBuyQuantity_TextChanged;
            btnLegs.Click -= btnLegs_Click;
        }

        private void btnLegs_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(Trade);

            PureArbitrageMLEGGrid grid = new PureArbitrageMLEGGrid();
            grid.FillDataGrid(Orders);
            grid.dataGrid.Columns[grid.dataGrid.Columns.Count - 1].Visibility = Visibility.Collapsed;
            grid.dataGrid.Columns[grid.dataGrid.Columns.Count - 2].Visibility = Visibility.Collapsed;

            grid.Title = "Legs - " + Symbol + " - Total:" + Total.ToString(Constants.FloatingPointFormat);
            grid.ShowDialog();
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button) sender;
            btn.IsEnabled = false;
            btn.Content = " Buying... ";

            foreach (Order order in Orders)
                order.OrderStatus = OrderStatus.Sending;

            OrderController.Instance.RequestOrders(Orders);
        }

        private void btnRes_Click(object sender, RoutedEventArgs e)
        {
            PureArbitrageMLEGGrid grid = new PureArbitrageMLEGGrid();
            grid.Title = "Trade Result - " + Symbol + " - Total:" + Total.ToString(Constants.FloatingPointFormat);
            grid.FillDataGrid(Orders);
            grid.ShowDialog();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveClick != null)
                RemoveClick(this, e);
        }
    }
}
