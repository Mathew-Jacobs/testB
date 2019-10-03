using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Student
    {
        public string name { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string colleagueID { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string state { get; set; }
        public string zipCode { get; set; }
        public string phoneNumber { get; set; }
        public string workNumber { get; set; }
        public List<string> race { get; set; }
        public List<string> ethnicity { get; set; }
        public List<string> pronoun { get; set; }
        public string chosenFirstName { get; set; }
        public string applicationDate { get; set; }
    }
}