using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Grade
    {
        public string Id { get; set; }
        public string LetterGrade { get; set; }
        public int? GradeValue { get; set; }
        public string GradeSchemeCode { get; set; }
        public string Description { get; set; }
        public bool IsWithdraw { get; set; }
        public double GradePriority { get; set; }
        public bool ExcludeFromFacultyGrading { get; set; }
        public string IncompleteGrade { get; set; }
        public bool RequireLastAttendanceDate { get; set; }
    }
}