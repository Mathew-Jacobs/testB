using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class FinancialAidChecklist
    {
        public string ChecklistItemCode { get; set; }
        public string ChecklistItemType { get; set; }
        public int ChecklistSortNumber { get; set; }
        public string Description { get; set; }
    }
}