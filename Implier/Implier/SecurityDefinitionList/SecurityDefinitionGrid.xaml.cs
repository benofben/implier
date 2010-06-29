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
using Implier.FIXApplication;
using Implier.SpreadMatrix;

namespace Implier.SecurityDefinitionList
{
    /// <summary>
    /// Interaction logic for SecurityDefinitionGrid.xaml
    /// </summary>
    public partial class SecurityDefinitionGrid : UserControl
    {
        #region Methods
        public SecurityDefinitionGrid()
        {
            InitializeComponent();
        }

        public SecurityDefinitionRow NewRow(SpreadMatrixData spreadMatrixData)
        {
            SecurityDefinitionRow row = new SecurityDefinitionRow();

            row.label1.Content = spreadMatrixData.Exchange + "/" + spreadMatrixData.Symbol;
            row.SpreadMatrixData = spreadMatrixData;

            return row;
        }

        public void AddRow(SecurityDefinitionRow row)
        {
            stackPanel.Children.Add(row);
            row.SpreadMatrixShowPressed += onSpreadMatrixShowPressed;
            row.RemovePressed += onRemovePressed;
        }

        public void RemoveRow(SecurityDefinitionRow row)
        {
            row.SpreadMatrixShowPressed -= onSpreadMatrixShowPressed;
            row.RemovePressed -= onRemovePressed;

            FixApplication.SpreadMatrixCollection.Remove(row.SpreadMatrixData.Exchange, row.SpreadMatrixData.Symbol);
            row.Dispose();

            stackPanel.Children.Remove(row);
        }

        public void RemoveRow(int index)
        {
            RemoveRow((SecurityDefinitionRow) stackPanel.Children[index]);
        }

        public void Clear()
        {
            while (stackPanel.Children.Count > 0)
                RemoveRow(0);
        }
        #endregion

        #region Events

        private void onSpreadMatrixShowPressed(object sender, RoutedEventArgs e)
        {
            SecurityDefinitionRow row = ((SecurityDefinitionRow)sender);

            if (row.SpreadMatrixData != null)
            {
                SMatrixForm spreadMatrixForm =
                    (SMatrixForm)row.SpreadMatrixData.SearchForWindow(wnd => wnd is SMatrixForm);

                if (spreadMatrixForm != null)
                {
                    spreadMatrixForm.Activate();
                    return;
                }
            }

            SMatrixForm spreadMatrixForm2 = new SMatrixForm { MessageProvider = row.SpreadMatrixData };

            spreadMatrixForm2.Show();            

        }

        private void onRemovePressed(object sender, RoutedEventArgs e)
        {
            SecurityDefinitionRow row = ((SecurityDefinitionRow)sender);
            RemoveRow(row);
        }
        #endregion

        #region Properties
        internal FixApplication FixApplication { get; set; }
        #endregion
    }
}
