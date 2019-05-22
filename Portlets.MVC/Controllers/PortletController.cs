using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portlets.MVC.Models;
using Portlets.Service.Admin;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
    }

}