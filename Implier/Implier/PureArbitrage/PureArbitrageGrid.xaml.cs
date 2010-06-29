using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
using Implier.CommonControls;
using Implier.FIXApplication;
using Implier.Graph;
using Implier.SpreadMatrix;

namespace Implier.PureArbitrage
{
    /// <summary>
    /// Interaction logic for PureArbitrageGrid.xaml
    /// </summary>
    public partial class PureArbitrageGrid : UserControl
    {
        private PureArbitrageRowFields activeSortField;
        private bool isAscSortOrder = false;

        public PureArbitrageGrid()
        {
            InitializeComponent();

            gridHeader1.btnHeader.Content = "Exchange";
            gridHeader2.btnHeader.Content = "Symbol";
            gridHeader3.btnHeader.Content = "Max Quantity";
            gridHeader4.btnHeader.Content = "Price";
            gridHeader5.btnHeader.Content = "Legs";
            gridHeader6.btnHeader.Content = "$Cost/Leg";
            gridHeader7.btnHeader.Content = "$Profit/Trade";
            gridHeader8.btnHeader.Content = "Buy Quantity";
            gridHeader9.btnHeader.Content = "Total";
            //gridHeader10.btnHeader.Content = "";

            activeSortField = PureArbitrageRowFields.Exchange;
            isAscSortOrder = true;
        }

        public void Clear()
        {
            foreach (IDisposable child in stackPanel.Children.OfType<IDisposable>())
            {
                child.Dispose();
            }
            stackPanel.Children.Clear();
        }

        public void Resort()
        {
            Sort(activeSortField, isAscSortOrder);
        }

        private void Sort(PureArbitrageRowFields field, bool isAscSortOrder)
        {
            List<PureArbitrageRow> rows = new List<PureArbitrageRow>(stackPanel.Children.Count);
            
            rows.AddRange(from UIElement child in stackPanel.Children select child as PureArbitrageRow);
            IComparer<PureArbitrageRow> comparer = new ExchangeComparer();
            switch (field)
            {
                case PureArbitrageRowFields.Exchange:
                    comparer = new ExchangeComparer();
                    break;

                case PureArbitrageRowFields.Symbol:
                    comparer = new SymbolComparer();
                    break;

                case PureArbitrageRowFields.MaxQuantity:
                    comparer = new MaxQuantityComparer();
                    break;

                case PureArbitrageRowFields.Price:
                    comparer = new PriceComparer();
                    break;

                case PureArbitrageRowFields.Legs:
                    comparer = new LegsComparer();
                    break;

                case PureArbitrageRowFields.CostPerLeg:
                    comparer = new CostPerLegComparer();
                    break;

                case PureArbitrageRowFields.ProfitPerTrade:
                    comparer = new ProfitPerTradeComparer();
                    break;

                case PureArbitrageRowFields.BuyQuantity:
                    comparer = new BuyQuantityComparer();
                    break;

                case PureArbitrageRowFields.Total:
                    comparer = new TotalComparer();
                    break;

                default:
                    break;
            }

            rows.Sort(comparer);

            stackPanel.Children.Clear();

            for (int i = 0; i < rows.Count; i++)
            {
                PureArbitrageRow arbitrageRow = rows[isAscSortOrder ? i : rows.Count - 1 - i];
                stackPanel.Children.Add(arbitrageRow);
            }

            rows.Clear();
        }

        public void AddRow(PureArbitrageRow row)
        {
            if (row.Total > 0)
                SystemSounds.Beep.Play();
            
            stackPanel.Children.Add(row);
            row.RemoveClick += rowBtnRemove_Click;

            foreach (Order order in row.Orders)
                OrderController.Instance.Register(order);

            //Resort();
        }

        internal void RemoveRow(PureArbitrageRow row)
        {
            row.RemoveClick -= rowBtnRemove_Click;

            foreach (Order order in row.Orders)
                OrderController.Instance.UnRegister(order);

            stackPanel.Children.Remove(row);
        }

        internal void RemoveRows(IEnumerable<PureArbitrageRow> rows)
        {
            foreach (PureArbitrageRow row in rows.ToList())
                RemoveRow(row);
        }

        internal IEnumerable<PureArbitrageRow> SelectRows(Func<PureArbitrageRow,bool> func)
        {
            return stackPanel.Children.Cast<object>().Where(o => o is PureArbitrageRow).Cast<PureArbitrageRow>().Where(func);
        }

        internal static PureArbitrageRow NewRow(Trade trade, SpreadMatrixData spreadMatrixData)
        {
            PureArbitrageRow row = new PureArbitrageRow();

            row.Exchange = spreadMatrixData.Exchange;
            row.Symbol = spreadMatrixData.Symbol;
            row.MaxQuantity = trade.Quantity;
            row.Legs = trade.Path.Count();
            row.Price = trade.Proposals.Sum(proposal => proposal.Price);
            row.CostPerLeg = Trade.DealCost;
            row.BuyQuantity = row.MaxQuantity;
            row.Trade = trade.ToString();

            row.Orders = trade.Proposals.Select(proposal => new Order(proposal, trade.Quantity)).ToList();

            return row;
        }

        private void rowBtnRemove_Click(object sender, RoutedEventArgs e)
        {
            PureArbitrageRow row = (PureArbitrageRow) sender;
            RemoveRow(row);
        }

        private void gridHeader_Click(object sender, RoutedEventArgs e)
        {
            PureArbitrageRowFields newSortField = activeSortField;
            switch (((GridHeader) sender).Name)
            {
                case "gridHeader1":
                    newSortField = PureArbitrageRowFields.Exchange;
                    break;
                case "gridHeader2":
                    newSortField = PureArbitrageRowFields.Symbol;
                    break;
                case "gridHeader3":
                    newSortField = PureArbitrageRowFields.MaxQuantity;
                    break;
                case "gridHeader4":
                    newSortField = PureArbitrageRowFields.Price;
                    break;
                case "gridHeader5":
                    newSortField = PureArbitrageRowFields.Legs;
                    break;
                case "gridHeader6":
                    newSortField = PureArbitrageRowFields.CostPerLeg;
                    break;
                case "gridHeader7":
                    newSortField = PureArbitrageRowFields.ProfitPerTrade;
                    break;
                case "gridHeader8":
                    newSortField = PureArbitrageRowFields.BuyQuantity;
                    break;
                case "gridHeader9":
                    newSortField = PureArbitrageRowFields.Total;
                    break;
            }

            if(newSortField!=activeSortField)
            {
                activeSortField = newSortField;
                isAscSortOrder = true;
            }
            else
            {
                isAscSortOrder = !isAscSortOrder;
            }

            Sort(activeSortField, isAscSortOrder);
        }
    }

    #region Comparers
    class ExchangeComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return Utils.CompareStrings(x.Exchange, y.Exchange);
        }
    }
    
    class SymbolComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return Utils.CompareStrings(x.Symbol, y.Symbol);
        }
    }

    class MaxQuantityComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return x.MaxQuantity.CompareTo(y.MaxQuantity);
        }
    }

    class PriceComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return x.Price.CompareTo(y.Price);
        }
    }

    class LegsComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return x.Legs.CompareTo(y.Legs);
        }
    }

    class CostPerLegComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return x.CostPerLeg.CompareTo(y.CostPerLeg);
        }
    }

    class ProfitPerTradeComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return x.ProfitPerTrade.CompareTo(y.ProfitPerTrade);
        }
    }

    class BuyQuantityComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return x.BuyQuantity.CompareTo(y.BuyQuantity);
        }
    }

    class TotalComparer : IComparer<PureArbitrageRow>
    {
        public int Compare(PureArbitrageRow x, PureArbitrageRow y)
        {
            return x.Total.CompareTo(y.Total);
        }
    }
    #endregion

}
