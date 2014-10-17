using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Notices.Controllers
{
    public class RejectionsController : Controller
    {
        [HttpGet]
        public ActionResult Remove()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Remove(string address)
        {
            var client = new RestClient("https://mandrillapp.com/api/1.0/");
            var request = new RestRequest("rejects/list.json", Method.POST);

            var json = new
            {
                key = AppSettings.MandrillApiKey,
                email = address
            };

            request.AddParameter("text/json", JsonConvert.SerializeObject(json), ParameterType.RequestBody);

            if (client.Execute(request).Content.Length < 10)
            {
                return View("RemovalResult", new RejectionResultViewModel { Success = false, Message = "Address does not exist in rejection list." });
            }

            request = new RestRequest("rejects/delete.json", Method.POST);            
            request.AddParameter("text/json", JsonConvert.SerializeObject(json), ParameterType.RequestBody);

            var result = client.Execute(request);
            if (result.Content.Contains("status") && result.Content.Contains("code"))
            {
                var data = JsonConvert.DeserializeObject<RejectionRemovalFailure>(result.Content);
                return View("RemovalResult", new RejectionResultViewModel { Success = false, Message = data.message });
            }

            if (result.Content.Contains("deleted") && result.Content.Contains("subaccount"))
            {
                var data = JsonConvert.DeserializeObject<RejectionRemovalSuccess>(result.Content);
                return View("RemovalResult", new RejectionResultViewModel { Success = true, Message = data.email + " successfully deleted" });
            }

            return View();
        }

    }
}
