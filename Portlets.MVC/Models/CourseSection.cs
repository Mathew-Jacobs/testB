using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Meeting
    {
        public string InstructionalMethodCode { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<object> Days { get; set; }
        public string Room { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; }
        public bool IsOnline { get; set; }
    }

    public class SectionCharge
    {
        public string Id { get; set; }
        public string ChargeCode { get; set; }
        public double BaseAmount { get; set; }
        public bool IsFlatFee { get; set; }
        public bool IsRuleBased { get; set; }
    }

    public class CourseSection
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string TermId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Number { get; set; }
        public string CourseName { get; set; }
        public int MinimumCredits { get; set; }
        public object MaximumCredits { get; set; }
        public object VariableCreditIncrement { get; set; }
        public object Ceus { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public List<Meeting> Meetings { get; set; }
        public List<object> FacultyIds { get; set; }
        public List<Faculty> Faculties { get; set; }
        public List<object> Books { get; set; }
        public List<object> ActiveStudentIds { get; set; }
        public string LearningProvider { get; set; }
        public object LearningProviderSiteId { get; set; }
        public string PrimarySectionId { get; set; }
        public bool AllowPassNoPass { get; set; }
        public bool AllowAudit { get; set; }
        public bool OnlyPassNoPass { get; set; }
        public int Capacity { get; set; }
        public int? Available { get; set; }
        public bool WaitlistAvailable { get; set; }
        public bool IsActive { get; set; }
        public int Waitlisted { get; set; }
        public bool OverridesCourseRequisites { get; set; }
        public List<object> Requisites { get; set; }
        public List<object> SectionRequisites { get; set; }
        public string AcademicLevelCode { get; set; }
        public object BookstoreUrl { get; set; }
        public string Comments { get; set; }
        public string TransferStatus { get; set; }
        public string TopicCode { get; set; }
        public string GradeSchemeCode { get; set; }
        public List<SectionCharge> SectionCharges { get; set; }
        public bool ExcludeFromAddAuthorization { get; set; }
    }
}