using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Twilio;
using RestSharp;
using NoticeSuite.Model;

namespace NoticeSuite.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        //
        // GET: /Recordings/
		TwilioRestClient twilio = Methods.CreateTwilioClient();
        const int RECORDINGS_PER_PAGE = 1000;

        public ActionResult Calls(string pn)
        {
			if (string.IsNullOrWhiteSpace(pn))
			{
                //var test2 = twilio.ListCalls(new CallListRequest { To = pn });//.Calls.Where(c => c.Direction == "outbound-api").ToList();
				return View(new callListModel { Calls = twilio.ListCalls().Calls.Where(c => c.Direction == "outbound-api").ToList() });
			}
			var test = twilio.ListCalls(new CallListRequest { To = pn }).Calls.Where(c => c.Direction == "outbound-api");
			
			return View(new callListModel { Calls = twilio.ListCalls(new CallListRequest { To = pn }).Calls.Where(c => c.Direction == "outbound-api").ToList(), PhoneNumber = pn });			
        }

		public ActionResult Listen(string callSid)
		{
			if (string.IsNullOrWhiteSpace(callSid))
				return Redirect("Index");

			var recording = twilio.ListRecordings(callSid, null, null, null).Recordings.FirstOrDefault();

			if (recording != null)
			{
				return Redirect(string.Format("{0}{1}", "https://api.twilio.com", recording.Uri.ToString().Replace(".json", "")));
			}
			else
			{
				return View("InvalidRecording");
			}
		}

        [AllowAnonymous]
        public ActionResult Cleanup()
        {
            var test = GetOldRecordings();
            var test2 = test;
            return Content("");
        }

        public List<Recording> GetOldRecordings()
        {
            var twilio = Methods.CreateTwilioClient();
            var recordings = new List<Recording>();

            var test = twilio.ListRecordings();
            var test2 = test;

            var request = new RestRequest("Accounts/{AccountSid}/Recordings.json?DateCreated<={DateCreated}&PageSize={PageSize}")
                .AddUrlSegment("DateCreated", Variables.RecordingCutOffDate.ToString("yyyy-MM-dd"))
                .AddUrlSegment("PageSize", RECORDINGS_PER_PAGE.ToString());

            var result = new RecordingResult();
            result = twilio.Execute<RecordingResult>(request);
            recordings.AddRange(result.Recordings);
            while (result.NextPageUri != null)
            {
                var nextPage = result.NextPageUri.ToString().Replace("/2010-04-01/", ""); ;
                result = twilio.Execute<RecordingResult>(new RestRequest(nextPage));
                recordings.AddRange(result.Recordings);
            }

            return recordings;
        }

        public ActionResult Sms(string pn)
        {
            twilio = new TwilioRestClient(AppSettings.SmsTwilioAccountNumber, AppSettings.SmsTwilioAuthToken);

            SmsListModel viewModel;

            if (string.IsNullOrWhiteSpace(pn))
            {
                viewModel = new SmsListModel(null, twilio.ListSmsMessages(null, null, null, null, 100).SMSMessages);
                return View(viewModel);
            }

            viewModel = new SmsListModel(pn, twilio.ListSmsMessages(pn, null, null, null, 100).SMSMessages);                        
            return View(viewModel);
        }
    }
}
