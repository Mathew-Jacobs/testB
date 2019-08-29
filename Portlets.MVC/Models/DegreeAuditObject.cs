using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class DegreeAuditObject
    {
        public string SASJSONExport { get; set; }
        public List<SASTableDataJSONTEST> SASTableData { get; set; }
    }

    public class SASTableDataJSONTEST
    {
        public string Not_used { get; set; }
        public string used { get; set; }
        public string Academic_Program_Reqmt_Id_Nb { get; set; }
        public int Minimum_Credit_Ct { get; set; }
        public int Institution_Min_Credit_Ct { get; set; }
        public int Program_Minimum_GPA_Ct { get; set; }
        public int Institution_Min_GPA_Ct { get; set; }
        public string Student_Program_Id_Nb { get; set; }
        public double running_total { get; set; }
        public double I_running_total { get; set; }
        public double CUMGPA { get; set; }
        public int cred_for_credntl { get; set; }
        public int crse_for_credntl { get; set; }
        public double PROGPA { get; set; }
        public int total_blocks { get; set; }
        public int blocks_met { get; set; }
    }
}