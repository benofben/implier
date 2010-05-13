using System.Windows;
using System.Windows.Threading;

namespace Implier
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constants
        private const double DefaultExpandedHeight = 530;
        #endregion

        #region Fields
        FixApplication fixApplication;
        private double expandedHeight = DefaultExpandedHeight;
        private double collapsedHeight;
        #endregion

        #region Delegate for thread-safe access to UI elements
        private delegate void UpdateText(MainWindow wnd, string text);

        private readonly UpdateText utDelegate = delegate(MainWindow wnd, string text)
        {
            bool needToScroll = wnd.textBoxFIXConsole.LineCount - 1 == wnd.textBoxFIXConsole.GetLastVisibleLineIndex();
            wnd.textBoxFIXConsole.AppendText(text);
            if( needToScroll)
            {
               wnd.textBoxFIXConsole.ScrollToLine(wnd.textBoxFIXConsole.LineCount-1);
            }
        };
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            collapsedHeight = Height; // store initail height
            this.MinHeight = Height;
            UpdateButtons(false);            
        }
        #endregion

        #region Methods
        private void UpdateButtons(bool logOn)
        {
            buttonLogOn.IsEnabled = !logOn;
            buttonLogOff.IsEnabled = logOn;
            comboBoxSymbol.IsEnabled = logOn;
            comboBoxExchange.IsEnabled = logOn;
            buttonSubscribe.IsEnabled = logOn;
        }
        #endregion

        #region Event handlers
        void FixApplicationOnTextAdded(object sender, string text)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, utDelegate, this, text);
        }

        private void buttonLogOn_Click(object sender, RoutedEventArgs e)
        {
            fixApplication = new FixApplication(comboBoxConfigFile.Text);
            textBoxFIXConsole.Text = fixApplication.ConsoleText;
            fixApplication.OnTextAdded += FixApplicationOnTextAdded;
            UpdateButtons(true);
        }

        private void buttonLogOff_Click(object sender, RoutedEventArgs e)
        {
            fixApplication.StopFix();
            fixApplication.OnTextAdded -= FixApplicationOnTextAdded;
            UpdateButtons(false);
        }

        private void buttonSubscribe_Click(object sender, RoutedEventArgs e)
        {
            fixApplication.RequestSymbols(comboBoxExchange.Text, comboBoxSymbol.Text);
        }

        private void expander1_Expanded(object sender, RoutedEventArgs e)
        {
            collapsedHeight = Height;
            Height = expandedHeight;
        }

        private void expander1_Collapsed(object sender, RoutedEventArgs e)
        {
            expandedHeight = Height;
            Height = collapsedHeight;
        }
        #endregion
    }
}
