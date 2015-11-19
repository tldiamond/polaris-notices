using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoticeSuite.Services.Mandrill
{
    public class Mandrill
    {
        static RestClient client = new RestClient("https://mandrillapp.com/api/1.0/");

        public static bool CheckRejection(string address)
        {
            var request = new RestRequest("rejects/list.json", Method.POST);
            var json = new
            {
                key = AppSettings.MandrillApiKey,
                email = address
            };
            request.AddParameter("text/json", JsonConvert.SerializeObject(json), ParameterType.RequestBody);
            

            var result = client.Execute<List<RejectionListEntry>>(request).Data;
            return result != null && result.Count > 0;
        }

        public static bool RemoveRejection(string address)
        {
            var request = new RestRequest("rejects/delete.json", Method.POST);
            var json = new
            {
                key = AppSettings.MandrillApiKey,
                email = address
            };
            request.AddParameter("text/json", JsonConvert.SerializeObject(json), ParameterType.RequestBody);

            var result = client.Execute(request);
            var failure = JsonConvert.DeserializeObject<RejectionRemovalFailure>(result.Content);
            var success = JsonConvert.DeserializeObject<RejectionRemovalSuccess>(result.Content);

            return success.deleted;
        }
    }
}