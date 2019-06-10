using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class ImportantDate
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Display { get; set; }
    }
}