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
using QuickFix42;

namespace Implier
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SMatrixCell : UserControl
    {
        enum SpreadMatrixCellStyles
        {
            Spread,
            Default
        }

        public SMatrixCell()
        {
            InitializeComponent();

            lblAskPx.Content = "";
            lblAskCount.Content = "";
            lblBidPx.Content = "";
            lblBidCount.Content = "";

            SetStyle(SpreadMatrixCellStyles.Default, null);          
        }

        private void SetStyle(SpreadMatrixCellStyles style, Dictionary<string, string> context)
        {
            string styleAskRect;
            string styleBidRect;
            string styleAskLbl;
            string styleBidLbl;

            switch (style)
            {
                case SpreadMatrixCellStyles.Spread:
                    styleAskRect = "StyleAsk";
                    styleBidRect = "StyleBid";

                    styleAskLbl = "lblStyleAskSpread";
                    styleBidLbl = "lblStyleBidSpread";
                    break;
                default:
                    styleAskRect = "StyleEmpty";
                    styleBidRect = "StyleEmpty";

                    styleAskLbl = "lblStyleAsk";
                    styleBidLbl = "lblStyleBid";
                    break;
            }

            if (context != null)
            {
                rectAskPx.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["aPx"]) ? "StyleEmpty" : styleAskRect);
                rectAskCount.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["aCount"]) ? "StyleEmpty" : styleAskRect);
                rectBidPx.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["bPx"]) ? "StyleEmpty" : styleBidRect);
                rectBidCount.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["bCount"]) ? "StyleEmpty" : styleBidRect);
            }
            else
            {
                rectAskPx.Style = (Style)this.FindResource("StyleEmpty");
                rectAskCount.Style = (Style)this.FindResource("StyleEmpty");
                rectBidPx.Style = (Style)this.FindResource("StyleEmpty");
                rectBidCount.Style = (Style)this.FindResource("StyleEmpty");
            }

            lblAskPx.Style = (Style)this.FindResource(styleAskLbl);
            lblAskCount.Style = (Style)this.FindResource(styleAskLbl);
            lblBidPx.Style = (Style)this.FindResource(styleBidLbl);
            lblBidCount.Style = (Style)this.FindResource(styleBidLbl);
        }
        
        public void FillData(MDEntry entry)
        {
            Dictionary<string, string> context = new Dictionary<string, string>();
            context.Add("bPx", "");
            context.Add("bCount", "");
            context.Add("aPx", "");
            context.Add("aCount", "");

            for (uint i = 0; i < entry.GroupCount; i++)
            {
                string px = entry.GetGroup(i).EntryPx.ToString("F2");
                string size = entry.GetGroup(i).EntrySize.ToString("F0");

                switch (entry.GetGroup(i).EntryType)
                {
                    case QuickFix.MDEntryType.BID:
                        context["bPx"] = px;
                        context["bCount"] = size;
                        break;
                    
                    case QuickFix.MDEntryType.OFFER:
                        context["aPx"] = px;
                        context["aCount"] = size;
                        break;

                    default:
                        throw new Exception("Undefined entry type.");
                }
            }

            MDDatePair datePair = entry.GetDatePair();
            SpreadMatrixCellStyles style = SpreadMatrixCellStyles.Default;

            if (datePair.Date1 != datePair.Date2)
                style = SpreadMatrixCellStyles.Spread;

            SetStyle(style, context);

            lblAskPx.Content = context["aPx"];
            lblAskCount.Content = context["aCount"];
            lblBidPx.Content = context["bPx"];
            lblBidCount.Content = context["bCount"];
        }
    }
}
