using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class RegistrationDate
    {
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public DateTime PreRegistrationStartDate { get; set; }
        public DateTime PreRegistrationEndDate { get; set; }
        public DateTime AddStartDate { get; set; }
        public DateTime AddEndDate { get; set; }
        public DateTime DropStartDate { get; set; }
        public DateTime DropEndDate { get; set; }
        public DateTime DropGradeRequiredDate { get; set; }
        public string Location { get; set; }
    }

    public class Term
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ReportingYear { get; set; }
        public int Sequence { get; set; }
        public string ReportingTerm { get; set; }
        public string FinancialPeriod { get; set; }
        public bool DefaultOnPlan { get; set; }
        public bool IsActive { get; set; }
        public bool ForPlanning { get; set; }
        public List<RegistrationDate> RegistrationDates { get; set; }
        public List<object> FinancialAidYears { get; set; }
        public List<object> SessionCycles { get; set; }
        public List<object> YearlyCycles { get; set; }
    }
}