using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Portlets.Service.Admin
{
    public class Utility
    {
        /// <summary>
        /// Does all the hard work of creating and sending an http request
        /// </summary>
        /// <param name="method"></param>
        /// <param name="baseUrl"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public IRestResponse CreateRequest(Method method, string baseUrl, string endpoint)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest(endpoint, method);
            return client.Execute(request);
        }
        /// <summary>
        /// Does all the hard work of creating and sending an http request
        /// </summary>
        /// <param name="method"></param>
        /// <param name="baseUrl"></param>
        /// <param name="endpoint"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public IRestResponse CreateRequest(Method method, string baseUrl, string endpoint, object body)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest(endpoint, method);
            request.AddJsonBody(body);
            return client.Execute(request);
        }
        /// <summary>
        /// Does all the hard work of creating and sending an http request
        /// </summary>
        /// <param name="method"></param>
        /// <param name="baseUrl"></param>
        /// <param name="endpoint"></param>
        /// <param name="requestHeaders"></param>
        /// <returns></returns>
        public IRestResponse CreateRequest(Method method, string baseUrl, string endpoint, RequestHeader[] requestHeaders)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest(endpoint, method);

            foreach (var header in requestHeaders)
            {
                request.AddHeader(header.Key, header.Value);
            }

            return client.Execute(request);
        }
        /// <summary>
        /// Does all the hard work of creating and sending an http request
        /// </summary>
        /// <param name="method"></param>
        /// <param name="baseUrl"></param>
        /// <param name="endpoint"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public IRestResponse CreateRequest(Method method, string baseUrl, string endpoint, RequestHeader[] requestHeaders, object body)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest(endpoint, method);

            foreach (var header in requestHeaders)
            {
                request.AddHeader(header.Key, header.Value);
            }

            request.AddJsonBody(body);

            return client.Execute(request);
        }
    }

    public class RequestHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
