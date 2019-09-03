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
            var blocks = new List<int?>();
            var obj = new List<DegreeAuditBlock>();
            foreach (var item in model)
            {
                if (!blocks.Contains(item.Parent_Block_Counter_Id))
                {
                    blocks.Add(item.Parent_Block_Counter_Id);
                    obj.Add(new DegreeAuditBlock
                    {
                        Block_ID = item.Parent_Block_Counter_Id,
                        Block_Label = item.Block_Label,
                        Block_Met = (item.met == 1),
                        Courses = new List<DegreeAuditCourse>()
                    });
                }
            }
            foreach (var block in obj)
            {
                foreach (var item in model.Where(x => x.Parent_Block_Counter_Id == block.Block_ID))
                {
                    block.Courses.Add(new DegreeAuditCourse
                    {
                        Course_Name = item.course_nm_s,
                        Course_Title = item.Course_Title_Nm,
                        Specification = item.Printed_Specification_Ds,
                        Verified_Grade = item.Verified_Grade_Cd
                    });
                }
            }
            return View(obj);
        }

        private List<DegreeAuditObject> LoadDefaultJSON()
        {
            using (StreamReader sr = new StreamReader(Server.MapPath("~/Content/DefaultJSON/DegreeAudit.json")))
            {
                dynamic jsonValue = JValue.Parse(sr.ReadToEnd());
                List<DegreeAuditObject> obj = jsonValue.ToObject<List<DegreeAuditObject>>();
                return obj;
            }
        }
    }
}