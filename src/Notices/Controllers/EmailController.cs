using Newtonsoft.Json;
using NoticeSuite.Data;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using NoticeSuite.Extensions;
using NoticeSuite.Resources;
using System.Globalization;
using System.Collections;
using NoticeSuite.Models;
using NoticeSuite.Services.Mandrill;
using Clc.Polaris.Api;
using NoticeSuite.Security;


namespace NoticeSuite.Controllers
{
    public class EmailController : Controller
    {
        PolarisEntities pdb;
        NoticeEntities db;

        public EmailController()
        {
            pdb = PolarisEntities.Create();
            db = NoticeEntities.Create();
        }

        public ActionResult Index()
        {
            return Content("");
        }

        [CustomAdminAuthorize]
        public ActionResult TestBounce(string auth, string email)
        {
            if (!AppSettings.EnableEmailProcessing) return Content("email processing disabled in config");

            ProcessBounce(email);
            return Content("test bounce processed");
        }

        [CustomAdminAuthorize]
        public ActionResult TestSpam(string auth, string email, string from)
        {
            if (!AppSettings.EnableEmailProcessing) return Content("email processing disabled in config");

            ProcessSpam(email, from);
            return Content("test spam processed");
        }

        [CustomAdminAuthorize]
        public ActionResult TestUnsub(string auth, string barcode, int? library)
        {
            if (!AppSettings.EnableEmailProcessing) return Content("email processing disabled in config");

            ProcessUnsubscribe(barcode);
            return Content("test unsub processed");
        }

        [HttpGet]
        public ActionResult Unsubscribe()
        {
            return View(new UnsubscribeViewModel());
        }

        [HttpPost]
        public ActionResult Unsubscribe(UnsubscribeViewModel model)
        {
            if (!ModelState.IsValid) return View();
            var polaris = new PolarisApiClient();
            
            if (!polaris.PatronValidate(model.Barcode, model.Pin).ValidPatron)
            {
                ModelState.AddModelError("", "Invalid barcode or pin");
                return View();
            }

            ProcessUnsubscribe(model.Barcode);

            return View("Unsubscribed");
        }

        int GetLibrary(string emailDomain)
        {
            if (!db.EmailDomains.Any(d => d.Domain == emailDomain)) return 0;
            var domain = db.EmailDomains.Single(d => d.Domain == emailDomain);
            if (domain == null) throw new Exception("no domain for organization");
            return domain.OrganizationID;
        }

        [ValidateInput(false)]
        public ActionResult ProcessFailure(FormCollection fc, string auth)
        {
            if (!AppSettings.EnableEmailProcessing) return Content("email processing disabled in config");

            if (!fc.AllKeys.Contains("mandrill_events") || auth != AppSettings.AuthString)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.OK);
            }

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
                        ProcessSpam(ev.Msg.Email, ev.Msg.Sender);
                        break;
                    default:
                        break;
                }
            }
            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }

        Patron ProcessPatron(Patron patron, string email, RecordSetTypes recordSetType, string note)
        {
            if (string.IsNullOrWhiteSpace(email)) return patron;

            AddToRecordSet(recordSetType, patron);
            patron.RemoveEmail(email);
            patron.AddNote(note);
            return patron;
        }

        void ProcessBounce(string email)
        {
            var patrons = pdb.Patrons.Where(p => p.PatronRegistration.EmailAddress == email || p.PatronRegistration.AltEmailAddress == email);
            foreach (var p in patrons)
            {
                ProcessPatron(p, email, RecordSetTypes.Bounce, string.Format("\r\n{0} - Address {1} bounced.", DateTime.Now.ToShortDateString(), email));
            }            

            pdb.SaveChanges();
        }

        void ProcessSpam(string email, string from)
        {
            var patrons = new List<Patron>();

            if (AppSettings.LibrarySpecificSpam)
            {
                var library = GetLibrary(from.Split('@')[1]);
                patrons = pdb.Patrons.Where(p => (p.PatronRegistration.EmailAddress == email || p.PatronRegistration.AltEmailAddress == email) && p.Organization.ParentOrganizationID == library).ToList();
            }
            else
            {
                patrons = pdb.Patrons.Where(p => p.PatronRegistration.EmailAddress == email || p.PatronRegistration.AltEmailAddress == email).ToList();
            }

            foreach (var p in patrons)
            {
                ProcessPatron(p, email, RecordSetTypes.Spam, string.Format("\r\n{0} - Address {1} reported spam from {2}.", DateTime.Now.ToShortDateString(), email, from));
            }

            pdb.SaveChanges();

            if (AppSettings.EnableSpamRejectionRemoval)
            {
                Mandrill.RemoveRejection(email);
            }
        }

        void ProcessUnsubscribe(string barcode)
        {
            var patron = pdb.Patrons.Single(p => p.Barcode == barcode); 
            
            ProcessPatron(patron, patron.PatronRegistration.EmailAddress, RecordSetTypes.Spam, string.Format("\r\n{0} - Address {1} unsubscribed.", DateTime.Now.ToShortDateString(), patron.PatronRegistration.EmailAddress));
            ProcessPatron(patron, patron.PatronRegistration.AltEmailAddress, RecordSetTypes.Spam, string.Format("\r\n{0} - Address {1} unsubscribed.", DateTime.Now.ToShortDateString(), patron.PatronRegistration.AltEmailAddress));

            pdb.SaveChanges();
        }

        void AddToRecordSet(RecordSetTypes rstype, Patron pat)
        {            
            var rsid = db.RecordSets.Single(rs => rs.RecordSetTypeID == (int)rstype && rs.OrganizationID == pat.Organization.ParentOrganizationID).RecordSetID;
            var recordSet = pdb.RecordSets.Single(rs => rs.RecordSetID == rsid);
            if (!recordSet.Patrons.Contains(pat))
            {
                recordSet.Patrons.Add(pat);
            }            
        }

        enum RecordSetTypes
        {
            Bounce = 1,
            Spam
        }      
    }
}
