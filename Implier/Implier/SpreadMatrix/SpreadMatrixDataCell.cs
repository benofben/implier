using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Implier.SpreadMatrix
{
    internal enum SpreadMatrixCellType : int
    {
        Future = 0,
        Spread,
        Empty
    }

    internal class SpreadMatrixBaseCell : Shape
    {
        #region Fields
        protected Pen borderPen = new Pen(new SolidColorBrush(Colors.Black), 1);
        protected Pen pen = new Pen();
        
        #endregion

        #region Properties
        protected override Geometry DefiningGeometry
        {
            get
            {
                // Create a StreamGeometry for describing the shape
                StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext context = geometry.Open())
                {
                    InternalDrawGeometry(context);
                }

                // Freeze the geometry for performance benefits
                geometry.Freeze();

                return geometry;
            }
        }
        #endregion

        #region Methods
        private void InternalDrawGeometry(StreamGeometryContext context)
        {
            double width = Width;
            double height = Height;

            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(width, 0);
            Point pt3 = new Point(width, height);
            Point pt4 = new Point(0, height);

            context.BeginFigure(pt1, true, true);
            context.LineTo(pt2, true, true);
            context.LineTo(pt3, true, true);
            context.LineTo(pt4, true, true);
        }

        protected void FillBackGround(DrawingContext drawingContext, Brush brush)
        {
            drawingContext.DrawRectangle(
                brush,
                pen,
                new Rect(0, 0, Width, Height));
        }

        protected void DrawBorder(DrawingContext drawingContext, Pen bdrPen)
        {
            int width = (int)Width;
            int height = (int)Height;

            // draw border
            drawingContext.DrawLine(bdrPen, new Point(width - 0.5, 0.5), new Point(width - 0.5, height - 0.5));
            drawingContext.DrawLine(bdrPen, new Point(0.5, height - 0.5), new Point(width - 0.5, height - 0.5));
        }

        protected FormattedText GetFormattedText(string text)
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
        }
        
        #endregion
    }

    internal class SpreadMatrixDataCell : SpreadMatrixBaseCell
    {
        #region Fields

        private Pen gridPen = new Pen(new SolidColorBrush(Colors.DarkGray), 1);

        private Color[] foreAskColors = new Color[] { Colors.Red, Colors.Black, Colors.Black };
        private Color[] foreBidColors = new Color[] { Colors.Blue, Colors.White, Colors.Black };


        private Brush[] askBrushes = new Brush[]
                                         {
                                             new SolidColorBrush(Colors.WhiteSmoke), 
                                             new SolidColorBrush(Colors.Red),
                                             new SolidColorBrush(Colors.Transparent)
                                         };

        private Brush[] bidBrushes = new Brush[]
                                         {
                                             new SolidColorBrush(Colors.WhiteSmoke), 
                                             new SolidColorBrush(Colors.Blue),
                                             new SolidColorBrush(Colors.Transparent)
                                         };

        private Brush[] bgBrushes = new Brush[]
                                         {
                                             new SolidColorBrush(Colors.Lavender), 
                                             new SolidColorBrush(Colors.Lavender),
                                             new SolidColorBrush(Colors.Gainsboro)
                                         };  

        private static int textOffset = 3;

        #endregion

        #region Properties
        public string AskPx { get; private set; }
        public string AskCount { get; private set; }
        public string BidPx { get; private set; }
        public string BidCount { get; private set; }
        public SpreadMatrixCellType CellType { get; private set; }

        #endregion

        #region Constructor
        public SpreadMatrixDataCell()
        {
            AskPx = String.Empty;
            AskCount = String.Empty;
            BidPx = String.Empty;
            BidCount = String.Empty;

            CellType = SpreadMatrixCellType.Empty;
        }
        #endregion

        #region Methods
        private static void FillData(MDEntryGroup entryGroup, ref Dictionary<string, string> dict)
        {
            if (entryGroup == null)
                return;

            string px = entryGroup.MDEntryPx.ToString(Constants.FloatingPointFormat);
            string size = entryGroup.MDEntrySize.ToString("F0");

            switch (entryGroup.MDEntryType)
            {
                case QuickFix.MDEntryType.BID:
                    dict["bPx"] = px;
                    dict["bCount"] = size;
                    break;

                case QuickFix.MDEntryType.OFFER:
                    dict["aPx"] = px;
                    dict["aCount"] = size;
                    break;

                default:
                    throw new Exception("Undefined entry type.");
            }
        }

        public void FillData(SecurityEntry entry)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
                                                  {
                                                      {"bPx", String.Empty},
                                                      {"bCount", String.Empty},
                                                      {"aPx", String.Empty},
                                                      {"aCount", String.Empty}
                                                  };

            for (uint i = 0; i < entry.MDGroupCount; i++)
                FillData(entry.GetGroup(i), ref dict);

            MDDatePair datePair = entry.GetDatePair();
            CellType = SpreadMatrixCellType.Future;

            if (datePair.Date1 != datePair.Date2)
                CellType = SpreadMatrixCellType.Spread;

            AskPx = dict["aPx"];
            AskCount = dict["aCount"];
            BidPx = dict["bPx"];
            BidCount = dict["bCount"];

            InvalidateVisual();
        }

        private void DrawDataCell(DrawingContext drawingContext)
        {
            int width = (int)Width;
            int height = (int)Height;

            int halfW = (int)width / 2;
            int halfH = (int)height / 2;

            FillBackGround(drawingContext, bgBrushes[(int)CellType]);

            if (!String.IsNullOrEmpty(AskPx))
            {
                Brush brush = askBrushes[(int)CellType];
                Color foreColor = foreAskColors[(int)CellType];
                Brush textBrush = new SolidColorBrush(foreColor);

                Rect rect = new Rect(new Point(0, 0), new Size(width, halfH));
                drawingContext.DrawRectangle(brush, pen, rect);

                FormattedText formattedText = GetFormattedText(AskPx);
                formattedText.SetForegroundBrush(textBrush);

                drawingContext.DrawText(formattedText,
                                        new Point(halfW - formattedText.Width - textOffset, (halfH - formattedText.Height) / 2));

                formattedText = GetFormattedText(AskCount);
                formattedText.SetForegroundBrush(textBrush);

                drawingContext.DrawText(formattedText,
                                        new Point(width - formattedText.Width - textOffset, (halfH - formattedText.Height) / 2));
            }

            if (!String.IsNullOrEmpty(BidPx))
            {
                Brush brush = bidBrushes[(int)CellType];
                Color foreColor = foreBidColors[(int)CellType];
                Brush textBrush = new SolidColorBrush(foreColor);

                Rect rect = new Rect(new Point(0, halfH), new Size(width, halfH));
                drawingContext.DrawRectangle(brush, pen, rect);

                FormattedText formattedText = GetFormattedText(BidPx);
                formattedText.SetForegroundBrush(textBrush);

                drawingContext.DrawText(formattedText,
                                        new Point(halfW - formattedText.Width - textOffset,
                                                  (halfH - formattedText.Height) / 2 + halfH));

                formattedText = GetFormattedText(BidCount);
                formattedText.SetForegroundBrush(textBrush);

                drawingContext.DrawText(formattedText,
                                        new Point(width - formattedText.Width - textOffset,
                                                  (halfH - formattedText.Height) / 2 + halfH));
            }

            // draw grid
            DrawGrid(drawingContext, gridPen);
            DrawBorder(drawingContext, borderPen);
        }

        private void DrawGrid(DrawingContext drawingContext, Pen pen)
        {
            int width = (int)Width;
            int height = (int)Height;
            int halfW = width/2;
            int halfH = height/2;

            // draw border
            drawingContext.DrawLine(pen, new Point(halfW - 0.5, 0.5), new Point(halfW - 0.5, height - 0.5));
            drawingContext.DrawLine(pen, new Point(0.5, halfH - 0.5), new Point(width - 0.5, halfH - 0.5));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);

            DrawDataCell(drawingContext);
        }
        #endregion
    }

    internal class SpreadMatrixCell : SpreadMatrixBaseCell
    {
        #region Fields
        
        private Color foreColor = Colors.Black;
        private Brush bgBrush = new SolidColorBrush(Colors.WhiteSmoke);
        
        #endregion

        #region Properties
        public DateTime Date { get; private set; }
        public string Caption { get; private set; }
        #endregion

        #region Constructor
        public SpreadMatrixCell()
        {
            Caption = String.Empty;
            Date = new DateTime();
        }
        #endregion

        #region Methods
        public void FillData(DateTime date)
        {
            Date = date;
            Caption = date.ToString("MMMyy");
            InvalidateVisual();
        }

        public void FillData(string caption)
        {
            Caption = caption;
            InvalidateVisual();
        }

        private void DrawDataCell(DrawingContext drawingContext)
        {
            int width = (int) Width;
            int height = (int) Height;

            FillBackGround(drawingContext, bgBrush);

            Brush textBrush = new SolidColorBrush(foreColor);

            FormattedText formattedText = GetFormattedText(Caption);
            formattedText.SetForegroundBrush(textBrush);

            drawingContext.DrawText(formattedText,
                                    new Point((width - formattedText.Width)/2,
                                              (height - formattedText.Height)/2));

            DrawBorder(drawingContext, borderPen);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            DrawDataCell(drawingContext);
        }
        #endregion
    }


}
