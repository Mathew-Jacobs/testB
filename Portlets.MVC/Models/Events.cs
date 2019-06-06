using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Events
    {
        public int results { get; set; }
        public bool success { get; set; }
        public List<Event> rows { get; set; }
    }

    public class Event
    {
        public string title { get; set; }
        public string pubDate { get; set; }
        public string body { get; set; }
        public int isFeature { get; set; }
    }
}