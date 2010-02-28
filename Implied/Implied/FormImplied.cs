using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QuickFix;

namespace Implied
{
    public partial class FormImplied : Form
    {
        public delegate void UpdateConsole();
        public UpdateConsole consoleDelegate;

        FIXApplication fixApplication;

        public FormImplied()
        {
            InitializeComponent();

            consoleDelegate = new UpdateConsole(UpdateConsoleMethod);
            
            fixApplication = new FIXApplication(this);
            UpdateConsoleMethod();
        }

        public void UpdateConsoleMethod()
        {
            textBoxConsole.Text = fixApplication.consoleText;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            try
            {
                // figure out which symbol was requested
                String[] a = comboBoxSecurity.Text.Split('-');
                String exchange = a[0];
                String security = a[1];
                
                // request all maturities for the symbol
                fixApplication.requestSymbols(exchange, security);

                // as the symbols come down, the fix app will create subscribers for each one.
            }
            catch(IndexOutOfRangeException)
            {
            }

        }

        private void FormImplied_FormClosing(Object sender, FormClosingEventArgs e)
        {
            fixApplication.StopFIX();
        }
    }
}
