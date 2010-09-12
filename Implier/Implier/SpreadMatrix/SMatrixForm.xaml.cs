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
using System.Windows.Shapes;
using Implier.CommonControls;
using Implier.CommonControls.Windows;
using Implier.FIXApplication;
using Implier.Graph;
using Implier.SpreadMatrix;
using QuickFix42;
using System.Windows.Threading;
using System.Timers;
using System.Collections;

namespace Implier
{
    /// <summary>
    /// Interaction logic for SMatrixForm.xaml
    /// </summary>
    internal partial class SMatrixForm : BaseUpdatableWindow
    {
        public SMatrixForm()
        {
            InitializeComponent();
            PreviewMouseWheel += SMatrixForm_PreviewMouseWheel;
        }

        void SMatrixForm_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                sMatrix1.scrlGrid.ScrollToHorizontalOffset(sMatrix1.scrlGrid.HorizontalOffset - e.Delta);
            }
            else
            {
                sMatrix1.scrlGrid.ScrollToVerticalOffset(sMatrix1.scrlGrid.VerticalOffset - e.Delta);
            }
            e.Handled = true;
        }

        protected override void DoUpdate(IEnumerable<DisposableBaseObject> changed)
        {
            sMatrix1.FillGrid((SpreadMatrixData)MessageProvider, changed.Cast<SecurityEntry>());
        }

        public override void ForceTotalUpdate()
        {
            SpreadMatrixData smd = (SpreadMatrixData)MessageProvider;

            lock (smd.LockObject)
            {
                foreach (SecurityEntry value in smd.Values)
                {
                    Changed(value);
                }
            }
        }

        private void BaseUpdatableWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SpreadMatrixData smd = (SpreadMatrixData)MessageProvider;
            lock (smd.LockObject)
            {
                Title = smd.Exchange + "/" + smd.Symbol;
                sMatrix1.Exchange = smd.Exchange;
                sMatrix1.Symbol = smd.Symbol;
            }
        }

    }
}
