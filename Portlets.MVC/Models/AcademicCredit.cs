using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public string TermId { get; set; }
        public double GradePointAverage { get; set; }
        public double Credits { get; set; }
        public int ContinuingEducationUnits { get; set; }
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
        public int TotalCreditsCompleted { get; set; }
        public int OverallGradePointAverage { get; set; }
        public object StudentId { get; set; }
    }
}