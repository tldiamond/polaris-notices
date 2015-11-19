using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twilio.TwiML;
using System.Configuration;
using Clc.Polaris.Api;
using System.Resources;
using NoticeSuite.Data;

namespace NoticeSuite.Extensions
{
	public static class NotificationExtensions
	{
		public static NotificationUpdateResult NotificationUpdate(this PolarisNotification notice, int callStatus, string details)
		{
			var updateParams = new NotificationUpdateParams()
			{
				NotificationTypeId = notice.NotificationTypeID,
				DeliveryOptionId = notice.DeliveryOptionID,
				DeliveryString = notice.DeliveryString,
				PatronId = notice.PatronID,
				ItemRecordId = notice.ItemRecordID,
				CallStatus = callStatus,
				Details = details
			};

			var client = new PolarisApiClient();
			var result = client.NotificationUpdate(updateParams);

			return result;
		}		
	}
}
