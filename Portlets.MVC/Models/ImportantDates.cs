using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class ImportantDates
    {
        public int results { get; set; }
        public bool success { get; set; }
        public List<ImportantDate> rows { get; set; }
    }

    public class ImportantDate
    {
        public string description { get; set; }
        public string keyDate { get; set; }
        public string id { get; set; }
    }
}