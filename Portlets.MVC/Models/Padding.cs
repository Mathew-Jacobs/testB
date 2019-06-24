using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Padding
    {
        public Padding(double padding)
        {
            PaddingTop = padding;
            PaddingRight = padding;
            PaddingBottom = padding;
            PaddingLeft = padding;
        }
        public Padding(double paddingY, double paddingX)
        {
            PaddingTop = paddingY;
            PaddingRight = paddingX;
            PaddingBottom = paddingY;
            PaddingLeft = paddingX;
        }
        public Padding(double paddingTop, double paddingX, double paddingBottom)
        {
            PaddingTop = paddingTop;
            PaddingRight = paddingX;
            PaddingBottom = paddingBottom;
            PaddingLeft = paddingX;
        }
        public Padding(double paddingTop, double paddingRight, double paddingBottom, double paddingLeft)
        {
            PaddingTop = paddingTop;
            PaddingRight = paddingRight;
            PaddingBottom = paddingBottom;
            PaddingLeft = paddingLeft;
        }
        public double PaddingTop { get; set; }
        public double PaddingRight { get; set; }
        public double PaddingLeft { get; set; }
        public double PaddingBottom { get; set; }

    }
}