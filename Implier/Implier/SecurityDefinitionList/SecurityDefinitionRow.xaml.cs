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
using Implier.SpreadMatrix;

namespace Implier.SecurityDefinitionList
{
    /// <summary>
    /// Interaction logic for SecurityDefinitionRow.xaml
    /// </summary>
    public partial class SecurityDefinitionRow : UserControl
    {
        internal SpreadMatrixData SpreadMatrixData { get; set; }

        public event RoutedEventHandler SpreadMatrixShowPressed;
        public event RoutedEventHandler RemovePressed;

        public SecurityDefinitionRow()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            //SpreadMatrixData.Dispose();
        }

        private void btnMatrixShow_Click(object sender, RoutedEventArgs e)
        {
            if (SpreadMatrixShowPressed != null)
                SpreadMatrixShowPressed(this, e);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (RemovePressed != null)
                RemovePressed(this, e);
        }
    }
}
