using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Book
    {
        public string SectionNo { get; set; }
        public string priceNew { get; set; }
        public string author { get; set; }
        public string Term { get; set; }
        public string reqRecOpt { get; set; }
        public string CourseCode { get; set; }
        public string isbn { get; set; }
        public string priceUsed { get; set; }
        public string title { get; set; }
        public int? copyrightYear { get; set; }
        public int? edition { get; set; }
        public int CourseNumber { get; set; }
        public string SubjectCode { get; set; }
    }
}