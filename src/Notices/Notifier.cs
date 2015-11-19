using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NoticeSuite
{
	public class Notifier
	{
        string Name;

        public Notifier(string name)
        {
            Name = name;
        }

        public static Notifier Create(string name)
        {
            return new Notifier(name);
        }

		public void Notify(string message, NotificationMethod notificationMethod)
		{
            if (!AppSettings.EnableNotifier) return;

			switch (notificationMethod)
			{
				case NotificationMethod.All:
					SendSMS(message);
					SendEmail(message);
					break;
				case NotificationMethod.SMS:
					SendSMS(message);
					break;
				case NotificationMethod.Email:
					SendEmail(message);
					break;
				default:
					goto case NotificationMethod.Email;
			}
		}

		void SendSMS(string message)
        {
            var twilio = Methods.CreateTwilioClient();
            var numbers = twilio.ListIncomingPhoneNumbers().IncomingPhoneNumbers;
			
			AppSettings.NotifyPhoneNumbers.ForEach(
				p => twilio.SendMessage(numbers.First().PhoneNumber, p, message)
				);
		}

        void SendEmail(string text)
        {
            var json = new
            {
                key = AppSettings.MandrillApiKey,
                message = new
                {
                    text = text,
                    subject = "Application Notification",
                    from_email = "donotreply@clcohio.org",
                    from_name = Name,
                    to = AppSettings.NotifyEmailAddresses.Select(e => new { email = e, type = "to" }).ToArray()
                }
            };

            var client = new RestClient("https://mandrillapp.com/api/1.0/");
            var request = new RestRequest("messages/send.json", Method.POST);
            request.AddParameter("text/json", JsonConvert.SerializeObject(json), ParameterType.RequestBody);
            var result = client.Execute(request);
        }
	}

	public enum NotificationMethod
	{
		All,
		SMS,
		Email
	}
}
