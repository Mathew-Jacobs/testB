using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Portlets.MVC.Models;
using Portlets.Service.Admin;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PdfSharp.Drawing.Layout;

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
            obj.StudentId = Id;
            obj = CombineSummers(obj);
            obj = OrderByTerm(obj);
            var grades = GetGrades(bearerToken);
            obj = CalculateGPA(obj, grades);
            return View(obj);
        }

        public string GeneratePDF(AcademicData academicData)
        {
            Dictionary<string, string> validGrades = new Dictionary<string, string>() { { "A", "4 points" }, { "B", "3 points" }, { "C", "2 points" }, { "D", "1 points" }, { "F", "0 points" }, { "Z", "0 points (Did not attend)" } };
            Dictionary<string, string> invalidGrades = new Dictionary<string, string>() { { "AA", "Articulation Agreement" }, { "AC", "Articulated Credit" }, { "AP", "Advanced Placement" }, { "CE", "Continuing Education" }, { "CL", "College Level Examination Program (CLEP)" }, { "DS", "DANTES (DSST)" }, { "I", "Incomplete" }, { "IP", "In Progress" }, { "N", "Progress" }, { "P", "Pass" }, { "S", "Satisfactory Completion" }, { "U", "Unsatisfactory Progress" }, { "WC", "WEBCAPE" }, { "W", "Withdrawal" }, { "X", "Audit" }, { "Y", "Proficiency Credit" }, { "-", "No grade was assigned" } };
            Dictionary<string, string> symbols = new Dictionary<string, string>() { { "#", "Grade was earned by proficiency examination" }, { "//", "Course has been repeated; this grade is not included in current cumulative GPA" }, { "=", "Course equates to and replaces a previous course" }, { ":", "Grade not included in the Fresh Start Policy Calculation" } };
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = $"Unofficial Transcript";
            PdfPage pdfPage = pdf.AddPage();
            pdfPage.TrimMargins.All = 25;
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XTextFormatterEx2 textFormatter = new XTextFormatterEx2(graph);

            PdfPageSetup pageSetup = new PdfPageSetup()
            {
                TotalHeight = 0,
                Column = 0,
                AvailableHeight = pdfPage.Height.Point
            };
            int lastCharIndex;
            double neededHeight;

            XFont headerFont = new XFont("Gotham", 10, XFontStyle.Bold);
            XFont font = new XFont("Minion Pro", 10, XFontStyle.Regular);
            XRect rect;
            textFormatter.Alignment = XParagraphAlignment.Center;
            rect = new XRect(0, 0, pdfPage.Width.Point * 3 / 4, 100);
            textFormatter.PrepareDrawString("This is an unofficial transcript and is for personal use only.", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 5;
            textFormatter.DrawString(XBrushes.Black);
            if (academicData.HasQuarters)
            {
                textFormatter.Alignment = XParagraphAlignment.Center;
                rect = new XRect(5, pageSetup.TotalHeight + 5, (pdfPage.Width.Point * 3 / 4) - 10, 100);
                textFormatter.PrepareDrawString("Since August 2012 Sinclair Community College has been on the semester system. The credit hours earned prior to August 2012 have been converted to semester hours on the transcript.", font, rect, out lastCharIndex, out neededHeight);
                rect = new XRect(0, pageSetup.TotalHeight, pdfPage.Width.Point * 3 / 4, neededHeight + 10);
                graph.DrawRectangle(XBrushes.Bisque, rect);
                pageSetup.TotalHeight += neededHeight + 15;
                textFormatter.DrawString(XBrushes.Black);
            }
            textFormatter.Alignment = XParagraphAlignment.Left;
            rect = new XRect(0, pageSetup.TotalHeight, pdfPage.Width.Point * 3 / 4, 100);
            textFormatter.PrepareDrawString("Grades", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 3;
            textFormatter.DrawString(XBrushes.Black);

            textFormatter.Alignment = XParagraphAlignment.Left;
            rect = new XRect(0, pageSetup.TotalHeight, pdfPage.Width.Point * 3 / 8, 100);
            textFormatter.PrepareDrawString("Grades used in the calculation of grade point average", font, rect, out lastCharIndex, out neededHeight);
            textFormatter.DrawString(XBrushes.Black);

            textFormatter.Alignment = XParagraphAlignment.Left;
            rect = new XRect(pdfPage.Width.Point * 3 / 8, pageSetup.TotalHeight, pdfPage.Width.Point * 3 / 8, 100);
            textFormatter.PrepareDrawString("Grades not used in the calculation of grade point average", font, rect, out lastCharIndex, out neededHeight);
            textFormatter.DrawString(XBrushes.Black);

            pageSetup.TotalHeight += neededHeight;

            var tempHeight = double.Parse(pageSetup.TotalHeight.ToString());

            foreach (var grade in validGrades.Keys)
            {
                textFormatter.Alignment = XParagraphAlignment.Left;
                rect = new XRect(0, pageSetup.TotalHeight, 75, 100);
                textFormatter.PrepareDrawString($"{grade}", font, rect, out lastCharIndex, out neededHeight);
                textFormatter.DrawString(XBrushes.Black);
                textFormatter.Alignment = XParagraphAlignment.Left;
                rect = new XRect(75, pageSetup.TotalHeight, 125, 100);
                textFormatter.PrepareDrawString($"{validGrades[grade]}", font, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight;
                textFormatter.DrawString(XBrushes.Black);
            }

            var bottomOfGrades = double.Parse(pageSetup.TotalHeight.ToString());
            pageSetup.TotalHeight = tempHeight;

            foreach (var grade in invalidGrades.Keys)
            {
                textFormatter.Alignment = XParagraphAlignment.Left;
                rect = new XRect(pdfPage.Width.Point * 3 / 8, pageSetup.TotalHeight, 75, 100);
                textFormatter.PrepareDrawString($"{grade}", font, rect, out lastCharIndex, out neededHeight);
                textFormatter.DrawString(XBrushes.Black);
                textFormatter.Alignment = XParagraphAlignment.Left;
                rect = new XRect((pdfPage.Width.Point * 3 / 8) + 75, pageSetup.TotalHeight, (pdfPage.Width.Point * 3 / 8) + 125, 100);
                textFormatter.PrepareDrawString($"{invalidGrades[grade]}", font, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight;
                textFormatter.DrawString(XBrushes.Black);
            }

            tempHeight = double.Parse(pageSetup.TotalHeight.ToString());
            pageSetup.TotalHeight = bottomOfGrades + 5;

            textFormatter.Alignment = XParagraphAlignment.Left;
            rect = new XRect(0, pageSetup.TotalHeight, pdfPage.Width.Point * 3 / 4, 100);
            textFormatter.PrepareDrawString("Symbols", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 3;
            textFormatter.DrawString(XBrushes.Black);

            foreach (var symbol in symbols.Keys)
            {
                textFormatter.Alignment = XParagraphAlignment.Left;
                rect = new XRect(0, pageSetup.TotalHeight, 75, 100);
                textFormatter.PrepareDrawString($"{symbol}", font, rect, out lastCharIndex, out neededHeight);
                textFormatter.DrawString(XBrushes.Black);
                textFormatter.Alignment = XParagraphAlignment.Left;
                rect = new XRect(35, pageSetup.TotalHeight, 180, 100);
                textFormatter.PrepareDrawString($"{symbols[symbol]}", font, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight;
                textFormatter.DrawString(XBrushes.Black);
            }

            if (pageSetup.TotalHeight < tempHeight)
            {
                pageSetup.TotalHeight = tempHeight;
            }

            textFormatter.Alignment = XParagraphAlignment.Right;
            rect = new XRect(pdfPage.Width.Point * 2 / 3, 0, pdfPage.Width.Point * 1 / 3, 100);
            textFormatter.PrepareDrawString($"Tartan ID: {academicData.StudentId}\n\nDate Printed: {DateTime.Now.ToShortDateString()}", font, rect, out lastCharIndex, out neededHeight);
            textFormatter.DrawString(XBrushes.Black);
            if (neededHeight > pageSetup.TotalHeight)
            {
                pageSetup.TotalHeight = neededHeight;
            }
            textFormatter.Alignment = XParagraphAlignment.Center;
            textFormatter.DrawString("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", font, XBrushes.Black, new XRect((pdfPage.Width.Point / 2) * pageSetup.Column, pageSetup.TotalHeight, pdfPage.Width.Point / 2, pdfPage.Height.Point));
            MemoryStream ms = new MemoryStream();
            string path = Server.MapPath("/Output/");
            string fileName = "Sample.pdf";
            pdf.Save(path + fileName);
            pdf.Close();

            var result = new { file = fileName };

            return JsonConvert.SerializeObject(result);
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
        private void Draw(XTextFormatterEx2 tf, string text, PdfPageSetup pageSetup, XFont font, XRect rect, Padding padding)
        {

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