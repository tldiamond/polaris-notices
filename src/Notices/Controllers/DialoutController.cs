using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NoticeSuite.Models;
using NoticeSuite;
using Twilio;
using NoticeSuite.Extensions;
using Clc.Polaris.Api;
using NoticeSuite.Data;
using Twilio.TwiML;
using RestSharp;
using System.Resources;

namespace NoticeSuite.Controllers
{
    public class DialoutController : Controller
    {
		NoticeEntities db;
		private static MyLogger logger = MyLogger.GetLogger();
        Notifier notifier = new Notifier("Dialout");
        const int RECORDINGS_PER_PAGE = 1000;

		public DialoutController()
		{
            db = NoticeEntities.Create();
		}

		[HttpGet]		
		public string PlaceCalls(string key, bool ignoreTime = false, bool startedByTask = false, bool forceCleanup = false)
		{            
			// Check to make sure this is an authorized request to start placing calls.
			if (key != AppSettings.AuthString)
			{
				//EventWriter.WriteError("Incorrect key supplied");
				//logger.Debug(string.Format(Internal.IncorrectKey), key);
				return string.Format(Internal.IncorrectKey, key);
			}
			
			// Check to see if we are already making calls to avoid making duplicates and generally screwing things up.
			if (Variables.InProcess)
			{
				// If this run was initiate by the task scheduler job we want to cancel all in process calls and force the process
				// to start once again.
				if (startedByTask)
				{
					//logger.Info(string.Format(Internal.CancellingCalls, Variables.CallsInProcess), action:Actions.None);
					Methods.CancelCallsInProcess();
				}
				// If this run was not started by the task scheduler job we don't want to force the calls. This provides some
				// extra protection if we for some reason have to start it manually as it won't cancel all the calls and hang
				// up on patrons or give them a generic awful message.
				else
				{
					//EventWriter.WriteError("Calls already in process");
					logger.Debug(Internal.AlreadyRunning);
					return Internal.AlreadyRunning;
				}
			}
			
			// Check that we should make calls at the current hour.
			if (!IsValidCallTime() && !ignoreTime)
			{
				logger.Debug(Internal.OutsideRunHours);
				return Internal.OutsideRunHours;
			}

			// If most libraries are closed today don't make calls.
			if (!IsValidCallDate() && !ignoreTime)
			{
				logger.Debug(Internal.ClosedToday);
				return Internal.ClosedToday;
			}

			if (!CheckPolarisApi())
			{
				logger.Error(Internal.ApiDown);
				notifier.Notify(Internal.ApiDown, NotificationMethod.All);
			    return Internal.ApiDown;
            }

            //var testResult = DoTests();
            //if (!String.IsNullOrWhiteSpace(testResult))
            //{
            //    notifier.Notify("Error in dialout application", NotificationMethod.All);
            //    return testResult;
            //}


			// If all of the required conditions are met then finally begin to make phone calls.
			Methods.SetVariablesForNewRun();
            Methods.PopulateStrings();
			LoadNotifications();
			RemovePreviouslyCalledPatrons();			
			logger.Info(string.Format(Internal.CalledPatronCountPre, Variables.Notifications.GroupBy(n=>n.PatronID).Count()));
			CallPatrons();

			return string.Format(Internal.CalledPatronCountPost, Variables.Notifications.GroupBy(n=>n.PatronID).Count());
		}

        protected void CallPatrons()
        {
            var twilio = Methods.CreateTwilioClient();

            foreach (var noticesForPatron in Variables.Notifications.GroupBy(n => n.PatronID))
            {
                var firstNotice = noticesForPatron.First();

                // If we're testing call Mike's google voice number.
                if (AppSettings.Debug) firstNotice.DeliveryString = AppSettings.DebugPhoneNumber;

                var call = twilio.InitiateOutboundCall
                (
                    new CallOptions
                    {
                        From = AppSettings.FromPhone,
                        To = firstNotice.DeliveryString,
                        IfMachine = "Continue",
                        FallbackUrl = AppSettings.TwilioFallbackURL,
                        StatusCallback = AppSettings.TwilioStatusURL,
                        Url = AppSettings.TwilioCallURL,
                        Record = AppSettings.RecordCalls
                    }
                );
                if (!string.IsNullOrWhiteSpace(call.Sid))
                {
                    Variables.CallsPlaced++;
                    Variables.Calls.Add(new TwilioCall(firstNotice.PatronID, call));
                    logger.Debug("Call placed", call.Sid, firstNotice.PatronID);
                }
                else
                {
                    logger.Info("Call unable to be placed - " + call.RestException.Message, null, firstNotice.PatronID, null, null, null);

                    foreach (var notice in noticesForPatron)
                    {
                        notice.NotificationUpdate(7, "Unable to call patron - " + call.RestException.Message);
                    }
                }
            }
            //EventWriter.LogInfo(string.Format("Made {0} calls.", Variables.Calls.Count));
        }

        public string TwilioResponse(string callsid, string answeredby, bool testing = false)
        {
            var fallback = new TwilioResponse().Say(Internal.FallbackPhoneMessage, new { voice = "alice" });

            if (string.IsNullOrWhiteSpace(callsid))
            {
                logger.Error(Internal.EmptyCallSid, callsid);
                return fallback.ToString();
            }

            logger.Debug(Internal.TwimlBuildStart, callsid);

            try
            {
                int patronid = Variables.Calls.Single(c => c.CallSid == callsid).PatronID;
                var notices = Variables.Notifications.GroupBy(n => n.PatronID).Where(n => n.First().PatronID == patronid).First().ToList();

                var twiml = notices.PhoneMessageTwiML(answeredby);
                logger.Debug(twiml, callsid: callsid, patronid: patronid);
                Variables.TwilioResponsesSent++;

                if (testing)
                {
                    twiml = "Human Message: ";
                    twiml += notices.PhoneMessageTwiML("humman");
                    twiml += "\r\n";
                    twiml += "Machine Message: ";
                    twiml += notices.PhoneMessageTwiML("machine");
                }

                return twiml;
            }
            catch (Exception ex)
            {
                logger.ErrorException(Internal.TwimlFail, callsid: callsid, Exception: ex);
                return fallback.ToString();
            }
        }

        public string FallbackResponse()
        {
            return new TwilioResponse().Say(Internal.FallbackPhoneMessage, new { voice = "alice" }).ToString();
        }        

        public ActionResult TwilioStatus(string callSid, string callStatus, bool testing = false)
        {
            if (string.IsNullOrWhiteSpace(callSid)) return Content("no callsid");
            if (Variables.TransferredCalls.Contains(callSid)) return Content("ok");

            var patron = Variables.Calls.SingleOrDefault(c => c.CallSid == callSid);
            if (patron == null)
            {
                return new HttpStatusCodeResult(500, "Call variable not found for callsid");
            }

            if (!Variables.Notifications.Any(n => n.PatronID == patron.PatronID))
            {
                return new HttpStatusCodeResult(500, "Notices not found for patron");
            }

            var notices = Variables.Notifications.Where(n => n.PatronID == patron.PatronID);


            logger.Debug(string.Format(Internal.NoticesProcessedCount, notices.Count()), callSid, patron.PatronID, callStatus);
            int callStatusID;
            switch (callStatus)
            {
                case "completed":
                    callStatusID = 1;
                    break;
                case "busy":
                    callStatusID = 4;
                    break;
                case "failed":
                    callStatusID = 7;
                    break;
                case "no-answer":
                    callStatusID = 5;
                    break;
                default:
                    goto case "no-answer";
            }

            // If we're testing don't update the notice as complete so it still gets delivered to the patron.
            if (AppSettings.Debug || testing) callStatusID = 5;

            var errorMessage = "";

            foreach (var notice in notices)
            {
                var updateParams = new NotificationUpdateParams()
                {
                    NotificationTypeId = notice.NotificationTypeID,
                    DeliveryOptionId = notice.DeliveryOptionID,
                    DeliveryString = notice.DeliveryString,
                    PatronId = notice.PatronID,
                    ItemRecordId = notice.ItemRecordID,
                    CallStatus = callStatusID,
                    Details = AppSettings.Debug ? Internal.TestCallDetailsMessage : callSid
                };

                var client = new PolarisApiClient();
                //var result = client.NotificationUpdate(updateParams);
                var result = notice.NotificationUpdate(callStatusID, AppSettings.Debug ? Internal.TestCallDetailsMessage : callSid);

                if (result.ErrorMessage != null)
                {
                    logger.Info(result.ErrorMessage, callSid, patron.PatronID, callStatus, notice.ItemRecordID, notice.NotificationTypeID);
                    errorMessage += result.ErrorMessage;
                }
                else
                {
                    logger.Info(Internal.NotificationUpdateSuccess, callSid, patron.PatronID, callStatus, notice.ItemRecordID, notice.NotificationTypeID);
                }
            }

            return Content(errorMessage);
        }

		[Authorize]
		public string TestTwiml()
		{
            Methods.PopulateStrings();
            LoadNotifications();
			var msg = "";
            
			foreach (var pn in Variables.Notifications.GroupBy(n => n.PatronID))
			{
                msg += pn.First().DeliveryString;
                msg += "<br/>-------------<br/>";
				msg += pn.ToList().PhoneMessageTwiML("human");
				msg += "<br/>";
				msg += "<br/>";
			}
			return msg;
		}

		bool CheckPolarisApi()
		{
			var papi = new PolarisApiClient();
            var x = papi.OrganizationsGet(OrganizationType.System);

            try
            {
                return x.OrganizationsGetRows.Count == 1;
            }
            catch
            {
                return false;
            }
		}        
				
		protected void LoadNotifications()
		{
            Variables.Notifications = db.PolarisNotifications.ToList();
            if (AppSettings.LibrariesToCall.Count > 1 || AppSettings.LibrariesToCall.First() != 0)
            {
                Variables.Notifications = Variables.Notifications.Where(n => AppSettings.LibrariesToCall.Contains(n.PatronLibrary ?? 0)).ToList();
            }

            if (Variables.Notifications.Count() == 0)
			{
				logger.Info(Internal.NoNotifications); 
				return;
			}

			// If we're testing only grab notices for one patron.
            if (AppSettings.Debug) Variables.Notifications = Variables.Notifications.GroupBy(n => n.PatronID).First().ToList();
			//Variables.Notifications = notices.ToList();
		}

		protected void RemovePreviouslyCalledPatrons()
		{
            var calledPatrons = db.TodaysHoldCalls.ToList();

            if (AppSettings.LibrariesToCall.Count > 1 || AppSettings.LibrariesToCall.First() != 0)
            {
                calledPatrons = calledPatrons.Where(n => AppSettings.LibrariesToCall.Contains(n.ReportingLibraryID)).ToList();
            }

			if (calledPatrons.Count() == 0)
			{
				logger.Info(Internal.NoHoldCallsYet);
				return;
			}            

			foreach (var calledPatronNotices in Variables.Notifications
				.Where(n => calledPatrons.Any(cp => cp.PatronID == n.PatronID && cp.ReportingOrgId == n.ReportingBranchID))
				.GroupBy(n => n.PatronID))
			{
				foreach (var notice in calledPatronNotices)
				{
					var result = notice.NotificationUpdate(1, Internal.PatronAlreadyCalledHold);

                    if (result.PAPIErrorCode != 0)
                        logger.Error(result.ErrorMessage, patronid: notice.PatronID, itemrecordid: notice.ItemRecordID);
                    else
                        logger.Info(Internal.PatronAlreadyCalledHold, null, notice.PatronID, null, notice.ItemRecordID, notice.NotificationTypeID);
				}

				Variables.Notifications.RemoveAll(n => n.PatronID == calledPatronNotices.First().PatronID);
			}
		}

        bool CheckUrl(string url)
        {
            var client = new RestClient(url);
            var result = client.Execute(new RestRequest());

            if (result.StatusCode == System.Net.HttpStatusCode.OK) return true;
            return false;
        }

        public ActionResult RunTests()
        {
            return Content(DoTests());
        }

        string DoTests()
        {
            string errorMessage = "";
            // Don't test if process is currently running
            if (Variables.InProcess) errorMessage += "currently in process\r\n";

            // Check the call urls
            if (!CheckUrl(AppSettings.TwilioCallURL)) errorMessage += "response url unreachable\r\n";
            if (!CheckUrl(AppSettings.TwilioStatusURL)) errorMessage += "status url unreachable\r\n";

            // If we've already found an error then return before destroying the call variables
            if (!String.IsNullOrWhiteSpace(errorMessage)) return errorMessage;

            // Set things up just like a regular run
            Methods.SetVariablesForNewRun();
            Methods.PopulateStrings();
            LoadNotifications();

            if (Variables.Notifications.Count == 0) return errorMessage += "no notifications loaded";

            var call = new Call { Sid = "testing" };

            var noticesForPatron = Variables.Notifications.GroupBy(n => n.PatronID).First();
            var firstNotice = noticesForPatron.First();
            Variables.Calls.Add(new TwilioCall(firstNotice.PatronID, call));

            var response = TwilioResponse(call.Sid, "human", true);
            // Hacky way to check to make sure the messages were created properly
            if (response.Count(c => c == '>') < 20)
            {
                errorMessage += "invalid twiml";
            }

            var status = TwilioStatus(call.Sid, "no-answer", true);
            if (status.GetType() == typeof(ContentResult))
            {
                var statusText = ((ContentResult)status).Content;
                // Any return value from the status callback indicates a failure
                if (!String.IsNullOrWhiteSpace(statusText)) errorMessage += "error during status callback";
            }
            // If the return type isn't a ContentResult then something went wrong
            else
            {
                errorMessage += "error during status callback";
            }

            return errorMessage;
        }

		protected bool IsValidCallDate()
		{
            return !db.GetClosedDates(AppSettings.ClosedDateBranch).Select(dc => dc.DateClosed).Contains(DateTime.Today);
		}        

		protected bool IsValidCallTime()
		{
			int startHour = 0, stopHour = 0;
			switch (DateTime.Now.DayOfWeek.ToString().ToLower())
			{
				case "monday":
					goto case "weekday";
				case "tuesday":
					goto case "weekday";
				case "wednesday":
					goto case "weekday";
				case "thursday":
					goto case "weekday";
				case "friday":
					goto case "weekday";
				case "saturday":
					goto case "weekend";
				case "sunday":
					goto case "weekend";

				case "weekday":
					startHour = AppSettings.WeekdayStartHour;
					stopHour = AppSettings.WeekdayStopHour;
					break;
				case "weekend":
					startHour = AppSettings.WeekendStartHour;
					stopHour = AppSettings.WeekendStopHour;
					break;
				default:
					logger.Error(String.Format(Internal.DayOfWeekFail, DateTime.Now.DayOfWeek.ToString().ToLower()));
					goto case "weekend";
			}

			var currentHour = int.Parse(DateTime.Now.ToString("HH"));
			if (currentHour >= startHour && currentHour <= stopHour)
			{
				return true;				
			}

			return false;	
		}
    }
}
