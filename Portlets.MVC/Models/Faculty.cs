using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Faculty
    {
        public string Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string ProfessionalName { get; set; }
        public List<string> Phones { get; set; }
        public List<string> EmailAddresses { get; set; }
        public List<string> Addresses { get; set; }
    }
}