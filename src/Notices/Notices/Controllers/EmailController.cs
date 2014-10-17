using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Web;
using System.Web.Mvc;

namespace Notices.Controllers
{
    public class EmailController : Controller
    {
        PolarisEntities db;

        public EmailController()
        {
            db = PolarisEntities.Create();
        }

        public ActionResult Test()
        {
            //ProcessBounce("asdf@asdf.com");
            return Content("");
        }        

        [ValidateInput(false)]
        public ActionResult ProcessFailure(FormCollection fc, string auth)
        {
            if (!fc.AllKeys.Contains("mandrill_events") || auth != AppSettings.AuthString)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.OK);
            }

            var test = fc["mandrill_events"];
            var test2 = test;

            var events = fc.GetEvents();

            foreach (var ev in events)
            {
                switch (ev.Event.ToLower())
                {
                    case "hard_bounce":
                        ProcessBounce(ev.Msg.Email);
                        break;
                    case "soft_bounce":
                        if (ev.Msg.BounceDescription.ToLower().Contains("general"))
                        {
                            goto case "hard_bounce";
                        }
                        return Content("");
                    case "reject":
                        goto case "hard_bounce";
                    case "spam":
                        ProcessSpam(ev.Msg.Email);
                        break;
                    default:
                        break;
                }
            }

            return Content("");
        }

        void ProcessBounce(string email)
        {
            var patrons = db.Patrons.Where(p => p.PatronRegistration.EmailAddress == email || p.PatronRegistration.AltEmailAddress == email);
            foreach (var p in patrons)
            {
                AddToRecordSet(RecordSetTypes.Bounce, p);
                p.ChangeDeliveryOption(email);
                p.RemoveEmail(email);
                p.AddNote(string.Format("\r\n{0} - Address {1} bounced.", DateTime.Now.ToShortDateString(), email));
            }

            db.SaveChanges();
        }

        void ProcessSpam(string email)
        {
            var patrons = db.Patrons.Where(p => p.PatronRegistration.EmailAddress == email || p.PatronRegistration.AltEmailAddress == email);
            foreach (var p in patrons)
            {
                AddToRecordSet(RecordSetTypes.Spam, p);
                p.ChangeDeliveryOption(email);
                p.RemoveEmail(email);
                p.AddNote(string.Format("\r\n{0} - Address {1} reported spam.", DateTime.Now.ToShortDateString(), email));
            }

            db.SaveChanges();
        }

        void AddToRecordSet(RecordSetTypes rstype, Patron pat)
        {
            ResourceManager rm;

            if (rstype == RecordSetTypes.Bounce) { rm = BounceRecordSets.ResourceManager; }
            else if (rstype == RecordSetTypes.Spam) { rm = SpamRecordSets.ResourceManager; }
            else { throw new Exception("Invalid recordset type"); }

            var rsid = int.Parse(rm.GetString(pat.Organization.ParentOrganization.Abbreviation));
            db.RecordSets.Single(rs => rs.RecordSetID == rsid).Patrons.Add(pat);
        }

        enum RecordSetTypes
        {
            Bounce,
            Spam
        }      
    }
}
