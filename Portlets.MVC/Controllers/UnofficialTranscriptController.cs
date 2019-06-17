using Newtonsoft.Json.Linq;
using Portlets.MVC.Models;
using Portlets.Service.Admin;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portlets.MVC.Controllers
{
    public class UnofficialTranscriptController : Controller
    {

        readonly Admin admin = new Admin();
        readonly Utility utility = new Utility();

        // GET: UnofficialTranscript
        public ActionResult Index(string Id)
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
                new RequestHeader() { Key = "X-CustomCredentials", Value = bearerToken }
            };

            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/colleagueapi", $"/students/{Id}/academic-credits", headers);

            dynamic jsonContent = JValue.Parse(response.Content);
            AcademicData obj = jsonContent.ToObject<AcademicData>();
            obj = CombineSummers(obj);
            obj = OrderByTerm(obj);
            var grades = GetGrades(bearerToken);
            obj = CalculateGPA(obj, grades);
            return View(obj);
        }
        private AcademicData OrderByTerm(AcademicData academicData)
        {
            academicData.AcademicTerms.Sort((a, b) => a.TermComparer.CompareTo(b.TermComparer));

            return academicData;
        }

        private AcademicData CombineSummers(AcademicData academicData)
        {
            List<string> validGrades = new List<string>() { "A", "B", "C", "D", "F", "Z" };
            var dupes = academicData.AcademicTerms.GroupBy(x => x.TermComparer)
                .SelectMany(group => group).ToList();
            var summerTerms = academicData.AcademicTerms.Where(x => x.TermSeason == "Summer").ToList();
            foreach (var term in summerTerms)
            {
                academicData.AcademicTerms.Remove(term);
            }
            var uniqueSummers = summerTerms.GroupBy(x => x.TermYear)
                .Where(group => group.Count() >= 1)
                .Select(group => group.Key).ToList();
            List<AcademicTerm> tempTerms = new List<AcademicTerm>();
            foreach (var year in uniqueSummers)
            {
                var terms = summerTerms.Where(x => x.TermYear == year).ToList();
                var attemptedCredits = 0d;
                var gradePoints = 0d;
                var academicTerm = new AcademicTerm()
                {
                    TermId = terms[0].TermId,
                    AcademicCredits = new List<AcademicCredit>()
                };
                foreach (var term in terms)
                {
                    foreach (var credit in term.AcademicCredits)
                    {
                        if (validGrades.Contains(credit.VerifiedGradeId))
                        {
                            attemptedCredits += credit.Credit;
                            gradePoints += credit.GradePoints;
                            academicTerm.Credits += credit.CompletedCredit;
                        }
                        academicTerm.AcademicCredits.Add(credit);
                    }
                }
                if (attemptedCredits != 0)
                {
                    academicTerm.GradePointAverage = gradePoints / attemptedCredits;
                }
                tempTerms.Add(academicTerm);
            }
            foreach (var term in tempTerms)
            {
                academicData.AcademicTerms.Add(term);
            }

            return academicData;
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

                    
                    if (credit.GradeInfo.LetterGrade == "CE" || credit.GradeInfo.LetterGrade == "L")
                    {
                        term.AcademicCredits.Remove(credit);
                    }
                    if ((validGrades.Contains(credit.GradeInfo.LetterGrade) && credit.ReplacedStatus != "Replaced") || (credit.GradeInfo.Symbol == "#" && credit.ReplacedStatus != "Replaced"))
                    {
                        termCredit += credit.Credit;
                        termGradePoints += credit.GradePoints;
                    }
                }
                term.Credits = termCredit;
                if (termCredit > 0)
                {
                    term.GradePointAverage = termGradePoints / termCredit;
                }
                if (term.Included != false)
                {
                    totalPossCredits += termCredit;
                    totalGradePoints += termGradePoints;
                }
                academicData.TotalCreditsCompleted += term.Credits;
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
        private enum SeasonValues
        {
            WI,
            SP,
            SU,
            FA
        }
    }

}