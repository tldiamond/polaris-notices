using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Notices
{
    public static class Extensions
    {
        public static IEnumerable<Mandrill.MailEvent> GetEvents(this FormCollection fc)
        {
            string json = fc["mandrill_events"];
            return JsonConvert.DeserializeObject<IEnumerable<Mandrill.MailEvent>>(json);
        }

        public static void LogInfo(this Logger logger, string message, string phoneNumber ="", string smsText = "", string sid = "")
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, logger.Name, message);

            logEvent.Properties["PhoneNumber"] = phoneNumber;
            logEvent.Properties["SMS"] = smsText;
            logEvent.Properties["Sid"] = sid;

            var x = logger.Name;
            logger.Log(typeof(Extensions), logEvent);
        }

        public static void Custom(this Logger logger, LogLevel level, string message, string phoneNumber = "", string smsText = "", string sid = "")
        {
            LogEventInfo logEvent = new LogEventInfo(level, logger.Name, message);

            logEvent.Properties["PhoneNumber"] = phoneNumber;
            logEvent.Properties["SMS"] = smsText;
            logEvent.Properties["Sid"] = sid;

            var x = logger.Name;
            logger.Log(typeof(Extensions), logEvent);
        }

        public static RecordSet BounceRecordSet(this Patron p)
        {
            using (var db = PolarisEntities.Create())
            {
                var rsid = int.Parse(BounceRecordSets.ResourceManager.GetString(p.Organization.ParentOrganization.Abbreviation));
                return db.RecordSets.Single(rs => rs.RecordSetID == rsid);
            }
        }

        public static Patron AddNote(this Patron p, string noteText)
        {
            if (p.PatronNote == null)
            {
                p.PatronNote = new PatronNote { NonBlockingStatusNotes = noteText };
            }
            else
            {
                p.PatronNote.NonBlockingStatusNotes += "\r\n" + noteText;
            }

            return p;
        }

        public static Patron ChangeDeliveryOption(this Patron p, string email)
        {
            if (String.Equals(p.PatronRegistration.EmailAddress, email, StringComparison.OrdinalIgnoreCase))
            {
                p.PatronRegistration.DeliveryOptionID = string.IsNullOrWhiteSpace(p.PatronRegistration.PhoneVoice1) ? 1 : 3;
            }
            return p;
        }

        public static Patron RemoveEmail(this Patron p, string email)
        {
            if (String.Equals(p.PatronRegistration.EmailAddress, email, StringComparison.OrdinalIgnoreCase))
            {
                p.PatronRegistration.EmailAddress = "";
            }

            if (String.Equals(p.PatronRegistration.AltEmailAddress, email, StringComparison.OrdinalIgnoreCase))
            {
                p.PatronRegistration.AltEmailAddress = "";
            }
            return p;
        }

    }
}