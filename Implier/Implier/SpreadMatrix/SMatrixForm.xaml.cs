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
    public partial class SMatrixForm : BaseUpdatableWindow
    {
        public SMatrixForm()
        {
            InitializeComponent();   
        }

        protected override void DoUpdate(IEnumerable<DisposableBaseObject> changed)
        {
            sMatrix1.FillGrid((SpreadMatrixData)MessageProvider, changed.Cast<MDEntry>());
        }

        internal override void ForceTotalUpdate()
        {
            SpreadMatrixData smd = (SpreadMatrixData)MessageProvider;

            lock (smd.LockObject)
            {
                foreach (MDEntry value in smd.Values)
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
            }
        }

    }
}
