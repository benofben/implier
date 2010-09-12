using System;
using System.Collections.Generic;
using System.Globalization;
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
    internal partial class SMatrixCell : UserControl
    {
        private Color foreAskColor = Colors.Red;
        private Color foreBidColor = Colors.Blue;
        private Color foreDefColor = Colors.Black;

        private Brush askBrush = new SolidColorBrush(Colors.Red);
        private Brush bidBrush = new SolidColorBrush(Colors.Blue);
        private Brush defaultBrush = new SolidColorBrush(Colors.GhostWhite);

        public enum SpreadMatrixCellType
        {
            Spread,
            Future
        }

        public string AskPx { get; private set; }
        public string AskCount { get; private set; }
        public string BidPx { get; private set; }
        public string BidCount { get; private set; }
        public SpreadMatrixCellType CellType { get; private set; }

        public SMatrixCell()
        {
            InitializeComponent();

            AskPx = "";
            AskCount = "";
            BidPx = "";
            BidCount = "";
            CellType = SpreadMatrixCellType.Future;

            //SetStyle(SpreadMatrixCellType.Future, null);          
        }

        /*private void SetStyle(SpreadMatrixCellType type, Dictionary<string, string> context)
        {
            string styleAskRect;
            string styleBidRect;
            string styleAskLbl;
            string styleBidLbl;

            switch (type)
            {
                case SpreadMatrixCellType.Spread:
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
                //rectAskPx.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["aPx"]) ? "StyleEmpty" : styleAskRect);
                //rectAskCount.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["aCount"]) ? "StyleEmpty" : styleAskRect);
                //rectBidPx.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["bPx"]) ? "StyleEmpty" : styleBidRect);
                //rectBidCount.Style = (Style)this.FindResource(String.IsNullOrEmpty(context["bCount"]) ? "StyleEmpty" : styleBidRect);
            }
            else
            {
                //rectAskPx.Style = (Style)this.FindResource("StyleEmpty");
                //rectAskCount.Style = (Style)this.FindResource("StyleEmpty");
                //rectBidPx.Style = (Style)this.FindResource("StyleEmpty");
                //rectBidCount.Style = (Style)this.FindResource("StyleEmpty");
            }

            //lblAskPx.Style = (Style)this.FindResource(styleAskLbl);
            //lblAskCount.Style = (Style)this.FindResource(styleAskLbl);
            //lblBidPx.Style = (Style)this.FindResource(styleBidLbl);
            //lblBidCount.Style = (Style)this.FindResource(styleBidLbl);
        }
        */

        delegate void FillDelegate(MDEntryGroup entryGroup, ref Dictionary<string, string> contxt);
        private FillDelegate fillContext = delegate(MDEntryGroup entryGroup, ref Dictionary<string, string> contxt)
                                               {
                                                   if (entryGroup == null)
                                                       return;

                                                   string px = entryGroup.MDEntryPx.ToString(Constants.FloatingPointFormat);
                                                   string size = entryGroup.MDEntrySize.ToString("F0");

                                                   switch (entryGroup.MDEntryType)
                                                   {
                                                       case QuickFix.MDEntryType.BID:
                                                           contxt["bPx"] = px;
                                                           contxt["bCount"] = size;
                                                           break;

                                                       case QuickFix.MDEntryType.OFFER:
                                                           contxt["aPx"] = px;
                                                           contxt["aCount"] = size;
                                                           break;

                                                       default:
                                                           throw new Exception("Undefined entry type.");
                                                   }
                                               };

        public void FillData(SecurityEntry entry)
        {
            Dictionary<string, string> context = new Dictionary<string, string>();
            context.Add("bPx", "");
            context.Add("bCount", "");
            context.Add("aPx", "");
            context.Add("aCount", "");

            for (uint i = 0; i < entry.MDGroupCount; i++)
                fillContext(entry.GetGroup(i), ref context);

            MDDatePair datePair = entry.GetDatePair();
            CellType = SpreadMatrixCellType.Future;

            if (datePair.Date1 != datePair.Date2)
                CellType = SpreadMatrixCellType.Spread;

            //SetStyle(CellType, context);

            AskPx = context["aPx"];
            AskCount = context["aCount"];
            BidPx = context["bPx"];
            BidCount = context["bCount"];

            InvalidateVisual();
        }

        private FormattedText GetFormattedText(string text)
        {
            // Create the initial formatted text string.
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.CurrentUICulture,//GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                11,
                Brushes.Black);

            return formattedText;

            // Set a maximum width and height. If the text overflows these values, an ellipsis "..." appears.
            formattedText.MaxTextWidth = 300;
            formattedText.MaxTextHeight = 240;

            // Use a larger font size beginning at the first (zero-based) character and continuing for 5 characters.
            // The font size is calculated in terms of points -- not as device-independent pixels.
            formattedText.SetFontSize(36 * (96.0 / 72.0), 0, 5);

            // Use a Bold font weight beginning at the 6th character and continuing for 11 characters.
            formattedText.SetFontWeight(FontWeights.Bold, 6, 11);

            // Use a linear gradient brush beginning at the 6th character and continuing for 11 characters.
            formattedText.SetForegroundBrush(
                                    new LinearGradientBrush(
                                    Colors.Orange,
                                    Colors.Teal,
                                    90.0),
                                    6, 11);

            // Use an Italic font style beginning at the 28th character and continuing for 28 characters.
            formattedText.SetFontStyle(FontStyles.Italic, 28, 28);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            int offset = 3;
            Pen pen = new Pen();

            int width = (int) Width;
            int height = (int) Height;

            int halfW = (int) width/2;
            int halfH = (int) height/2;

            // draw background
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.LightGray),
                                         new Pen(new SolidColorBrush(Colors.Black), 0),
                                         new Rect(0, 0, width, height));

            if (!String.IsNullOrEmpty(AskPx))
            {
                Brush brush = CellType == SpreadMatrixCellType.Future ? defaultBrush : askBrush;
                Color foreColor = CellType == SpreadMatrixCellType.Future ? foreAskColor : foreDefColor;

                Rect rect = new Rect(new Point(0, 0), new Size(width, halfH));
                drawingContext.DrawRectangle(brush, pen, rect);

                FormattedText formattedText = GetFormattedText(AskPx);
                formattedText.SetForegroundBrush(new SolidColorBrush(foreColor));

                drawingContext.DrawText(formattedText,
                                        new Point(halfW - formattedText.Width - offset, (halfH - formattedText.Height)/2));

                formattedText = GetFormattedText(AskCount);
                formattedText.SetForegroundBrush(new SolidColorBrush(foreColor));

                drawingContext.DrawText(formattedText,
                                        new Point(width - formattedText.Width - offset, (halfH - formattedText.Height)/2));
            }

            if (!String.IsNullOrEmpty(BidPx))
            {
                Brush brush = CellType == SpreadMatrixCellType.Future ? defaultBrush : bidBrush;
                Color foreColor = CellType == SpreadMatrixCellType.Future ? foreBidColor : Colors.White;

                Rect rect = new Rect(new Point(0, halfH), new Size(width, halfH));
                drawingContext.DrawRectangle(brush, pen, rect);

                FormattedText formattedText = GetFormattedText(BidPx);
                formattedText.SetForegroundBrush(new SolidColorBrush(foreColor));

                drawingContext.DrawText(formattedText,
                                        new Point(halfW - formattedText.Width - offset,
                                                  (halfH - formattedText.Height)/2 + halfH));

                formattedText = GetFormattedText(BidCount);
                formattedText.SetForegroundBrush(new SolidColorBrush(foreColor));

                drawingContext.DrawText(formattedText,
                                        new Point(width - formattedText.Width - offset,
                                                  (halfH - formattedText.Height)/2 + halfH));
            }

            // draw grid
            drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.DarkGray), 1), new Point(halfW - 0.5, 0.5),
                                    new Point(halfW - 0.5, height - 0.5));
            drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.DarkGray), 1), new Point(0.5, halfH - 0.5),
                                    new Point(width - 0.5, halfH - 0.5));

            // draw border
            drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point(width - 0.5, 0.5),
                                    new Point(width - 0.5, height - 0.5));
            drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point(0.5, height - 0.5),
                                    new Point(width - 0.5, height - 0.5));
        }
    }
}
