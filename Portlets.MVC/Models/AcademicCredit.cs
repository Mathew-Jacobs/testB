using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Portlets.Service.Admin;
using RestSharp;

namespace Portlets.MVC.Models
{
    public class AcademicCredit
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public object StudentId { get; set; }
        public object SectionId { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string VerifiedGradeId { get; set; }
        public Grade GradeInfo { get; set; }
        public DateTime? VerifiedGradeTimestamp { get; set; }
        public double Credit { get; set; }
        public double GpaCredit { get; set; }
        public double GradePoints { get; set; }
        public double CompletedCredit { get; set; }
        public int ContinuingEducationUnits { get; set; }
        public string Status { get; set; }
        public object StatusDate { get; set; }
        public string TermCode { get; set; }
        public List<object> MidTermGrades { get; set; }
        public string GradingType { get; set; }
        public string SectionNumber { get; set; }
        public bool HasVerifiedGrade { get; set; }
        public double AdjustedCredit { get; set; }
        public bool IsNonCourse { get; set; }
        public object StartDate { get; set; }
        public object EndDate { get; set; }
        public bool IsCompletedCredit { get; set; }
        public string ReplacedStatus { get; set; }
        public string ReplacementStatus { get; set; }
    }

    public class AcademicTerm
    {
        private bool incl = true;
        public string TermId { get; set; }
        public string TermSeason
        {
            get
            {
                var season = TermId.Substring(3);
                switch (season)
                {
                    case "WI":
                        return "Winter";
                    case "SP":
                        return "Spring";
                    case "SU":
                        return "Summer";
                    case "FA":
                        return "Fall";
                    case "SUA":
                        return "Summer";
                    case "SUB":
                        return "Summer";
                    case "SUC":
                        return "Summer";
                    case "SUD":
                        return "Summer";
                    default:
                        return null;
                }

            }
        }
        public string TermYear
        {
            get
            {
                var today = int.Parse(DateTime.Now.Year.ToString().Substring(2));
                var year = TermId.Substring(0, 2);
                if (int.Parse(year) > today)
                {
                    return $"19{year}";
                }
                else
                {
                    return $"20{year}";
                }
            }
        }
        public DateTime TermComparer
        {
            get
            {
                string month;
                switch (TermSeason)
                {
                    case "Winter":
                        month = "1";
                        break;
                    case "Spring":
                        month = "2";
                        break;
                    case "Summer":
                        month = "3";
                        break;
                    case "Fall":
                        month = "7";
                        break;
                    default:
                        month = "10";
                        break;
                };
                return DateTime.ParseExact($"{TermYear}/{month}", "yyyy/M", CultureInfo.CurrentCulture);
            }
        }
        public double GradePointAverage { get; set; }
        public double Credits { get; set; }
        public int ContinuingEducationUnits { get; set; }
        public bool FreshStart { get; set; }
        public bool Included { get { return incl; } set { incl = value; } }
        public List<AcademicCredit> AcademicCredits { get; set; }
    }

    public class NonTermAcademicCredit
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public object StudentId { get; set; }
        public object SectionId { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string VerifiedGradeId { get; set; }
        public object VerifiedGradeTimestamp { get; set; }
        public double Credit { get; set; }
        public int GpaCredit { get; set; }
        public int GradePoints { get; set; }
        public double CompletedCredit { get; set; }
        public int ContinuingEducationUnits { get; set; }
        public string Status { get; set; }
        public object StatusDate { get; set; }
        public string TermCode { get; set; }
        public List<object> MidTermGrades { get; set; }
        public string GradingType { get; set; }
        public string SectionNumber { get; set; }
        public bool HasVerifiedGrade { get; set; }
        public double AdjustedCredit { get; set; }
        public bool IsNonCourse { get; set; }
        public object StartDate { get; set; }
        public object EndDate { get; set; }
        public bool IsCompletedCredit { get; set; }
        public string ReplacedStatus { get; set; }
        public string ReplacementStatus { get; set; }
    }

    public class GradeRestriction
    {
        public bool IsRestricted { get; set; }
        public List<object> Reasons { get; set; }
    }

    public class AcademicData
    {
        public List<AcademicTerm> AcademicTerms { get; set; }
        public List<NonTermAcademicCredit> NonTermAcademicCredits { get; set; }
        public GradeRestriction GradeRestriction { get; set; }
        public double TotalCreditsCompleted { get; set; }
        public double OverallGradePointAverage { get; set; }
        public bool HasQuarters
        {
            get
            {
                if (AcademicTerms[0].TermComparer < DateTime.ParseExact("2012/7", "yyyy/M", CultureInfo.CurrentCulture))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public double PossibleCredits { get; set; }
        public double CreditsEarned
        {
            get
            {
                var cred = 0d;
                foreach (var term in AcademicTerms)
                {
                    foreach (var credit in term.AcademicCredits)
                    {
                        if (credit.GradeInfo != null)
                        {
                            if (credit.GradeInfo.Symbol != ":" && credit.GradeInfo.Symbol != "//")
                            {
                                cred += credit.CompletedCredit;
                            }
                        }
                    }
                }
                return cred;
            }
        }
        public double CalculatedGPA { get; set; }
        public string StudentId { get; set; }


    }
}