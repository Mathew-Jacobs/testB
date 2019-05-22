using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class StudentFinance
    {
        public List<AccountTerm> AccountTerms { get; set; }
        public object StartDate { get; set; }
        public object EndDate { get; set; }
        public string PersonName { get; set; }
        public double Balance { get; set; }
    }

    public class AccountTerm
    {
        public double Amount { get; set; }
        public string TermId { get; set; }
        public string Description { get; set; }
        public List<GeneralItem> GeneralItems { get; set; }
        public List<object> InvoiceItems { get; set; }
        public List<object> PaymentPlanItems { get; set; }
        public List<object> DepositDueItems { get; set; }
    }

    public class GeneralItem
    {
        public double AmountDue { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        public bool Overdue { get; set; }
        public string Term { get; set; }
        public string TermDescription { get; set; }
        public object Period { get; set; }
        public object PeriodDescription { get; set; }
        public string AccountType { get; set; }
        public string AccountDescription { get; set; }
        public string Distribution { get; set; }
    }

}