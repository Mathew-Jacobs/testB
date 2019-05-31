using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class DayModel
    {
        public string location { get; set; }
        public string notes { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public object SectionNo { get; set; }
        public string title { get; set; }
        public int? eventId { get; set; }
        public string term { get; set; }
        public string satLocation { get; set; }
        public int? calendarId { get; set; }
    }

    public class Row
    {
        public string location { get; set; }
        public int? sortOrderTwo { get; set; }
        public string startDate { get; set; }
        public string regEndDate { get; set; }
        public string EndTime { get; set; }
        public string flexFlag { get; set; }
        public string Days { get; set; }
        public int? seatCount { get; set; }
        public string comments { get; set; }
        public string restrictions { get; set; }
        public string courseSysnonum { get; set; }
        public string LongName { get; set; }
        public string Term { get; set; }
        public string specialProperty { get; set; }
        public string satLocation { get; set; }
        public string division { get; set; }
        public int? seatCapacity { get; set; }
        public string DaysOld { get; set; }
        public string AltTitle { get; set; }
        public string status { get; set; }
        public string Quarter { get; set; }
        public string building { get; set; }
        public string Description { get; set; }
        public string withoutDate { get; set; }
        public string SubjectCode { get; set; }
        public double? otherFee { get; set; }
        public string regStartDate { get; set; }
        public List<DayModel> dayModels { get; set; }
        public object SectionNo { get; set; }
        public int? sortOrderOne { get; set; }
        public int? CreditHours { get; set; }
        public List<Book> booklist { get; set; }
        public string divid { get; set; }
        public int? CourseNo { get; set; }
        public string withDate { get; set; }
        public string endDate { get; set; }
        public string CourseCode { get; set; }
        public object SectionLoc { get; set; }
        public List<object> additionalSched { get; set; }
        public int? openSeats { get; set; }
        public string StartTime { get; set; }
        public string Faculty { get; set; }
        public int? CourseKey { get; set; }
        public string labFee { get; set; }
        public string printedComments { get; set; }
        public int? CourseNumber { get; set; }
        public string waitListAllowed { get; set; }
    }

    public class CourseCatalog
    {
        public int? results { get; set; }
        public bool success { get; set; }
        public List<Row> rows { get; set; }
    }
}