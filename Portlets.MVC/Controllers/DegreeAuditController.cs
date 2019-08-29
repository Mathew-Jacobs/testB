using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portlets.MVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portlets.MVC.Controllers
{
    public class DegreeAuditController : Controller
    {
        // GET: DegreeAudit
        public ActionResult Index()
        {
            var model = LoadDefaultJSON();
            List<DataPoint> dataPointsBlocks = new List<DataPoint>
            {
                new DataPoint("Blocks Met", Convert.ToDouble(model.SASTableData[0].blocks_met)),
                new DataPoint("Blocks Left", Convert.ToDouble(model.SASTableData[0].total_blocks - model.SASTableData[0].blocks_met))
            };
            List<DataPoint> dataPointsCourses = new List<DataPoint>
            {
                new DataPoint("Courses Complete", 30d),
                new DataPoint("Courses Left", 5d)
            };
            ViewBag.DataPointsBlocks = JsonConvert.SerializeObject(dataPointsBlocks);
            ViewBag.DataPointsCourses = JsonConvert.SerializeObject(dataPointsCourses);
            return View(model);
        }

        private DegreeAuditObject LoadDefaultJSON()
        {
            string json = "{\"SASJSONExport\": \"1.0\",\"SASTableData\": [{\"Not_used\": \"BIO-112,D,2.67,2.67,I,2.67,BIS-160,CR,2,0,T,2\",\"used\": \"MUS-1121,HIS-1101,LIT-2230,COM-206,ENG-111,ENG-112,ENG-113,SCC-101,PSY-2225,BIO-1211,BIO-111,PSY-2242,SOC-111,PSY-2200,PSY-122,THE-1101,SOC-112,PLS-2200,PSY-1160,PSY-2217,GEO-201,PSY-121,PSY-2220\",\"Academic_Program_Reqmt_Id_Nb\": \"LA.S.AA*2019\",\"Minimum_Credit_Ct\": 60,\"Institution_Min_Credit_Ct\": 15,\"Program_Minimum_GPA_Ct\": 2,\"Institution_Min_GPA_Ct\": 2,\"Student_Program_Id_Nb\": \"0745785*LA.S.AA\",\"running_total\": 60.34,\"I_running_total\": 53.67,\"CUMGPA\": 2.139,\"cred_for_credntl\": 3,\"crse_for_credntl\": 0,\"PROGPA\": 2.54648779579,\"total_blocks\": 10,\"blocks_met\": 9}]}";
            dynamic jsonValue = JValue.Parse(json);
            DegreeAuditObject obj = jsonValue.ToObject<DegreeAuditObject>();
            return obj;
        }
    }
}