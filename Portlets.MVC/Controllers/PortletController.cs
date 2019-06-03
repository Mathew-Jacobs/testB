using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portlets.MVC.Models;
using Portlets.Service.Admin;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Portlets.MVC.Controllers
{
    public class PortletController : Controller
    {
        readonly Admin admin = new Admin();
        readonly Utility utility = new Utility();

        // Using Colleague API
        public ActionResult AccountSummary(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else if (Id.Length > 7)
            {
                Id = utility.TrimId(Id);
                if (string.IsNullOrEmpty(Id))
                {
                    return HttpNotFound();
                }
            }

            string bearerToken = admin.Login();

            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "Content-Type", Value = "application/json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };

            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", "account-due/term/admin/" + Id, headers);

            dynamic jsonContent = JValue.Parse(response.Content);
            StudentFinance obj = jsonContent.ToObject<StudentFinance>();

            return View(obj);
        }

        public ActionResult LatestGrades(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else if (Id.Length > 7)
            {
                Id = utility.TrimId(Id);
                if (string.IsNullOrEmpty(Id))
                {
                    return HttpNotFound();
                }
            }

            string bearerToken = admin.Login();

            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "Content-Type", Value = "application/json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };

            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/students/{Id}/academic-credits", headers);

            dynamic jsonContent = JValue.Parse(response.Content);
            AcademicData obj = jsonContent.ToObject<AcademicData>();
            obj = OrderByTerm(obj);
            return View(obj);
        }

        public ActionResult BookList(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else if (Id.Length > 7)
            {
                Id = utility.TrimId(Id);
                if (string.IsNullOrEmpty(Id))
                {
                    return HttpNotFound();
                }
            }

            string bearerToken = admin.Login();

            var books = new BookList()
            {
                Books = GetBookList(Id, bearerToken)
            };

            return View(books);
        }

        public ActionResult ClassSchedule(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return HttpNotFound();
            }
            else if (Id.Length > 7)
            {
                Id = utility.TrimId(Id);
                if (string.IsNullOrEmpty(Id))
                {
                    return HttpNotFound();
                }
            }

            string bearerToken = admin.Login();

            var classSchedule = new ClassSchedule()
            {
                CourseSections = GetClassSchedule(Id, bearerToken)
            };

            return View(classSchedule);
        }

        // Methods
        private AcademicData OrderByTerm(AcademicData academicData)
        {
            AcademicTerm temp;
            var now = DateTime.Now;
            int onlyNinetiesBabies = int.Parse(now.Year.ToString().Substring(2));
            for (int i = 0; i < academicData.AcademicTerms.Count - 1; i++)
            {
                for (int j = i + 1; j < academicData.AcademicTerms.Count; j++)
                {
                    var a = int.Parse(academicData.AcademicTerms[j].TermId.Substring(0, 2));
                    var b = int.Parse(academicData.AcademicTerms[i].TermId.Substring(0, 2));
                    var sA = academicData.AcademicTerms[j].TermId.Substring(3);
                    var sB = academicData.AcademicTerms[i].TermId.Substring(3);
                    if (a < b && ((b <= onlyNinetiesBabies && a <= onlyNinetiesBabies) || (b > onlyNinetiesBabies && a > onlyNinetiesBabies)))
                    {
                        temp = academicData.AcademicTerms[j];
                        academicData.AcademicTerms[j] = academicData.AcademicTerms[i];
                        academicData.AcademicTerms[i] = temp;
                    }
                    else if (a == b && sA != sB)
                    {
                        Enum.TryParse(academicData.AcademicTerms[j].TermId.Substring(3, 2), out SeasonValues seasonJ);
                        Enum.TryParse(academicData.AcademicTerms[i].TermId.Substring(3, 2), out SeasonValues seasonI);
                        if (seasonJ < seasonI)
                        {
                            temp = academicData.AcademicTerms[j];
                            academicData.AcademicTerms[j] = academicData.AcademicTerms[i];
                            academicData.AcademicTerms[i] = temp;
                        }
                        else if (seasonJ == SeasonValues.SU && seasonI == SeasonValues.SU)
                        {
                            if (string.Compare(academicData.AcademicTerms[j].TermId.Substring(5), academicData.AcademicTerms[i].TermId.Substring(5)) < 0)
                            {
                                temp = academicData.AcademicTerms[j];
                                academicData.AcademicTerms[j] = academicData.AcademicTerms[i];
                                academicData.AcademicTerms[i] = temp;
                            }
                        }
                    }
                }
            }
            return academicData;
        }
        private enum SeasonValues
        {
            WI,
            SP,
            SU,
            FA
        }
        private AcademicData GetCurrentClasses(string Id, string bearerToken)
        {

            string term = GetCurrentTerm(bearerToken);

            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "Content-Type", Value = "application/json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };

            var body = new { BestFit = false, Filter = false, IncludeStudentSections = true, StudentIds = new string[] { Id }, term };


            var response = utility.CreateRequest(Method.POST, "https://api.sinclair.edu/colleagueapi", $"/qapi/academic-history", headers, body);
            dynamic jsonContent = JValue.Parse(response.Content);
            List<AcademicData> obj = jsonContent.ToObject<List<AcademicData>>();
            return obj[0];
        }
        private string GetCurrentTerm(string bearerToken)
        {
            var date = DateTime.Now;
            var year = date.Year.ToString().Substring(2);
            List<Term> terms = new List<Term>();

            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "Content-Type", Value = "application/json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };
            var winterRe = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/terms?id={year}/WI", headers);
            if (winterRe.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonWinter = JValue.Parse(winterRe.Content);
                Term winterTerm = jsonWinter.ToObject<Term>();
                terms.Add(winterTerm);
            }
            var springRe = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/terms?id={year}/SP", headers);
            if (springRe.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonSpring = JValue.Parse(springRe.Content);
                Term springTerm = jsonSpring.ToObject<Term>();
                terms.Add(springTerm);
            }
            var summerRe = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/terms?id={year}/SU", headers);
            if (summerRe.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonSummer = JValue.Parse(summerRe.Content);
                Term summerTerm = jsonSummer.ToObject<Term>();
                terms.Add(summerTerm);
            }
            var fallRe = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/terms?id={year}/FA", headers);
            if (fallRe.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonFall = JValue.Parse(fallRe.Content);
                Term fallTerm = jsonFall.ToObject<Term>();
                terms.Add(fallTerm);
            }
            foreach (var term in terms)
            {
                if (term.StartDate < date && term.EndDate > date)
                {
                    return term.Code;
                }
            }
            return null;
        }
        private List<Book> GetBookList(string Id, string bearerToken)
        {
            List<Book> books = new List<Book>();
            AcademicData data = GetCurrentClasses(Id, bearerToken);
            List<CourseCatalog> catalogs = new List<CourseCatalog>();
            string courseIds = "";

            if (data.AcademicTerms.Count == 0)
            {
                return null;
            }

            foreach (var course in data.AcademicTerms[0].AcademicCredits)
            {
                courseIds = courseIds + course.CourseId + ",";

                var keyword = course.CourseName;
                var term = data.AcademicTerms[0].TermId;
                var subjectCode = course.CourseName.Split('-')[0];
                var subjectNum = course.CourseName.Split('-')[1];

                var response = utility.CreateRequest(Method.GET, "https://rest.sinclair.edu/api/1/index.cfm", $"/Bulletin/Sections/{subjectCode}/{subjectNum}/{term.Replace("/", "")}?keyword={course.CourseName}&courseList=any&term={term.Replace("/", "")}&subjectCode={subjectCode}&building=any&courseFormat=all&termFormat=all&creditHoursMin=0&creditHoursMax=15&timeChoice=segments&segOptions=any");
                dynamic jsonContent = JValue.Parse(response.Content);
                CourseCatalog obj = jsonContent.ToObject<CourseCatalog>();
                catalogs.Add(obj);
            }

            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };

            var response2 = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/courses/sections?courseIds={courseIds}", headers);
            dynamic jsonContent2 = JValue.Parse(response2.Content);
            List<CourseSection> sections = jsonContent2.ToObject<List<CourseSection>>();

            foreach (var courseCatalog in catalogs)
            {
                foreach (var row in courseCatalog.rows)
                {
                    foreach (var section in sections)
                    {
                        if (section.ActiveStudentIds.Contains(Id) && row.CourseKey.ToString() == section.Id)
                        {
                            foreach (var book in row.booklist)
                            {
                                books.Add(book);
                            }
                        }
                    }
                }
            }
            List<Book> results = books.Distinct().ToList();
            return results;
        }

        private List<CourseInfo> GetClassSchedule(string Id, string bearerToken)
        {
            List<CourseInfo> info = new List<CourseInfo>();
            List<CourseSection> courses = new List<CourseSection>();

            List<CourseCatalog> catalogs = new List<CourseCatalog>();
            List<string> facultyIds = new List<string>();
            AcademicData data = GetCurrentClasses(Id, bearerToken);
            List<Location> loc = GetLocations(bearerToken);
            string courseIds = "";
            foreach (var course in data.AcademicTerms[0].AcademicCredits)
            {
                courseIds = courseIds + course.CourseId + ",";

                var keyword = course.CourseName;
                var term = data.AcademicTerms[0].TermId;
                var subjectCode = course.CourseName.Split('-')[0];
                var subjectNum = course.CourseName.Split('-')[1];

                var r = utility.CreateRequest(Method.GET, "https://rest.sinclair.edu/api/1/index.cfm", $"/Bulletin/Sections/{subjectCode}/{subjectNum}/{term.Replace("/", "")}?keyword={course.CourseName}&courseList=any&term={term.Replace("/", "")}&subjectCode={subjectCode}&building=any&courseFormat=all&termFormat=all&creditHoursMin=0&creditHoursMax=15&timeChoice=segments&segOptions=any");
                dynamic jC = JValue.Parse(r.Content);
                CourseCatalog obj = jC.ToObject<CourseCatalog>();
                catalogs.Add(obj);
            }

            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };

            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/courses/sections?courseIds={courseIds}", headers);
            dynamic jsonContent = JValue.Parse(response.Content);
            List<CourseSection> sections = jsonContent.ToObject<List<CourseSection>>();

            foreach (var course in sections)
            {
                if (course.ActiveStudentIds.Contains(Id) && !courses.Contains(course))
                {
                    courses.Add(course);
                    foreach (var id in course.FacultyIds)
                    {
                        if (!facultyIds.Contains(id.ToString()))
                        {
                            facultyIds.Add(id.ToString());
                        }
                    }

                }
            }
            RequestHeader[] facultyHeaders =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken },
                new RequestHeader() { Key = "Content-Type", Value = "application/json" }
            };
            var body = new { FacultyIds = facultyIds.ToArray() };
            var facultyRe = utility.CreateRequest(Method.POST, "https://api.sinclair.edu/colleagueapi", "/qapi/faculty", headers, body);
            dynamic jsonFaculty = JValue.Parse(facultyRe.Content);
            List<Faculty> faculty = jsonFaculty.ToObject<List<Faculty>>();
            foreach (var course in courses)
            {
                course.Faculties = new List<Faculty>();
                foreach (var id in course.FacultyIds)
                {
                    foreach (var person in faculty)
                    {
                        if (person.Id == id.ToString())
                        {
                            course.Faculties.Add(person);
                        }
                    }
                }
            }

            foreach (var courseCatalog in catalogs)
            {
                foreach (var row in courseCatalog.rows)
                {
                    foreach (var section in sections)
                    {
                        if (section.ActiveStudentIds.Contains(Id) && row.CourseKey.ToString() == section.Id)
                        {
                            foreach (var location in loc)
                            {
                                if ((location.Code == section.Location || section.Location == ""))
                                {
                                    CourseInfo temp = new CourseInfo()
                                    {
                                        CourseId = section.CourseId,
                                        Instructors = section.Faculties,
                                        CourseName = row.CourseCode,
                                        Loc = location,
                                        Meetings = section.Meetings,
                                        Title = row.LongName,
                                        StartDate = section.StartDate,
                                        EndDate = section.EndDate,
                                        Room = row.building,
                                        Section = row.SectionNo.ToString()
                                    };

                                    info.Add(temp);
                                }
                            }
                        }
                    }
                }
            }

            return info;
        }

        private List<Location> GetLocations(string bearerToken)
        {
            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };
            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", "/locations", headers);
            dynamic jsonResponse = JValue.Parse(response.Content);
            return jsonResponse.ToObject<List<Location>>();
        }

        private static bool FindISBN(Book bk, string isbn)
        {
            if (bk.isbn == isbn)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}