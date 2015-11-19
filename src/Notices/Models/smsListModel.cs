using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio;

namespace NoticeSuite.Model
{
	public class SmsListModel
	{
		public string PhoneNumber { get; set; }
		public List<SMSMessage> Messages { get; set; }

		public SmsListModel(string phoneNumber, List<SMSMessage> messages)
		{
			PhoneNumber = phoneNumber;
			Messages = messages;
		}
	}
}