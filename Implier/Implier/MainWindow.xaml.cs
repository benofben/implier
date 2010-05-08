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

namespace Implier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FIXApplication fixApplication;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonLogOn_Click(object sender, RoutedEventArgs e)
        {
            fixApplication = new FIXApplication(comboBoxConfigFile.Text);
            textBoxFIXConsole.Text = fixApplication.consoleText;
        }

        private void buttonLogOff_Click(object sender, RoutedEventArgs e)
        {
            fixApplication.StopFIX();
            textBoxFIXConsole.Text = fixApplication.consoleText;
        }

        private void buttonSubscribe_Click(object sender, RoutedEventArgs e)
        {
            fixApplication.requestSymbols(comboBoxExchange.Text, comboBoxSymbol.Text);
            textBoxFIXConsole.Text = fixApplication.consoleText;
        }

        // this needs to be replaced with WPF events!!!!!!
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            textBoxFIXConsole.Text = fixApplication.consoleText;
        }
    }
}
