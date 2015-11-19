using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoticeSuite.Models;
using Twilio.TwiML;
using System.Configuration;
using NoticeSuite.Data;

namespace NoticeSuite.Extensions
{
	public static class NotificationListExtensions
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static string PhoneMessageTwiML(this List<PolarisNotification> notices, string answeredBy)
		{
			var response = new TwilioResponse();

			if (notices.Select(n => n.PatronID).Distinct().Count() > 1)
			{
				string ids = "";
				notices.Select(n => n.PatronID).Distinct().ToList().ForEach(n => ids += string.Format("{0} ", n));
				var e = new Exception(string.Format("This method can only be used on a list of notices for a single patron. Currently the list contains patrons {0}", ids));
				logger.ErrorException(e.Message, e);
				throw e;
			}

			var messageBody = new StringBuilder();
			var firstNotice = notices.First();

            messageBody.Append(firstNotice.GetString(MessageType.Intro));

			int firstBranch = 0;
			int lastBranch = 0;
			if (notices.HasHolds())
			{
				firstBranch = notices.Where(n => n.IsHold).Select(n => n.ReportingBranchID).Distinct().First();
				lastBranch = notices.Where(n => n.IsHold).Select(n => n.ReportingBranchID).Distinct().Last();
			}
			// Group holds by pickup branch
			foreach (var branchHolds in notices.Where(n => n.IsHold).GroupBy(t => t.ReportingBranchID))
			{
				var hold = branchHolds.First();
				
				if (hold.ReportingBranchID == firstBranch)
				{
                    messageBody.AppendFormat(hold.GetString(MessageType.HoldMessageFirst).ToString(),
                            branchHolds.Count(),
                            branchHolds.Count() == 1 ? "item" : "items",
                            hold.GetString(MessageType.Location, false),
                            branchHolds.Min(t => t.HoldTillDate).Value.ToString(hold.GetString(MessageType.HoldDateFormats).ToString()));

					if (hold.ReportingBranchID == lastBranch)
					{
						messageBody.Append(notices.HasOverdues() ? " and, " : ". ");
					}
				}
				else
				{
					if (hold.ReportingBranchID == lastBranch)
					{
						messageBody.Append(notices.HasOverdues() ? ", " : " and, ");
					}
					else
					{
						messageBody.Append(", ");
					}

					messageBody.AppendFormat(hold.GetString(MessageType.HoldMessageAdditional).ToString(),
							branchHolds.Count(),
							branchHolds.Count() == 1 ? "item" : "items",
							hold.GetString(MessageType.Location, false),
							branchHolds.Min(t => t.HoldTillDate).Value.ToString(hold.GetString(MessageType.HoldDateFormats).ToString()));

					if (hold.ReportingBranchID == lastBranch)
					{
						messageBody.Append(notices.HasOverdues() ? " and, " : ".");
					}
				}
			}

			// Patron has overdues
			if (notices.HasOverdues())
			{	// Get the overdue message for the library				
				messageBody.Append(notices.First().GetString(MessageType.Overdue));
			}
			
			//response.Say(messageBody.ToString());

			var voiceAttribute = new { voice = "alice" };

			string patronName = string.Format("{0} {1}.", firstNotice.NameFirst, firstNotice.NameLast).ToLower();

            var greeting = notices.First().GetString(MessageType.Greeting);
            var renewal = notices.First().GetString(MessageType.Renewal);
            var goodbye = notices.First().GetString(MessageType.Goodbye);
            var repeat = notices.First().GetString(MessageType.Repeat);
			// Put the pieces together to build the phone message. If a human answered the phone we will present the user
			// the option to transfer to the renewals system. We will also repeat the message, incase Twilio failed to detect
			// an answering machine.
			switch (answeredBy.ToLower())
			{
				case "human":
					response.AddMessagePart(greeting, patronName);
					response.Say(messageBody.ToString(), voiceAttribute);

					if (notices.HasOverdues())
					{
                        response.BeginGather(new { numDigits = 1, action = string.Format("{0}?barcode2={1}", AppSettings.DialinURL, notices.First().PatronBarcode), method = "GET" });
						response.AddMessagePart(renewal);
						response.EndGather();
					}

					response.AddMessagePart(goodbye);
					response.AddMessagePart(repeat);
                    break;

				case "machine":
					response.AddMessagePart(greeting, patronName);
					response.Say(messageBody.ToString(), voiceAttribute);
					response.AddMessagePart(goodbye);
                    response.AddMessagePart(repeat);                    
					break;

				default:
					goto case "human";
			}

            response.AddMessagePart(greeting, patronName);
            response.Say(messageBody.ToString(), voiceAttribute);
            response.AddMessagePart(goodbye);
            response.Hangup();

			return response.ToString();
		}

		public static bool HasOverdues(this List<PolarisNotification> notices)
		{
			return notices.Any(n => n.IsOverdue);
		}

		public static bool HasHolds(this List<PolarisNotification> notices)
		{
			return notices.Any(n => n.IsHold);
		}

        public static bool Has2ndHolds(this List<PolarisNotification> notices)
        {
            return notices.Any(n => n.Is2ndHold);
        }
	}
}
