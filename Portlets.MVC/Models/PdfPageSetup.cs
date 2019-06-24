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
                    if (Column == 1)
                    {
                        // Make New Page
                    }
                    else
                    {
                        Column = 1;
                    }
                }
            }
        }
        public double Column { get; set; }
        public double AvailableHeight { get; set; }

        private double _totalHeight;
    }
}