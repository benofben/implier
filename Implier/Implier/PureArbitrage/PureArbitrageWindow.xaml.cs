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
using System.Windows.Shapes;
using System.Windows.Threading;
using Implier.CommonControls.Windows;
using Implier.FIXApplication;
using Implier.Graph;
using Implier.PureArbitrage;
using Implier.SpreadMatrix;

namespace Implier
{
    /// <summary>
    /// Interaction logic for PureArbitrageWindow.xaml
    /// </summary>
    internal partial class PureArbitrageWindow : BaseUpdatableWindow
    {
        private bool onlyProfitable = false;
        public PureArbitrageWindow()
        {
            InitializeComponent();

            btnMode.Content = (!onlyProfitable) ? "Show Only Profitable" : "Show All";
        }

        protected override void DoUpdate(IEnumerable<DisposableBaseObject> changed)
        {
            changed = changed.Distinct();

            TimeSpan algTime = new TimeSpan();
            TimeSpan guiTime = new TimeSpan();

            foreach (PureArbitrageRow row in pureArbitrageGrid.SelectRows(row => row.Orders.All(order => order.OrderStatus != OrderStatus.Init)).ToList())
            {
                if(row.Orders.Any(order => order.OrderStatus == OrderStatus.Rejected))
                    row.btnRes.Content = "Rejected";
                else if(row.Orders.Any(order => order.OrderStatus == OrderStatus.Canceled))
                    row.btnRes.Content = "Failed";
                else if (row.Orders.All(order => order.OrderStatus == OrderStatus.Filled))
                    row.btnRes.Content = "Success";

                row.btnBuy.Visibility = Visibility.Hidden;
                row.btnRes.Visibility = Visibility.Visible;
                row.btnRemove.Visibility = Visibility.Visible;
            }

            foreach (SpreadMatrixData smd in changed)
            {
                pureArbitrageGrid.RemoveRows(
                    pureArbitrageGrid.SelectRows(
                        row =>
                        (row.Symbol == smd.Symbol && row.Exchange == smd.Exchange &&
                         row.Orders.All(order => order.OrderStatus == OrderStatus.Init))));

                if (!smd.IsDisposed)
                {
                    TimeSpan alg = new TimeSpan();
                    List<PureArbitrageRow> deals = smd.RunSimpleTest(ref alg);
                    algTime += alg;

                    DateTime dt1 = DateTime.Now;
                    foreach (PureArbitrageRow row in deals)
                    {
                        if (onlyProfitable && row.Total > 0 || !onlyProfitable)
                            pureArbitrageGrid.AddRow(row);
                    }
                    DateTime dt2 = DateTime.Now;
                    guiTime += (dt2 - dt1);
                }
            }
            pureArbitrageGrid.Resort();

            btnRunAlg.Content = "Run Algorithm (ALG=" + algTime.ToString() + ", GUI=" + guiTime.ToString() + ")";
        }

        private void BaseUpdatableWindow_Closed(object sender, EventArgs e)
        {
            pureArbitrageGrid.Clear();
        }

        public override void ForceTotalUpdate()
        {            
            SpreadMatrixCollection smc = (SpreadMatrixCollection)MessageProvider;
            pureArbitrageGrid.Clear();
            lock(smc.LockObject)
            {
                foreach (SpreadMatrixData value in smc.Values)
                {
                    Changed(value);
                }
            }
        }

        private void btnMode_Click(object sender, RoutedEventArgs e)
        {
            onlyProfitable = !onlyProfitable;

            ((Button) sender).Content = (!onlyProfitable) ? "Show Only Profitable" : "Show All";
            ForceTotalUpdate();
        }
    }
}
