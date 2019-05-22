using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Portlets.Service.Admin
{
    public class Admin
    {

        /// <summary>
        /// Provides admin bearer token for authentication
        /// </summary>
        /// <returns>The bearer token in the form of a string</returns>
        public string Login()
        {
            byte[] dataU = Convert.FromBase64String(ConfigurationManager.AppSettings["ColleagueUsername"]);
            byte[] dataP = Convert.FromBase64String(ConfigurationManager.AppSettings["ColleaguePassword"]);
            var loginTemp = new ColleagueLogin()
            {
                UserId = Encoding.UTF8.GetString(dataU),
                Password = Encoding.UTF8.GetString(dataP)
            };

            var client = new RestClient("https://api.sinclair.edu/colleagueapi");
            var request = new RestRequest("session/login/", Method.POST);
            request.AddHeader("Accept", "application/vnd.ellucian.v1+json");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(loginTemp);
            var response = client.Execute(request);

            return response.Content;
        }

        private class ColleagueLogin
        {
            public string UserId { get; set; }
            public string Password { get; set; }
        }

    }


}
