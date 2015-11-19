using System;
using System.Collections.Generic;
using System.Configuration;
using Twilio;
using NoticeSuite.Models;
using NoticeSuite.Data;

namespace NoticeSuite
{
	public static class Variables
	{
		public static List<PolarisNotification> Notifications = new List<PolarisNotification>();
		public static int CallsPlaced = 0;
		public static bool InProcess
		{
			get
			{
				return CallsInProcess > 0;
			}
		}
		public static int TwilioResponsesSent = 0;
		public static int PreviousCalls = CallsPlaced;
		public static int PreviousResponses = TwilioResponsesSent;
		public static int PatronsAlreadyCalled = 0;
		public static DateTime LastRunTime = new DateTime();
		public static List<TwilioCall> Calls = new List<TwilioCall>();
        public static List<string> TransferredCalls = new List<string>();

        public static IEnumerable<Dialout_Strings> Strings;

		public static int CallsInProcess
		{
			get
			{
				var twilio = new TwilioRestClient(ConfigurationManager.AppSettings["twilio_account_number"], ConfigurationManager.AppSettings["twilio_auth_token"]);
				var test2 = ConfigurationManager.AppSettings["twilio_auth_token"];
                var request = new CallListRequest { From=AppSettings.FromPhone, StartTime = DateTime.Now.Date, StartTimeComparison = ComparisonType.GreaterThanOrEqualTo };

				request.Status = "queued";
				var test = twilio.ListCalls(request);
				var queued = twilio.ListCalls(request).Calls.Count;

				request.Status = "ringing";
				var ringing = twilio.ListCalls(request).Calls.Count;

				request.Status = "in-progress";
				var in_process = twilio.ListCalls(request).Calls.Count;

				return queued + ringing + in_process;				
			}
		}

		public static DateTime RecordingCutOffDate
		{
			get
			{
				int timeToKeep;
                int.TryParse(ConfigurationManager.AppSettings["recording_retention_days"], out timeToKeep);
				timeToKeep = timeToKeep > 0 ? timeToKeep : 30;
				return DateTime.Now.Subtract(new TimeSpan(timeToKeep, 0, 0, 0));
			}
		}
	}
}
