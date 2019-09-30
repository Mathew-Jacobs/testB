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

            var grades = GetGrades(bearerToken);

            obj = CalculateGPA(obj, grades);

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

            var books = new List<Book>();

            var schedule = GetClassSchedule(Id);
            foreach (var row in schedule.rows)
            {
                books.AddRange(row.booklist);
            }

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

            var schedule = GetClassSchedule(Id);

            return View(schedule);
        }

        public ActionResult ImportantDates()
        {
            var bearerToken = admin.Login();

            var obj = GetRegistrationDates(bearerToken);
            var temp = obj;
            foreach (var term in obj.ToList())
            {
                if (term.EndDate < DateTime.Now)
                {
                    temp.Remove(term);
                }
            }
            return View(temp);
        }

        public ActionResult FinancialChecklist()
        {
            var bearerToken = admin.Login();

            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };
            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", "/financial-aid-checklist-items", headers);
            dynamic jsonContent = JValue.Parse(response.Content);
            List<FinancialAidChecklist> obj = jsonContent.ToObject<List<FinancialAidChecklist>>();
            obj.Sort((a, b) => a.ChecklistSortNumber.CompareTo(b.ChecklistSortNumber));
            return View(obj);
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
        private Schedule GetClassSchedule(string Id)
        {
            Id = (long.Parse(Id) * 4567).ToString();
            var response = utility.CreateRequest(Method.GET, "https://rest.sinclair.edu/api/1/index.cfm", $"/Bulletin/CurrentCourses/{Id}");
            dynamic jsonContent = JValue.Parse(response.Content);
            Schedule sections = jsonContent.ToObject<Schedule>();

            return sections;
        }
        private List<Term> GetRegistrationDates(string bearerToken)
        {
            RequestHeader[] headers =
            {
                new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };

            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/terms/registration", headers);
            dynamic jsonContent = JValue.Parse(response.Content);
            List<Term> terms = jsonContent.ToObject<List<Term>>();
            return terms;
        }
        private List<Grade> GetGrades(string bearerToken)
        {
            RequestHeader[] headers =
            {
                    new RequestHeader() { Key = "Accept", Value = "application/vnd.ellucian.v1+json" },
                    new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
                };

            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/grades", headers);

            dynamic jsonContent = JValue.Parse(response.Content);
            List<Grade> obj = jsonContent.ToObject<List<Grade>>();

            return obj;
        }
        private AcademicData CalculateGPA(AcademicData academicData, List<Grade> grades)
        {
            List<string> validGrades = new List<string>() { "A", "B", "C", "D", "F", "Z" };
            Dictionary<string, string> symbolDesc = new Dictionary<string, string>()
            {
                { "i", "Course has been completed previously" },
                { "#", "Grade earned by proficiency examination" },
                { "//", "Course repeated; Not included in cumulative GPA" },
                { "=", "Course equates to and replaces a previous course" },
                { ":", "Grade not included in the Fresh Start Policy Calculation" },
            };
            var totalPossCredits = 0d;
            var totalGradePoints = 0d;
            foreach (var term in academicData.AcademicTerms)
            {
                var termCredit = 0d;
                var termGradePoints = 0d;
                foreach (var credit in term.AcademicCredits)
                {
                    if (credit.GradeInfo == null)
                    {
                        if (credit.ReplacedStatus == "Replaced" && grades.FirstOrDefault(x => x.Id == credit.VerifiedGradeId).Symbol == "")
                        {
                            var tempLetterGrade = grades.FirstOrDefault(x => x.Id == credit.VerifiedGradeId).LetterGrade.ToString();
                            credit.GradeInfo = new Grade()
                            {
                                Symbol = "//",
                                LetterGrade = tempLetterGrade + "//"
                            };
                        }
                        else if (grades.FirstOrDefault(x => x.Id == credit.VerifiedGradeId) == null)
                        {
                            credit.GradeInfo = new Grade
                            {
                                Description = "",
                                LetterGrade = ""
                            };
                        }
                        else
                        {
                            credit.GradeInfo = grades.FirstOrDefault(x => x.Id == credit.VerifiedGradeId);
                        }
                        if (symbolDesc.TryGetValue(credit.GradeInfo.Symbol, out string val))
                        {
                            credit.GradeInfo.Description = val;
                        }
                    }

                    if (credit.Title == "FRESH START" && term.FreshStart != true)
                    {
                        term.FreshStart = true;
                        int index = academicData.AcademicTerms.FindIndex(x => x.FreshStart == true);
                        for (int i = 0; i < index; i++)
                        {
                            foreach (var course in academicData.AcademicTerms[i].AcademicCredits)
                            {
                                var tempGrade = grades.FirstOrDefault(x => x.Id == course.VerifiedGradeId).LetterGrade.ToString();
                                if (tempGrade == "F" || tempGrade == "Z")
                                {
                                    course.GradeInfo = new Grade
                                    {
                                        LetterGrade = $"{tempGrade}:",
                                        Symbol = ":"
                                    };
                                    if (symbolDesc.TryGetValue(course.GradeInfo.Symbol, out string val))
                                    {
                                        course.GradeInfo.Description = val;
                                    }
                                }
                            }
                        }
                        academicData = CalculateGPA(academicData, grades);
                        return academicData;
                    }


                    if ((validGrades.Contains(credit.GradeInfo.LetterGrade) && credit.ReplacedStatus != "Replaced") || (credit.GradeInfo.Symbol == "#" && credit.ReplacedStatus != "Replaced"))
                    {
                        termCredit += credit.Credit;
                        termGradePoints += credit.GradePoints;
                    }
                }
                term.AcademicCredits.RemoveAll(x => (x.GradeInfo.LetterGrade == "CE" || x.GradeInfo.LetterGrade == "L"));
                term.Credits = termCredit;
                if (termCredit > 0)
                {
                    term.GradePointAverage = termGradePoints / termCredit;
                }
                if (term.Included != false)
                {
                    term.GPACredits = termCredit;
                    term.GradePoints = termGradePoints;
                    totalPossCredits += termCredit;
                    totalGradePoints += termGradePoints;
                }
                academicData.TotalCreditsCompleted += term.Credits;

            }
            foreach (var term in academicData.AcademicTerms.ToList())
            {
                if (term.AcademicCredits.Count == 0)
                {
                    academicData.AcademicTerms.Remove(term);
                }
            }
            academicData.PossibleCredits = totalPossCredits;
            if (totalPossCredits > 0)
            {
                academicData.CalculatedGPA = totalGradePoints / totalPossCredits;
            }
            return academicData;
        }
    }
}