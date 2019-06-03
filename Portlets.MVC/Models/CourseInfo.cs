using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class CourseInfo
    {
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Meeting> Meetings { get; set; }
        public List<Faculty> Instructors { get; set; }
        public Location Loc { get; set; }
        public string Room { get; set; }
        public string Section { get; set; }
    }
}