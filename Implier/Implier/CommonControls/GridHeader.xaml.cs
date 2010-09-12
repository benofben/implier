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

namespace Implier.CommonControls
{
    /// <summary>
    /// Interaction logic for GridHeader.xaml
    /// </summary>
    internal partial class GridHeader : UserControl
    {
        public event RoutedEventHandler Click;
        
        public GridHeader()
        {
            InitializeComponent();
        }

        private void btnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }
    }
}
