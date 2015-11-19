using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NoticeSuite.Models;
using Twilio;
using NoticeSuite.Data;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;

namespace NoticeSuite
{
	public static class Methods
	{
		public static void SetVariablesForNewRun()
		{
			Variables.PreviousCalls = Variables.CallsPlaced;
			Variables.PreviousResponses = Variables.TwilioResponsesSent;
			Variables.LastRunTime = DateTime.Now;			
			Variables.CallsPlaced = 0;
			Variables.TwilioResponsesSent = 0;
			Variables.PatronsAlreadyCalled = 0;

            Variables.Calls = new List<TwilioCall>();
            Variables.Notifications = new List<PolarisNotification>();
            Variables.TransferredCalls = new List<string>();
		}

        public static void PopulateStrings()
        {
            using (var db = NoticeEntities.Create())
            {
                Variables.Strings = db.Dialout_Strings.ToArray();
            }
        }

        public static string GetConnectionString(string server, string database, string metadata)
        {
            // Specify the provider name, server and database.
            string providerName = "System.Data.SqlClient";

            // Initialize the connection string builder for the
            // underlying provider.
            SqlConnectionStringBuilder sqlBuilder =
                new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = server;
            sqlBuilder.InitialCatalog = database;
            sqlBuilder.IntegratedSecurity = true;
            sqlBuilder.Encrypt = false;
            sqlBuilder.PersistSecurityInfo = true;
            sqlBuilder.MultipleActiveResultSets = true;

            // Build the SqlConnection connection string.
            string providerString = sqlBuilder.ToString();

            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = providerName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = providerString;

            // Set the Metadata location.
            entityBuilder.Metadata = string.Format("res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", metadata);
            

            return entityBuilder.ToString();
        }

        public static TwilioRestClient CreateTwilioClient()
        {
            return new TwilioRestClient(AppSettings.TwilioAccountNumber, AppSettings.TwilioAuthToken);
        }

		public static void CancelCallsInProcess(HangupStyle hangupStyle = HangupStyle.Canceled)
		{
			Console.WriteLine("");
			Console.WriteLine("Gathering in process calls from Twilio.");
			var callsInProcess = new List<Call>();
			var twilio = Methods.CreateTwilioClient();
			var request = new CallListRequest();

			request.Status = "queued";
			callsInProcess.AddRange(twilio.ListCalls(request).Calls);

			request.Status = "ringing";
			callsInProcess.AddRange(twilio.ListCalls(request).Calls);

			request.Status = "in-process";
			callsInProcess.AddRange(twilio.ListCalls(request).Calls);

			Console.WriteLine("Cancelling {0} calls.", callsInProcess.Count);

			callsInProcess.ForEach(call =>
			{
				Console.WriteLine("[ Sid:{0} ] - [ Previous Status:{1} ]", call.Sid, call.Status);
				var result = twilio.HangupCall(call.Sid, hangupStyle);
			});
			Console.WriteLine("Process completed");
		}
	}
}
