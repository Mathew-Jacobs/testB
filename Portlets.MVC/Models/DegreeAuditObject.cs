using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class DegreeAuditObject
    {
        public string Student_Id_Nb { get; set; }
        public int? Parent_Block_Counter_Id { get; set; }
        public int? ARB_Counter_Id { get; set; }
        public string Block_Label { get; set; }
        public string COL1 { get; set; }
        public string Academic_Program_Reqmt_Id_Nb { get; set; }
        public string Printed_Specification_Ds { get; set; }
        public int? Minimum_Credit_Ct { get; set; }
        public int? Institution_Min_Credit_Ct { get; set; }
        public int? Program_Minimum_GPA_Ct { get; set; }
        public int? Institution_Min_GPA_Ct { get; set; }
        public string Student_Program_Id_Nb { get; set; }
        public int? running_total { get; set; }
        public int? I_running_total { get; set; }
        public int? earn_total { get; set; }
        public int? pts_total { get; set; }
        public int? met { get; set; }
        public int? credits_left { get; set; }
        public int? subjects_left { get; set; }
        public int? courses_left { get; set; }
        public int? CUMGPA { get; set; }
        public int? cred_for_credntl { get; set; }
        public int? crse_for_credntl { get; set; }
        public int? PROGPA { get; set; }
        public int? met_prog_gpa { get; set; }
        public int? met_cum_gpa { get; set; }
        public int? met_inst_cred { get; set; }
        public int? met_prog_cred { get; set; }
        public string Catalog_Cd { get; set; }
        public string Program_Nm { get; set; }
        public string Credit_Type_Cd { get; set; }
        public string Term_Id_Cd { get; set; }
        public int? Sem_GPA_Credit_Ct { get; set; }
        public int? Sem_Credit_Hours_Earned_Ct { get; set; }
        public int? Sem_Credit_Hours_Completed_Ct { get; set; }
        public string Verified_Grade_Cd { get; set; }
        public string Course_Title_Nm { get; set; }
        public string course_nm_s { get; set; }
        public string Academic_Standing_Cd { get; set; }
    }

    public class DegreeAuditCourse
    {
        public string Course_Title { get; set; }
        public string Course_Name { get; set; }
        public string Specification { get; set; }
        public string Verified_Grade { get; set; }
    }

    public class DegreeAuditBlock
    {
        public int? Block_ID { get; set; }
        public string Block_Label { get; set; }
        public bool Block_Met { get; set; }
        public List<DegreeAuditCourse> Courses { get; set; }
    }
}