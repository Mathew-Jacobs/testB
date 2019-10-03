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
            var student = GetStudent(obj.StudentId);
            obj.StudentData = student;
            var creditIDs = new List<string>();
            foreach (var term in obj.AcademicTerms)
            {
                foreach (var credit in term.AcademicCredits)
                {
                    creditIDs.Add(credit.Id);
                }
            }

            RequestHeader[] noteHeaders =
            {
                new RequestHeader() { Key = "Content-Type", Value = "application/json" }
            };

            var noteResponse = utility.CreateRequest(Method.POST, "https://apitest.sinclair.edu/portlet/api", $"coursefinalnotes", noteHeaders, creditIDs);

            if (noteResponse.IsSuccessful)
            {
                dynamic noteJsonContent = JValue.Parse(noteResponse.Content);
                List<CourseFinalNote> helperObj = noteJsonContent.ToObject<List<CourseFinalNote>>();

                foreach (var note in helperObj)
                {
                    foreach (var term in obj.AcademicTerms)
                    {
                        foreach (var course in term.AcademicCredits)
                        {
                            if (note.studentAcadCredKey == course.Id)
                            {
                                course.Note = note.scsFinalNote;
                                if (course.Note == "MR")
                                {
                                    course.ReplacedStatus = "Replaced";
                                }
                            }
                        }
                    }
                }
            }

            obj = CalculateGPA(obj, grades);
            return View(obj);
        }

        public string GeneratePDF(AcademicData academicData)
        {
            Dictionary<string, string> validGrades = new Dictionary<string, string>() { { "A", "4 points" }, { "B", "3 points" }, { "C", "2 points" }, { "D", "1 points" }, { "F", "0 points" }, { "Z", "0 points (Did not attend)" } };
            Dictionary<string, string> invalidGrades = new Dictionary<string, string>() { { "AA", "Articulation Agreement" }, { "AC", "Articulated Credit" }, { "AP", "Advanced Placement" }, { "CE", "Continuing Education" }, { "CL", "College Level Examination Program (CLEP)" }, { "DS", "DANTES (DSST)" }, { "I", "Incomplete" }, { "IP", "In Progress" }, { "N", "Progress" }, { "P", "Pass" }, { "S", "Satisfactory Completion" }, { "U", "Unsatisfactory Progress" }, { "WC", "WEBCAPE" }, { "W", "Withdrawal" }, { "X", "Audit" }, { "Y", "Proficiency Credit" }, { "-", "No grade was assigned" } };
            Dictionary<string, string> symbols = new Dictionary<string, string>() { { "#", "Grade was earned by proficiency examination" }, { "//", "Course has been repeated; this grade is not included in current cumulative GPA" }, { "=", "Course equates to and replaces a previous course" }, { ":", "Grade not included in the Fresh Start Policy Calculation" } };
            //PdfDocument pdf = new PdfDocument();
            //pdf.Info.Title = $"Unofficial Transcript";
            //PdfPage pdfPage = pdf.AddPage();
            //pdfPage.TrimMargins.All = 25;
            //XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            //XTextFormatterEx2 textFormatter = new XTextFormatterEx2(graph);

            PdfPageSetup pageSetup = new PdfPageSetup();
            pageSetup.Pdf = new PdfDocument();
            pageSetup.Pdf.Info.Title = "Unofficial Transcript";
            pageSetup.PDFPage = pageSetup.Pdf.AddPage();
            pageSetup.PDFPage.TrimMargins.All = 25;
            pageSetup.Graph = XGraphics.FromPdfPage(pageSetup.PDFPage);
            pageSetup.TF = new XTextFormatterEx2(pageSetup.Graph);
            pageSetup.AvailableHeight = pageSetup.PDFPage.Height.Point;
            int lastCharIndex;
            double neededHeight;

            XFont headerFont = new XFont("Gotham", 10, XFontStyle.Bold);
            XFont font = new XFont("Minion Pro", 10, XFontStyle.Regular);
            XRect rect;
            pageSetup.TF.Alignment = XParagraphAlignment.Center;
            rect = new XRect(0, 0, pageSetup.PDFPage.Width.Point * 3 / 4, 100);
            pageSetup.TF.PrepareDrawString("This is an unofficial transcript and is for personal use only.", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 5;
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Center;
                rect = new XRect(0, 0, pageSetup.PDFPage.Width.Point * 3 / 4, 100);
                pageSetup.TF.PrepareDrawString("This is an unofficial transcript and is for personal use only.", headerFont, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight + 5;
            }
            pageSetup.TF.DrawString(XBrushes.Black);
            if (academicData.HasQuarters)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Center;
                rect = new XRect(5, pageSetup.TotalHeight + 5, (pageSetup.PDFPage.Width.Point * 3 / 4) - 10, 100);
                pageSetup.TF.PrepareDrawString("Since August 2012 Sinclair Community College has been on the semester system. The credit hours earned prior to August 2012 have been converted to semester hours on the transcript.", font, rect, out lastCharIndex, out neededHeight);
                rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 4, neededHeight + 10);
                pageSetup.Graph.DrawRectangle(XBrushes.Bisque, rect);
                pageSetup.TotalHeight += neededHeight + 15;
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Center;
                    rect = new XRect(5, pageSetup.TotalHeight + 5, (pageSetup.PDFPage.Width.Point * 3 / 4) - 10, 100);
                    pageSetup.TF.PrepareDrawString("Since August 2012 Sinclair Community College has been on the semester system. The credit hours earned prior to August 2012 have been converted to semester hours on the transcript.", font, rect, out lastCharIndex, out neededHeight);
                    pageSetup.TotalHeight += neededHeight + 15;
                }
                pageSetup.TF.DrawString(XBrushes.Black);
            }
            pageSetup.TF.Alignment = XParagraphAlignment.Left;
            rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 4, 100);
            pageSetup.TF.PrepareDrawString("Grades", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 3;
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 4, 100);
                pageSetup.TF.PrepareDrawString("Grades", headerFont, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight + 3;
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Left;
            rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 8, 100);
            pageSetup.TF.PrepareDrawString("Grades used in the calculation of grade point average", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 8, 100);
                pageSetup.TF.PrepareDrawString("Grades used in the calculation of grade point average", font, rect, out lastCharIndex, out neededHeight);

            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Left;
            rect = new XRect(pageSetup.PDFPage.Width.Point * 3 / 8, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 8, 100);
            pageSetup.TF.PrepareDrawString("Grades not used in the calculation of grade point average", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(pageSetup.PDFPage.Width.Point * 3 / 8, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 8, 100);
                pageSetup.TF.PrepareDrawString("Grades not used in the calculation of grade point average", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TotalHeight += neededHeight;

            var tempHeight = double.Parse(pageSetup.TotalHeight.ToString());

            foreach (var grade in validGrades.Keys)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(0, pageSetup.TotalHeight, 75, 100);
                pageSetup.TF.PrepareDrawString($"{grade}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Left;
                    rect = new XRect(0, pageSetup.TotalHeight, 75, 100);
                    pageSetup.TF.PrepareDrawString($"{grade}", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(75, pageSetup.TotalHeight, 125, 100);
                pageSetup.TF.PrepareDrawString($"{validGrades[grade]}", font, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight;
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Left;
                    rect = new XRect(75, pageSetup.TotalHeight, 125, 100);
                    pageSetup.TF.PrepareDrawString($"{validGrades[grade]}", font, rect, out lastCharIndex, out neededHeight);
                    pageSetup.TotalHeight += neededHeight;
                }
                pageSetup.TF.DrawString(XBrushes.Black);
            }

            var bottomOfGrades = double.Parse(pageSetup.TotalHeight.ToString());
            pageSetup.TotalHeight = tempHeight;

            foreach (var grade in invalidGrades.Keys)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(pageSetup.PDFPage.Width.Point * 3 / 8, pageSetup.TotalHeight, 75, 100);
                pageSetup.TF.PrepareDrawString($"{grade}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Left;
                    rect = new XRect(pageSetup.PDFPage.Width.Point * 3 / 8, pageSetup.TotalHeight, 75, 100);
                    pageSetup.TF.PrepareDrawString($"{grade}", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect((pageSetup.PDFPage.Width.Point * 3 / 8) + 75, pageSetup.TotalHeight, (pageSetup.PDFPage.Width.Point * 3 / 8) + 125, 100);
                pageSetup.TF.PrepareDrawString($"{invalidGrades[grade]}", font, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight;
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Left;
                    rect = new XRect((pageSetup.PDFPage.Width.Point * 3 / 8) + 75, pageSetup.TotalHeight, (pageSetup.PDFPage.Width.Point * 3 / 8) + 125, 100);
                    pageSetup.TF.PrepareDrawString($"{invalidGrades[grade]}", font, rect, out lastCharIndex, out neededHeight);
                    pageSetup.TotalHeight += neededHeight;
                }
                pageSetup.TF.DrawString(XBrushes.Black);
            }

            tempHeight = double.Parse(pageSetup.TotalHeight.ToString());
            pageSetup.TotalHeight = bottomOfGrades + 5;

            pageSetup.TF.Alignment = XParagraphAlignment.Left;
            rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point * 3 / 4, 100);
            pageSetup.TF.PrepareDrawString("Symbols", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 3;
            if (!pageSetup.TF._preparedText)
            {

            }
            pageSetup.TF.DrawString(XBrushes.Black);

            foreach (var symbol in symbols.Keys)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(0, pageSetup.TotalHeight, 75, 100);
                pageSetup.TF.PrepareDrawString($"{symbol}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {

                }
                pageSetup.TF.DrawString(XBrushes.Black);
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(35, pageSetup.TotalHeight, 180, 100);
                pageSetup.TF.PrepareDrawString($"{symbols[symbol]}", font, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight;
                if (!pageSetup.TF._preparedText)
                {

                }
                pageSetup.TF.DrawString(XBrushes.Black);
            }

            if (pageSetup.TotalHeight < tempHeight)
            {
                pageSetup.TotalHeight = tempHeight;
            }

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(pageSetup.PDFPage.Width.Point * 2 / 3, 0, pageSetup.PDFPage.Width.Point * 1 / 3, 100);
            pageSetup.TF.PrepareDrawString($"Tartan ID: {academicData.StudentId}\n\nName: {academicData.StudentData.firstName} {academicData.StudentData.lastName}\n\nAddress: {academicData.StudentData.address}\n\t{academicData.StudentData.city} {academicData.StudentData.state} {academicData.StudentData.zipCode}\n\nDate Printed: {DateTime.Now.ToShortDateString()}", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {

            }
            pageSetup.TF.DrawString(XBrushes.Black);
            if (neededHeight > pageSetup.TotalHeight)
            {
                pageSetup.TotalHeight = neededHeight;
            }

            pageSetup.TotalHeight += 10;

            pageSetup.TF.Alignment = XParagraphAlignment.Center;
            rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, 100);
            pageSetup.TF.PrepareDrawString("Beginning of Record", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 5;
            if (!pageSetup.TF._preparedText)
            {

            }
            pageSetup.TF.DrawString(XBrushes.Black);

            XPen lineBlack2 = new XPen(XColors.Black, 2);
            XPen lineBlack1 = new XPen(XColors.Black, 1);

            pageSetup.Graph.DrawLine(lineBlack2, 0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, pageSetup.TotalHeight);

            pageSetup.TotalHeight += 10;

            var bottomFooter = pageSetup.TotalHeight;

            var courseNameX = 34d;
            var courseTitleX = courseNameX + 75d;
            var attCredX = courseTitleX + 175;
            var earnCredX = attCredX + 75;
            var courseGradeX = earnCredX + 75;
            var courseGPAPtsX = courseGradeX + 75;

            foreach (var term in academicData.AcademicTerms)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Center;
                rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, 100);
                pageSetup.TF.PrepareDrawString($"{term.TermSeason} {term.TermYear}", headerFont, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight + 3;
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Center;
                    rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, 100);
                    pageSetup.TF.PrepareDrawString($"{term.TermSeason} {term.TermYear}", headerFont, rect, out lastCharIndex, out neededHeight);
                    pageSetup.TotalHeight += neededHeight + 3;
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
                pageSetup.TF.PrepareDrawString($"Course", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Left;
                    rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
                    pageSetup.TF.PrepareDrawString($"Course", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Center;
                rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                pageSetup.TF.PrepareDrawString($"Title", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Center;
                    rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                    pageSetup.TF.PrepareDrawString($"Title", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                pageSetup.TF.PrepareDrawString($"Att Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                    pageSetup.TF.PrepareDrawString($"Att Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                pageSetup.TF.PrepareDrawString($"E Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                    pageSetup.TF.PrepareDrawString($"E Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                pageSetup.TF.PrepareDrawString($"Grade", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                    pageSetup.TF.PrepareDrawString($"Grade", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                pageSetup.TF.PrepareDrawString($"GPA Pts", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                    pageSetup.TF.PrepareDrawString($"GPA Pts", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TotalHeight += neededHeight;

                double mostHeight = 0;

                foreach (var credit in term.AcademicCredits)
                {
                    mostHeight = 0;
                    if (!string.IsNullOrEmpty(credit.CourseName))
                    {
                        pageSetup.TF.Alignment = XParagraphAlignment.Left;
                        rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
                        pageSetup.TF.PrepareDrawString($"{credit.CourseName}", font, rect, out lastCharIndex, out neededHeight);
                        if (mostHeight < neededHeight)
                        {
                            mostHeight = neededHeight;
                        }
                        if (!pageSetup.TF._preparedText)
                        {
                            pageSetup.TF.Alignment = XParagraphAlignment.Left;
                            rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
                            pageSetup.TF.PrepareDrawString($"{credit.CourseName}", font, rect, out lastCharIndex, out neededHeight);
                        }
                        pageSetup.TF.DrawString(XBrushes.Black);
                    }
                    if (!string.IsNullOrEmpty(credit.Title))
                    {
                        pageSetup.TF.Alignment = XParagraphAlignment.Left;
                        rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                        pageSetup.TF.PrepareDrawString($"{credit.Title}", font, rect, out lastCharIndex, out neededHeight);
                        if (mostHeight < neededHeight)
                        {
                            mostHeight = neededHeight;
                        }
                        if (!pageSetup.TF._preparedText)
                        {
                            pageSetup.TF.Alignment = XParagraphAlignment.Left;
                            rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                            pageSetup.TF.PrepareDrawString($"{credit.Title}", font, rect, out lastCharIndex, out neededHeight);
                        }
                        pageSetup.TF.DrawString(XBrushes.Black);
                    }
                    if (!string.IsNullOrEmpty(credit.Credit.ToString()))
                    {
                        pageSetup.TF.Alignment = XParagraphAlignment.Right;
                        rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                        pageSetup.TF.PrepareDrawString($"{Math.Round(credit.Credit, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                        if (mostHeight < neededHeight)
                        {
                            mostHeight = neededHeight;
                        }
                        if (!pageSetup.TF._preparedText)
                        {
                            pageSetup.TF.Alignment = XParagraphAlignment.Right;
                            rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                            pageSetup.TF.PrepareDrawString($"{Math.Round(credit.Credit, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                        }
                        pageSetup.TF.DrawString(XBrushes.Black);
                    }
                    if (!string.IsNullOrEmpty(credit.CompletedCredit.ToString()))
                    {
                        pageSetup.TF.Alignment = XParagraphAlignment.Right;
                        rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                        pageSetup.TF.PrepareDrawString($"{Math.Round(credit.CompletedCredit, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                        if (mostHeight < neededHeight)
                        {
                            mostHeight = neededHeight;
                        }
                        if (!pageSetup.TF._preparedText)
                        {
                            pageSetup.TF.Alignment = XParagraphAlignment.Right;
                            rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                            pageSetup.TF.PrepareDrawString($"{Math.Round(credit.CompletedCredit, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                        }
                        pageSetup.TF.DrawString(XBrushes.Black);
                    }
                    if (credit.GradeInfo.LetterGrade != null)
                    {

                        if (!string.IsNullOrEmpty(credit.GradeInfo.LetterGrade.ToString()))
                        {
                            pageSetup.TF.Alignment = XParagraphAlignment.Right;
                            rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                            pageSetup.TF.PrepareDrawString($"{credit.GradeInfo.LetterGrade}", font, rect, out lastCharIndex, out neededHeight);
                            if (mostHeight < neededHeight)
                            {
                                mostHeight = neededHeight;
                            }
                            if (!pageSetup.TF._preparedText)
                            {
                                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                                rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                                pageSetup.TF.PrepareDrawString($"{credit.GradeInfo.LetterGrade}", font, rect, out lastCharIndex, out neededHeight);
                            }
                            pageSetup.TF.DrawString(XBrushes.Black);
                        }
                    }
                    if (!string.IsNullOrEmpty(credit.GradePoints.ToString()))
                    {
                        pageSetup.TF.Alignment = XParagraphAlignment.Right;
                        rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                        pageSetup.TF.PrepareDrawString($"{Math.Round(credit.GradePoints, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                        if (mostHeight < neededHeight)
                        {
                            mostHeight = neededHeight;
                        }
                        if (!pageSetup.TF._preparedText)
                        {
                            pageSetup.TF.Alignment = XParagraphAlignment.Right;
                            rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                            pageSetup.TF.PrepareDrawString($"{Math.Round(credit.GradePoints, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                        }
                        pageSetup.TF.DrawString(XBrushes.Black);
                    }
                    pageSetup.TotalHeight += mostHeight;
                }
                pageSetup.TotalHeight += 5;

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                pageSetup.TF.PrepareDrawString($"GPA Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                    pageSetup.TF.PrepareDrawString($"GPA Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TotalHeight += neededHeight;

                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
                pageSetup.TF.PrepareDrawString($"Term GPA", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Left;
                    rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
                    pageSetup.TF.PrepareDrawString($"Term GPA", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(term.GradePointAverage, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Left;
                    rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                    pageSetup.TF.PrepareDrawString($"{Math.Round(term.GradePointAverage, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                pageSetup.TF.PrepareDrawString($"Term Total", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                    pageSetup.TF.PrepareDrawString($"Term Total", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(term.Credits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                    pageSetup.TF.PrepareDrawString($"{Math.Round(term.Credits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(term.Credits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                    pageSetup.TF.PrepareDrawString($"{Math.Round(term.Credits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);


                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(term.GPACredits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                    pageSetup.TF.PrepareDrawString($"{Math.Round(term.GPACredits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(term.GradePoints, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                if (!pageSetup.TF._preparedText)
                {
                    pageSetup.TF.Alignment = XParagraphAlignment.Right;
                    rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                    pageSetup.TF.PrepareDrawString($"{Math.Round(term.GradePoints, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
                }
                pageSetup.TF.DrawString(XBrushes.Black);

                pageSetup.TotalHeight += neededHeight + 10;

                pageSetup.Graph.DrawLine(lineBlack2, 0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, pageSetup.TotalHeight);

                pageSetup.TotalHeight += 10;
            }

            pageSetup.TotalHeight += 50;

            pageSetup.TF.Alignment = XParagraphAlignment.Center;
            rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, 100);
            pageSetup.TF.PrepareDrawString($"Educational Career Totals", headerFont, rect, out lastCharIndex, out neededHeight);
            pageSetup.TotalHeight += neededHeight + 3;
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Center;
                rect = new XRect(0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, 100);
                pageSetup.TF.PrepareDrawString($"Educational Career Totals", headerFont, rect, out lastCharIndex, out neededHeight);
                pageSetup.TotalHeight += neededHeight + 3;
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
            pageSetup.TF.PrepareDrawString($"Att Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                pageSetup.TF.PrepareDrawString($"Att Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
            pageSetup.TF.PrepareDrawString($"E Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                pageSetup.TF.PrepareDrawString($"E Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
            pageSetup.TF.PrepareDrawString($"GPA Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                pageSetup.TF.PrepareDrawString($"GPA Hrs", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
            pageSetup.TF.PrepareDrawString($"GPA Pts", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                pageSetup.TF.PrepareDrawString($"GPA Pts", new XFont("Minion Pro", 10, XFontStyle.Underline), rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TotalHeight += neededHeight;

            pageSetup.TF.Alignment = XParagraphAlignment.Left;
            rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
            pageSetup.TF.PrepareDrawString($"Cumulative GPA", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(courseNameX, pageSetup.TotalHeight, courseTitleX - courseNameX, 100);
                pageSetup.TF.PrepareDrawString($"Cumulative GPA", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Left;
            rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
            pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.CalculatedGPA, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Left;
                rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.CalculatedGPA, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
            pageSetup.TF.PrepareDrawString($"Cumulative Total", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseTitleX, pageSetup.TotalHeight, attCredX - courseTitleX, 100);
                pageSetup.TF.PrepareDrawString($"Cumulative Total", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
            pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.PossibleCredits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(attCredX, pageSetup.TotalHeight, earnCredX - attCredX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.PossibleCredits, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
            pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.TotalCreditsCompleted, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(earnCredX, pageSetup.TotalHeight, courseGradeX - earnCredX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.TotalCreditsCompleted, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);


            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
            pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.TotalCreditsCompleted, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGradeX, pageSetup.TotalHeight, courseGPAPtsX - courseGradeX, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.TotalCreditsCompleted, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TF.Alignment = XParagraphAlignment.Right;
            rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
            pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.TotalCreditsCompleted, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            if (!pageSetup.TF._preparedText)
            {
                pageSetup.TF.Alignment = XParagraphAlignment.Right;
                rect = new XRect(courseGPAPtsX, pageSetup.TotalHeight, 75, 100);
                pageSetup.TF.PrepareDrawString($"{Math.Round(academicData.TotalCreditsCompleted, 2).ToString("0.00")}", font, rect, out lastCharIndex, out neededHeight);
            }
            pageSetup.TF.DrawString(XBrushes.Black);

            pageSetup.TotalHeight += 50;

            pageSetup.Graph.DrawLine(lineBlack2, 0, pageSetup.TotalHeight, pageSetup.PDFPage.Width.Point, pageSetup.TotalHeight);

            pageSetup.TotalHeight += 10;
            MemoryStream ms = new MemoryStream();
            string path = Server.MapPath("/Output/");
            string fileName = "UnofficialTranscript.pdf";
            pageSetup.Pdf.Save(path + fileName);
            pageSetup.Pdf.Close();

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
        private Student GetStudent(string tartanID)
        {
            var response = utility.CreateRequest(Method.GET, "https://api.sinclair.edu/portlet/api/", $"studentinfo/{tartanID}");
            if (response.IsSuccessful)
            {
                dynamic jsonContent = JValue.Parse(response.Content);
                Student student = jsonContent.ToObject<Student>();
                return student;
            }
            else
            {
                return null;
            }
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