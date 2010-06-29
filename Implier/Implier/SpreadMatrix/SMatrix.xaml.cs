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
using QuickFix42;
using System.Windows.Threading;
using Implier.SpreadMatrix;

namespace Implier
{
    /// <summary>
    /// Interaction logic for SMatrix.xaml
    /// </summary>
    public partial class SMatrix : UserControl
    {
        #region Fields
        const int CellWidth = 140;
        const int CellHeight = 50;
        
        DualKeyDictionary<int, int, UIElement> cellPosDictionary = new DualKeyDictionary<int, int, UIElement>();
        DualKeyDictionary<int, int, UIElement> headerPosDictionary = new DualKeyDictionary<int, int, UIElement>();
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

        public void UpdateGrid(SpreadMatrixData smd, MDEntry entry)
        {
            MDDatePair datePair = entry.GetDatePair();

            int dMonth1 = GetDeltaMonth(smd.MinYearMonth, datePair.Date1);
            int dMonth2 = GetDeltaMonth(smd.MinYearMonth, datePair.Date2);

            SMatrixCell cell = (SMatrixCell)cellPosDictionary.GetValue(dMonth1 - 1, dMonth2 - 1);

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
                col.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(col);                
            }

            for (int i = 0; i < dMonth + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                grid.RowDefinitions.Add(row);
            }

            for (int c = 0; c < dMonth + 1; c++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(120 + 1 + 1);
                gridHeader.ColumnDefinitions.Add(col);

                SMatrixDateCell cellHeaderDate = new SMatrixDateCell();                
                cellHeaderDate.lblMonthYear.Content = smd.MinYearMonth.AddMonths(c).ToString("MMMyy");
                Grid.SetRow(cellHeaderDate, 2);
                Grid.SetColumn(cellHeaderDate, c);

                SMatrixCell cellHeader = new SMatrixCell();
                cellHeader.lblAskPx.Content = "?";
                cellHeader.lblAskCount.Content = "?";
                cellHeader.lblBidPx.Content = "?";
                cellHeader.lblBidCount.Content = "?";

                Grid.SetRow(cellHeader, 1);
                Grid.SetColumn(cellHeader, c);

                SMatrixElement cellHeaderElm = new SMatrixElement();
                Grid.SetRow(cellHeaderElm, 0);
                Grid.SetColumn(cellHeaderElm, c);
                cellHeaderElm.lbl.Content = "?";

                if (c == 0)
                {
                    //cellHeaderDate.BorderThickness = new Thickness(1, 0, 1, 1);
                    //cellHeader.BorderThickness = new Thickness(1, 0, 1, 1);
                    //cellHeaderElm.BorderThickness = new Thickness(1, 0, 1, 1);
                }

                gridHeader.Children.Add(cellHeaderDate);
                gridHeader.Children.Add(cellHeader);
                gridHeader.Children.Add(cellHeaderElm);
            }

            for (int r = 0; r < dMonth; r++)
            {
                SMatrixDateCell cellDate = new SMatrixDateCell();
                Grid.SetRow(cellDate, r);
                Grid.SetColumn(cellDate, r);
                cellDate.lblMonthYear.Content = smd.MinYearMonth.AddMonths(r).ToString("MMMyy");
                grid.Children.Add(cellDate);

                /*
                 if (datePair.Date1 == datePair.Date2)
                    cell.lblYearMonth.Content = datePair.Date1.ToString("yyyy MMMM");
                else
                    cell.lblYearMonth.Content = datePair.Date1.ToString("yyyy MMM") + " / " + datePair.Date2.ToString("yyyy MMM");
                */

                for (int c = r; c < dMonth; c++)
                {
                    SMatrixCell cell = new SMatrixCell();
                    Grid.SetRow(cell, r == c ? r + 1 : r);
                    Grid.SetColumn(cell, c);

                    //if (c == r)
                    //    cell.BorderThickness = new Thickness(1, 0, 1, 1);

                    cellPosDictionary.SetValue(r, c, cell);

                    MDDatePair datePair = new MDDatePair(smd.MinYearMonth.AddMonths(r), smd.MinYearMonth.AddMonths(c));                    
                    //cell.lblYearMonth.Content = datePair.Date1.ToString("yyyy MMM") + " / " + datePair.Date2.ToString("yyyy MMM");
                    
                    MDEntry curEntry = smd.GetWeakCopy(datePair);

                    if (curEntry != null)
                    {
                        cell.FillData(curEntry);
                    }

                    grid.Children.Add(cell);                    
                }
                InvalidateVisual();
            }
        }

        public void FillGrid(SpreadMatrixData smd, MDEntry entry)
        {
            int dMonth = GetDeltaMonth(smd.MinYearMonth, smd.MaxYearMonth);
            
            if (grid.ColumnDefinitions.Count != dMonth)
                CreateGrid(smd);
            else
                UpdateGrid(smd, entry);
        }

        public void FillGrid(SpreadMatrixData smd, IEnumerable<MDEntry> entries)
        {
            int dMonth = GetDeltaMonth(smd.MinYearMonth, smd.MaxYearMonth);

            if (grid.ColumnDefinitions.Count != dMonth)
                CreateGrid(smd);
            else
            {
                foreach (MDEntry entry in entries.Where(entry => !entry.IsDisposed))
                    UpdateGrid(smd, entry);
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scrlHeader.ScrollToHorizontalOffset(e.HorizontalOffset);
        }
    }
}
