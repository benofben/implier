using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Collections.Generic;
using Implier.CommonControls.Windows;
using Implier.FIXApplication;
using Implier.SecurityDefinitionList;
using Implier.SpreadMatrix;
using MDEntry = QuickFix42.MarketDataSnapshotFullRefresh;
using MDDictionary = System.Collections.Generic.Dictionary<System.DateTime, System.Collections.Generic.Dictionary<System.DateTime, QuickFix42.MarketDataSnapshotFullRefresh>>;
using System;

namespace Implier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constants
        private const double DefaultExpandedHeight = 500;
        #endregion

        #region Fields
        FixApplication fixApplication;

        public FixApplication FixApplication
        {
            get { return fixApplication; }
        }

        private double expandedHeight = DefaultExpandedHeight;
        private double collapsedHeight;
        #endregion

        #region Delegate for thread-safe access to UI elements

        private delegate void UpdateText(MainWindow wnd, string text);
        private readonly UpdateText utDelegate = delegate(MainWindow wnd, string text)
        {
            bool needToScroll = wnd.textBoxFIXConsole.LineCount - 1 == wnd.textBoxFIXConsole.GetLastVisibleLineIndex();
            wnd.textBoxFIXConsole.AppendText(text);
            if (needToScroll)
            {
                wnd.textBoxFIXConsole.ScrollToLine(wnd.textBoxFIXConsole.LineCount - 1);
            }
        };

        private delegate void UpdateControls(MainWindow wnd, bool enable);
        private readonly UpdateControls ucDelegate = delegate(MainWindow wnd, bool enable)
        {
            wnd.UpdateButtons(enable);
        };

        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            MinHeight = Height;
            MinWidth = Width;
            collapsedHeight = Height; // store initail height
            //ResizeMode = ResizeMode.CanMinimize;

            UpdateButtons(false);
        }
        #endregion

        #region Methods

        private void UpdateButtons(bool logOn)
        {
            buttonLogOn.IsEnabled = !logOn;
            buttonLogOff.IsEnabled = logOn;
            comboBoxExchangeSymbol.IsEnabled = logOn;
            //buttonSubscribe.IsEnabled = logOn;
            buttonPureArbitrage.IsEnabled = logOn;
            buttonAdd.IsEnabled = logOn;
        }
        #endregion

        #region Event handlers
        void FixApplicationOnTextAdded(object sender, string text)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, utDelegate, this, text);
        }

        private void buttonLogOn_Click(object sender, RoutedEventArgs e)
        {
            buttonLogOn.IsEnabled = false;
            fixApplication = new FixApplication(comboBoxConfigFile.Text);
            securityDefinitionGrid.FixApplication = fixApplication;
            textBoxFIXConsole.Text = fixApplication.ConsoleText;
            fixApplication.OnTextAdded += FixApplicationOnTextAdded;
            fixApplication.Logon += fixApplication_Logon;
            fixApplication.Logout += fixApplication_Logout;
        }

        private void buttonLogOff_Click(object sender, RoutedEventArgs e)
        {
            securityDefinitionGrid.Clear();
            securityDefinitionGrid.FixApplication = null;
            buttonLogOff.IsEnabled = false;
            fixApplication.StopFix();
            fixApplication.Logon -= fixApplication_Logon;
            fixApplication.Logout -= fixApplication_Logout;
            fixApplication.OnTextAdded -= FixApplicationOnTextAdded;
            fixApplication = null;
        }

        private void fixApplication_Logon(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, ucDelegate, this, true);
        }

        private void fixApplication_Logout(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, ucDelegate, this, false);
        }

        /*private void buttonSubscribe_Click(object sender, RoutedEventArgs e)
        {
            fixApplication.RequestSymbols(comboBoxExchange.Text, comboBoxSymbol.Text);
        }*/

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            //buttonSpreadMatrix_Click(sender, e);
            //return;

            String[] parameters = comboBoxExchangeSymbol.Text.Split('/');
            if (parameters.Length == 2)
            {
                String exchange = parameters[0];
                String symbol = parameters[1];

                SpreadMatrixData smd = fixApplication.SpreadMatrixCollection.Get(exchange, symbol);
                if (smd == null)
                {
                    smd = fixApplication.RequestSymbols(exchange, symbol);
                    SecurityDefinitionRow row = securityDefinitionGrid.NewRow(smd);
                    securityDefinitionGrid.AddRow(row);
                }
            }
        }

        //private void buttonSpreadMatrix_Click(object sender, RoutedEventArgs e)
        //{
        //    if (fixApplication.SpreadMatrixData != null)
        //    {
        //        SMatrixForm spreadMatrixForm =
        //            (SMatrixForm) fixApplication.SpreadMatrixData.SearchForWindow(wnd => wnd is SMatrixForm);

        //        if (spreadMatrixForm != null)
        //        {
        //            spreadMatrixForm.Activate();
        //            return;
        //        }
        //    }

        //    SMatrixForm spreadMatrixForm2 = new SMatrixForm();

        //    fixApplication.RequestSymbols(comboBoxExchange.Text, comboBoxSymbol.Text);
        //    spreadMatrixForm2.MessageProvider = fixApplication.SpreadMatrixData;
        //    spreadMatrixForm2.Show();            
        //}

        private void expander1_Expanded(object sender, RoutedEventArgs e)
        {
            collapsedHeight = Height;
            Height = expandedHeight;
            //ResizeMode = ResizeMode.CanResize;
        }

        private void expander1_Collapsed(object sender, RoutedEventArgs e)
        {
            expandedHeight = Height;
            Height = collapsedHeight;
            //ResizeMode = ResizeMode.CanMinimize;
        }

        private void buttonPureArbitrage_Click(object sender, RoutedEventArgs e)
        {
            PureArbitrageWindow pureArbitrageWindow =
                (PureArbitrageWindow)fixApplication.SpreadMatrixCollection.SearchForWindow(wnd => wnd is PureArbitrageWindow);

            if (pureArbitrageWindow != null)
            {
                pureArbitrageWindow.Activate();
                return;
            }

            pureArbitrageWindow = new PureArbitrageWindow
                                      {
                                          MessageProvider = fixApplication.SpreadMatrixCollection
                                      };
            pureArbitrageWindow.Show();            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        #endregion
    }
}
