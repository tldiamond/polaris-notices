using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Net;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using Twilio;
using NoticeSuite.Extensions;
using NoticeSuite.Models;
using NoticeSuite.Data;
using NoticeSuite.Services.SMS;

namespace NoticeSuite.Controllers
{	
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    public class SmsController : Controller
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        
        private static readonly Random r = new Random();
        static TwilioRestClient twilio = Methods.CreateTwilioClient();

        public SmsController()
        {
        }

		[ValidateInput(false)]
        public ActionResult Process(FormCollection fc, string auth)
        {
            if (!AppSettings.EnableSmsProcessing) return Content("sms processing disabled in config");

            if (!fc.AllKeys.Contains("mandrill_events") || auth != AppSettings.AuthString)
            {
                return Content(" ");
            }
            
            var events = fc.GetEvents();	
            SMS.SendMessages(events);
            
			return Content("message processed");
        }

        public string Test(string auth)
        {
            if (!AppSettings.EnableSmsProcessing) return "sms processing disabled in config";
   
            if (auth != AppSettings.AuthString)
            {
                return "Invalid auth string";
            }

            SMS.SendTestMessage();
            
            return "test";
        }
	}
}