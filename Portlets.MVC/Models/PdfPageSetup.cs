using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class PdfPageSetup
    {
        public double TotalHeight
        {
            get { return _totalHeight; }
            set
            {
                _totalHeight = value;
                if (_totalHeight > AvailableHeight)
                {
                    PDFPage = Pdf.AddPage();
                    PDFPage.TrimMargins.All = 25;
                    Graph = XGraphics.FromPdfPage(PDFPage);
                    TF = new XTextFormatterEx2(Graph);
                    AvailableHeight = PDFPage.Height.Point;
                    TotalHeight = 0;
                }
            }
        }
        public double AvailableHeight { get; set; }
        public int MyProperty { get; set; }
        public PdfDocument Pdf { get; set; }
        public PdfPage PDFPage { get; set; }
        public XGraphics Graph { get; set; }
        public XTextFormatterEx2 TF { get; set; }

        private double _totalHeight;
    }
}