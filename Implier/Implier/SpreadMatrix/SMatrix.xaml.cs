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
using Implier.PureArbitrage;
using QuickFix;
using QuickFix42;
using System.Windows.Threading;
using Implier.SpreadMatrix;

namespace Implier
{
    /// <summary>
    /// Interaction logic for SMatrix.xaml
    /// </summary>
    internal partial class SMatrix : UserControl
    {
        #region Fields
        const int CellWidth = 110;
        const int CellHeight = 45;
        
        DualKeyDictionary<int, int, UIElement> cellPosDictionary = new DualKeyDictionary<int, int, UIElement>();
        DualKeyDictionary<int, int, UIElement> headerPosDictionary = new DualKeyDictionary<int, int, UIElement>();

        #endregion

        #region Properties
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        #endregion 

        #region Constructor
        public SMatrix()
        {
            InitializeComponent();
        }
        #endregion

        static int GetDeltaMonth(DateTime fromDate, DateTime ToDate)
        {
            int dYear = ToDate.Year - fromDate.Year;
            int dMonth = dYear * 12 - (fromDate.Month - 1) + ToDate.Month;

            return dMonth;
        }

        private SecurityEntry GetSecurity(SpreadMatrixData smd, SpreadMatrixDataCell cell)
        {
            int r = Grid.GetRow(cell);
            int c = Grid.GetColumn(cell);
            if (r > c)
                r--;

            DateTime date1 = smd.MinYearMonth.AddMonths(r);
            DateTime date2 = smd.MinYearMonth.AddMonths(c);

            MDDatePair datePair = new MDDatePair(date1, date2);

            return smd.Get(datePair);
        }

        public void UpdateGrid(SpreadMatrixData smd, SecurityEntry entry)
        {
            if (!smd.Values.Contains(entry))
                return;

            MDDatePair datePair = entry.GetDatePair();

            int dMonth1 = GetDeltaMonth(smd.MinYearMonth, datePair.GetMinDate());
            int dMonth2 = GetDeltaMonth(smd.MinYearMonth, datePair.GetMaxDate());
            int r = dMonth1 - 1;
            int c = dMonth2 - 1;
            
            SpreadMatrixDataCell cell = (SpreadMatrixDataCell)cellPosDictionary.GetValue(r, c);
            
            if(cell==null)
            {
                cell = new SpreadMatrixDataCell { Width = CellWidth, Height = CellHeight };
                
                cell.MouseDown += cell_MouseDown;

                Grid.SetRow(cell, r == c ? r + 1 : r);
                Grid.SetColumn(cell, c);
                
                cellPosDictionary.SetValue(r, c, cell);
                grid.Children.Add(cell);
            }

            cell.FillData(entry);  
        }

        void ClearGrid()
        {
            cellPosDictionary = new DualKeyDictionary<int, int, UIElement>();
            headerPosDictionary = new DualKeyDictionary<int,int, UIElement>();
            
            gridHeader.Children.Clear();
            gridHeader.ColumnDefinitions.Clear();
            
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
        }

        public void CreateGrid(SpreadMatrixData smd)
        {
            ClearGrid();

            int dMonth = GetDeltaMonth(smd.MinYearMonth, smd.MaxYearMonth);

            if (smd.MaxYearMonth == new DateTime())
                return;

            for (int i = 0; i < dMonth; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(CellWidth);
                grid.ColumnDefinitions.Add(col);                
            }

            for (int i = 0; i < dMonth + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(CellHeight);
                grid.RowDefinitions.Add(row);
            }

            for (int c = 0; c < dMonth + 1; c++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(CellWidth);
                gridHeader.ColumnDefinitions.Add(col);

                SpreadMatrixCell cellHeaderDate = new SpreadMatrixCell
                                                     {
                                                         Width = CellWidth,
                                                         Height = CellHeight/2
                                                     };
                cellHeaderDate.FillData(smd.MinYearMonth.AddMonths(c));

                Grid.SetRow(cellHeaderDate, 2);
                Grid.SetColumn(cellHeaderDate, c);

                SpreadMatrixDataCell cellHeader = new SpreadMatrixDataCell
                                             {
                                                 Width = CellWidth,
                                                 Height = CellHeight,
                                                 //AskPx = "?",
                                                 //AskCount = "?",
                                                 //BidPx = "?",
                                                 //BidCount = "?"
                                             };

                Grid.SetRow(cellHeader, 1);
                Grid.SetColumn(cellHeader, c);

                SpreadMatrixCell cellHeaderElm = new SpreadMatrixCell { Width = CellWidth, Height = CellHeight };

                Grid.SetRow(cellHeaderElm, 0);
                Grid.SetColumn(cellHeaderElm, c);
                cellHeaderElm.FillData("?");

                gridHeader.Children.Add(cellHeaderDate);
                gridHeader.Children.Add(cellHeader);
                gridHeader.Children.Add(cellHeaderElm);
            }

            for (int r = 0; r < dMonth; r++)
            {
                SpreadMatrixCell cellDate = new SpreadMatrixCell { Width = CellWidth, Height = CellHeight };

                Grid.SetRow(cellDate, r);
                Grid.SetColumn(cellDate, r);
                cellDate.FillData(smd.MinYearMonth.AddMonths(r));
                grid.Children.Add(cellDate);

                for (int c = r; c < dMonth; c++)
                {
                    MDDatePair datePair = new MDDatePair(smd.MinYearMonth.AddMonths(r), smd.MinYearMonth.AddMonths(c));
                    SpreadMatrixDataCell cell = smd.GetDataCell(datePair) ?? new SpreadMatrixDataCell();

                    cell.MouseDown += cell_MouseDown;

                    cell.Width = CellWidth;
                    cell.Height = CellHeight;
                    Grid.SetRow(cell, r == c ? r + 1 : r);
                    Grid.SetColumn(cell, c);
                    cellPosDictionary.SetValue(r, c, cell);
                    grid.Children.Add(cell);
                }
            }
            InvalidateVisual();
        }

        private void cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SpreadMatrixDataCell cell = (SpreadMatrixDataCell) sender;
            SpreadMatrixData smd = FixApplication.Current.SpreadMatrixCollection.Get(Exchange, Symbol);
            SecurityEntry entry = GetSecurity(smd, cell);
            if (entry == null)
                return;

            Point pos = e.GetPosition(cell);
            char side = pos.Y > cell.Height/2 ? Side.SELL : Side.BUY;

            if (side == Side.SELL && entry.GetBidGroup() == null || side == Side.BUY && entry.GetAskGroup() == null)
                return;
            
            cell.ContextMenu = new ContextMenu();
            MenuItem mi = new MenuItem();

            if (side == Side.SELL)
            {
                mi.Header = "Sell 1";
                mi.Click += mi_SellClick;
            }
            else
            {
                mi.Header = "Buy 1";
                mi.Click += mi_BuyClick;
            }
            mi.Tag = entry;

            cell.ContextMenu.Items.Add(mi);
        }

        void mi_SellClick(object sender, RoutedEventArgs e)
        {
            SecurityEntry entry = (SecurityEntry) ((MenuItem) sender).Tag;

            Order order = new Order(entry.GetBidGroup(), 1);

            FixApplication.Current.RequestOrders(new[] { order.OrderMessage });
        }

        void mi_BuyClick(object sender, RoutedEventArgs e)
        {
            SecurityEntry entry = (SecurityEntry)((MenuItem)sender).Tag;

            Order order = new Order(entry.GetAskGroup(), 1);

            FixApplication.Current.RequestOrders(new[] {order.OrderMessage});
        }

        public void FillGrid(SpreadMatrixData smd, SecurityEntry entry)
        {
            int dMonth = GetDeltaMonth(smd.MinYearMonth, smd.MaxYearMonth);
            
            if (grid.ColumnDefinitions.Count != dMonth)
                CreateGrid(smd);
            else
                UpdateGrid(smd, entry);
        }

        public void FillGrid(SpreadMatrixData smd, IEnumerable<SecurityEntry> entries)
        {
            int dMonth = GetDeltaMonth(smd.MinYearMonth, smd.MaxYearMonth);

            if (grid.ColumnDefinitions.Count != dMonth)
                CreateGrid(smd);
            else
            {
                foreach (SecurityEntry entry in entries.Where(entry => !entry.IsDisposed))
                    UpdateGrid(smd, entry);
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scrlHeader.ScrollToHorizontalOffset(e.HorizontalOffset);
        }
    }
}
