using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio;

namespace NoticeSuite
{
    public class Janitor
    {
        static List<Recording> GetOldRecordings()
        {
            var twilio = Methods.CreateTwilioClient();
            var recordings = new List<Recording>();

            var request = new RestRequest("Accounts/{AccountSid}/Recordings.json?DateCreated<={DateCreated}&PageSize=1000")
                .AddUrlSegment("DateCreated", Variables.RecordingCutOffDate.ToString("yyyy-MM-dd"));

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

        public static void CleanupRecordings()
        {
            if (AppSettings.EnableDialout) return;

            var twilio = Methods.CreateTwilioClient();
            GetOldRecordings().ForEach(r => twilio.DeleteRecording(r.Sid));
        }
    }
}