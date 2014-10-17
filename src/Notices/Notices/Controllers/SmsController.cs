using Mandrill;
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

namespace Notices.Controllers
{	
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    public class SmsController : Controller
    {
        public static bool sendingQueue = false;
        static Logger logger = LogManager.GetCurrentClassLogger();
        
        private static readonly Random r = new Random();
		static TwilioRestClient twilio = new TwilioRestClient(AppSettings.TwilioAccountSid, AppSettings.TwilioAuthToken);

        public SmsController()
        {
         
        }

		[ValidateInput(false)]
        public ActionResult Process(FormCollection fc, string auth)
        {            
            if (!fc.AllKeys.Contains("mandrill_events") || auth != AppSettings.AuthString)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.OK);
            }
            
            var events = fc.GetEvents();
            
			if (!isValidRunTime)
			{
				SaveToQueue(events);
				return Content("message queued");
			}
			
            SendMessages(events);
			SendQueue();
            
			return Content("message sent");
        }

        public string Test(string auth)
        {            
            if (auth != AppSettings.AuthString)
            {
                return "Invalid auth string";
            }

            SaveToQueue(
                new List<MailEvent>() 
                { 
                    new MailEvent 
                    { 
                        Msg = new Mandrill.Message 
                        {
                            To = new string[][] { new string[] { AppSettings.SmsTestNumber, "" } },
                            Text = "woo it works" 
                        }
                    }
                }
            );

            SendQueue();
            
            return "test";
        }

		void SaveToQueue(IEnumerable<MailEvent> events)
		{
            using (var db = PolarisEntities.Create())
            {
                foreach (var ev in events)
                {
                    db.CLC_Custom_SMS_Queue.Add(new CLC_Custom_SMS_Queue
                    {
                        PhoneNumber = ev.Msg.CleanTo,
                        Message = ev.Msg.CleanBody,
                        InsertDate = DateTime.Now
                    });
                    logger.LogInfo("Message queued", ev.Msg.CleanTo, ev.Msg.CleanBody);
                }
                db.SaveChanges();
            }
		}

        void SendMessages(IEnumerable<MailEvent> events)
        {
            foreach (var ev in events)
            {                
                var result = twilio.SendMessage(
                    fromNumber,
                    ev.Msg.CleanTo,
                    ev.Msg.CleanBody
                    );
                logger.LogInfo("Message sent", ev.Msg.CleanTo, ev.Msg.CleanBody, result.Sid);

                if (string.IsNullOrWhiteSpace(result.Sid))
                {
                    var test = result;
                    logger.Custom(LogLevel.Error, "blank result sid", ev.Msg.CleanTo, ev.Msg.CleanBody, "none");
                }
            }            
        }

		void SendQueue()
		{
            if (!sendingQueue)
            {
                sendingQueue = true;
                using (var db = PolarisEntities.Create())
                {
                    foreach (var msg in db.CLC_Custom_SMS_Queue)
                    {                        
                        var result = twilio.SendMessage(fromNumber, msg.PhoneNumber, msg.Message);
                        logger.LogInfo("Message sent from queue", msg.PhoneNumber, msg.Message, result.Sid);
                        db.CLC_Custom_SMS_Queue.Remove(msg);

                        if (string.IsNullOrWhiteSpace(result.Sid))
                        {
                            var test = result;
                            logger.Custom(LogLevel.Error, "blank result sid", msg.PhoneNumber, msg.Message, "none");
                        }
                    }

                    db.SaveChanges();
                }
                sendingQueue = false;
            }
		}

		public string fromNumber
		{
			get
			{
				var numbers = twilio.ListIncomingPhoneNumbers().IncomingPhoneNumbers;
				return numbers[r.Next(0, numbers.Count())].PhoneNumber;
			}
		}

		public bool isValidRunTime
		{
			get
			{
                if (AppSettings.StartHour == 0 || AppSettings.StopHour == 0) { return false; }
				return DateTime.Now.Hour >= AppSettings.StartHour && DateTime.Now.Hour < AppSettings.StopHour;
			}
		}
	}
}




