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
    internal partial class SecurityDefinitionGrid : UserControl
    {
        #region Methods
        public SecurityDefinitionGrid()
        {
            InitializeComponent();
        }

        public SecurityDefinitionRow NewRow(SpreadMatrixData spreadMatrixData)
        {
            SecurityDefinitionRow row = new SecurityDefinitionRow();

            row.label.Text = spreadMatrixData.Exchange + "/" + spreadMatrixData.Symbol;
            row.Exchange = spreadMatrixData.Exchange;
            row.Symbol = spreadMatrixData.Symbol;

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

            FixApplication.Current.SpreadMatrixCollection.Remove(row.Exchange, row.Symbol);

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
            
            SpreadMatrixData sm = FixApplication.Current.SpreadMatrixCollection.Get(row.Exchange, row.Symbol);

            if (sm != null)
            {
                SMatrixForm spreadMatrixForm =
                    (SMatrixForm)sm.SearchForUpdatableObject(wnd => wnd is SMatrixForm);

                if (spreadMatrixForm != null)
                {
                    spreadMatrixForm.Activate();
                    return;
                }
            }

            SMatrixForm spreadMatrixForm2 = new SMatrixForm { MessageProvider = sm };

            spreadMatrixForm2.Show();
        }

        private void onRemovePressed(object sender, RoutedEventArgs e)
        {
            SecurityDefinitionRow row = ((SecurityDefinitionRow)sender);
            RemoveRow(row);
        }
        #endregion
    }
}
