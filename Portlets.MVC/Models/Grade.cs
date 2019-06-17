using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Grade
    {
        private string symbol = "";
        public string Id { get; set; }
        public string LetterGrade { get; set; }
        public string Symbol
        {
            get
            {
                if (symbol != "")
                {
                    return symbol;
                }
                else
                {
                    if (!string.IsNullOrEmpty(LetterGrade))
                    {
                        char[] array = LetterGrade.ToCharArray();
                        return new string(Array.FindAll(array, x => !char.IsLetterOrDigit(x)));
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            set
            {
                symbol = value;
            }
        }
        public string DisplayGrade
        {
            get
            {
                if (!string.IsNullOrEmpty(LetterGrade))
                {
                    char[] array = LetterGrade.ToCharArray();
                    array = Array.FindAll<char>(array, x => char.IsLetterOrDigit(x));
                    return new string(array);
                }
                else
                {
                    return "";
                }
            }
        }
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