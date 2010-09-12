using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Collections.Generic;
using Implier.CommonControls.Windows;
using Implier.FIXApplication;
using Implier.SecurityDefinitionList;
using Implier.SpreadMatrix;
using QuickFix;
using MDEntry = QuickFix42.MarketDataSnapshotFullRefresh;
using MDDictionary = System.Collections.Generic.Dictionary<System.DateTime, System.Collections.Generic.Dictionary<System.DateTime, QuickFix42.MarketDataSnapshotFullRefresh>>;
using System;

namespace Implier
{
    internal enum AppMode
    {
        Regular,
        Sleep
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow
    {
        #region Constants
        private const double DefaultExpandedHeight = 500;
        #endregion

        #region Fields
        FixApplication fixApplication;
        AppMode mode = AppMode.Regular;

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

        private delegate void LogonDelegate(MainWindow wnd);
        private readonly LogonDelegate logonDelegate = delegate(MainWindow wnd)
        {
            wnd.UpdateButtons(true);

            SpreadMatrixCollection smc = FixApplication.Current.SpreadMatrixCollection;

            foreach (SecurityDefinitionRow row in
                wnd.securityDefinitionGrid.stackPanel.Children.OfType<SecurityDefinitionRow>())
            {
                smc.Remove(row.Exchange, row.Symbol);
                FixApplication.Current.RequestSymbols(row.Exchange, row.Symbol);
            }
        };

        private delegate void LogoutDelegate(MainWindow wnd);
        private readonly LogoutDelegate logoutDelegate = delegate(MainWindow wnd)
        {
            wnd.UpdateButtons(false);
            wnd.radRegular.IsChecked = true;
            wnd.SwitchMode(AppMode.Regular);
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

            //cmbMode.ItemsSource = Enum.GetValues(typeof (AppMode));
            //cmbMode.SelectedItem = AppMode.Regular;
        }
        #endregion

        #region Methods
        private void WriteStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void UpdateButtons(bool logOn)
        {
            buttonLogOn.IsEnabled = !logOn;
            buttonLogOff.IsEnabled = logOn;

            buttonPureArbitrage.IsEnabled = logOn;
            
            comboBoxExchangeSymbol.IsEnabled = logOn;
            buttonAdd.IsEnabled = logOn;

            comboBoxExchangeSymbolFromFile.IsEnabled = logOn;
            buttonAddFromFile.IsEnabled = logOn;

            radRegular.IsEnabled = logOn;
            radSleep.IsEnabled = logOn;
            //cmbMode.IsEnabled = logOn;
        }

        private void CreateExchangeSymbolRow(string exchange, string symbol)
        {
            SpreadMatrixData smd = fixApplication.SpreadMatrixCollection.Get(exchange, symbol);
            if (smd == null)
            {
                smd = fixApplication.RequestSymbols(exchange, symbol);
                SecurityDefinitionRow row = securityDefinitionGrid.NewRow(smd);
                securityDefinitionGrid.AddRow(row);
            }
        }

        void SwitchMode(AppMode newMode)
        {
            if (mode == newMode)
                return;

            mode = newMode;

            switch (mode)
            {
                case AppMode.Regular:

                    buttonPureArbitrage.IsEnabled = true;
                    LogModeController.Stop();
                    break;

                case AppMode.Sleep:

                    buttonPureArbitrage.IsEnabled = false;
                    PureArbitrageWindow pureArbitrageWindow =
                        (PureArbitrageWindow)
                        fixApplication.SpreadMatrixCollection.SearchForUpdatableObject(wnd => wnd is PureArbitrageWindow);
                    if (pureArbitrageWindow != null)
                        pureArbitrageWindow.MessageProvider = null;

                    foreach (SpreadMatrixData spreadMatrixData in fixApplication.SpreadMatrixCollection.Values)
                    {
                        SMatrixForm spreadMatrixForm =
                            (SMatrixForm) spreadMatrixData.SearchForUpdatableObject(wnd => wnd is SMatrixForm);

                        if (spreadMatrixForm != null)
                            spreadMatrixForm.MessageProvider = null;
                    }

                    LogModeController.Start();
                    break;
            }
        }

        #endregion

        #region Event handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        void FixApplicationOnTextAdded(object sender, string text)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, utDelegate, this, text);
        }

        private void buttonLogOn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (fixApplication != null)
                    buttonLogOff_Click(sender, e);

                WriteStatus("Connecting...");

                buttonLogOn.IsEnabled = false;
                fixApplication = new FixApplication(comboBoxConfigFile.Text);
            }
            catch(ThreadAbortException)
            {
                buttonLogOn.IsEnabled = true;
                throw;
            }
            catch (Exception)
            {
                buttonLogOn.IsEnabled = true;
                return;
            }
            finally
            {
                WriteStatus(String.Empty);
            }
            
            textBoxFIXConsole.Text = fixApplication.ConsoleText;
            fixApplication.OnTextAdded += FixApplicationOnTextAdded;
            fixApplication.Logon += fixApplication_Logon;
            fixApplication.Logout += fixApplication_Logout;
        }

        private void buttonLogOff_Click(object sender, RoutedEventArgs e)
        {
            WriteStatus("Disconnecting...");

            UpdateButtons(false);
            buttonLogOn.IsEnabled = false;

            radRegular.IsChecked = true;
            SwitchMode(AppMode.Regular);
            securityDefinitionGrid.Clear();

            fixApplication.StopFix();

            WriteStatus(String.Empty);
            fixApplication.Logon -= fixApplication_Logon;
            fixApplication.Logout -= fixApplication_Logout;
            fixApplication.OnTextAdded -= FixApplicationOnTextAdded;
            fixApplication = null;
        }

        private void fixApplication_Logon(object sender, EventArgs e)
        {
            if (FixApplication.Current.LogonCount==2)
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, logonDelegate, this);
        }

        private void fixApplication_Logout(object sender, EventArgs e)
        {
            if (FixApplication.Current.LogonCount == 0)
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, logoutDelegate, this);
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            //buttonSpreadMatrix_Click(sender, e);
            //return;

            String[] parameters = comboBoxExchangeSymbol.Text.Split('/');
            if (parameters.Length == 2)
            {
                String exchange = parameters[0];
                String symbol = parameters[1];

                CreateExchangeSymbolRow(exchange, symbol);
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
                (PureArbitrageWindow)fixApplication.SpreadMatrixCollection.SearchForUpdatableObject(wnd => wnd is PureArbitrageWindow);

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

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text files (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = dlg.ShowDialog();

            if (result != true) return;

            // Open document
            string filename = dlg.FileName;
            List<string> list = new List<string>();

            using (TextReader tr = new StreamReader(filename))
            {
                while (tr.Peek() >= 0)
                {
                    string line = tr.ReadLine();

                    list.Add(line);
                }
            }

            foreach (Object item in comboBoxExchangeSymbol.Items)
            {
                if(item is ComboBoxItem)
                    list.Add(((ComboBoxItem) item).Content.ToString());
                else
                    list.Add(item.ToString());
            }

            list = list.Distinct().ToList();

            list.Sort();

            comboBoxExchangeSymbol.ItemsSource = null;
            comboBoxExchangeSymbol.Items.Clear();
            comboBoxExchangeSymbol.ItemsSource = list;
            if (list.Count>0)
                comboBoxExchangeSymbol.SelectedIndex = 0;
        }

        private void buttonAddFromFile_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = Utils.GetCurrentFolder();
            string fileName = comboBoxExchangeSymbolFromFile.Text;

            string filePath = Path.Combine(folderPath, fileName);

            try
            {
                if (!File.Exists(filePath))
                    return;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                return;
            }

            using (TextReader tr = new StreamReader(filePath))
            {
                while (tr.Peek() >= 0)
                {
                    string line = tr.ReadLine();

                    String[] parameters = line.Split('/');
                    if (parameters.Length == 2)
                    {
                        String exchange = parameters[0];
                        String symbol = parameters[1];

                        CreateExchangeSymbolRow(exchange, symbol);
                    }
                }
            }

        }

        #endregion

        private void cmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SwitchMode((AppMode)cmbMode.SelectedItem);
        }

        private void radRegular_Checked(object sender, RoutedEventArgs e)
        {
            SwitchMode(AppMode.Regular);
        }

        private void radSleep_Checked(object sender, RoutedEventArgs e)
        {
            SwitchMode(AppMode.Sleep);
        }
    }
}
