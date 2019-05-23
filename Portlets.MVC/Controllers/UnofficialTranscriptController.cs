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
        public ActionResult Index(string Id, string searchString)
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
    }

}