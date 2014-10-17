using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HipchatApiV2;
using System.Text;
using Newtonsoft.Json;
using Twilio;

namespace Notices.Controllers
{
    public class HipchatController : Controller
    {
        TwilioRestClient twilio = new TwilioRestClient(AppSettings.TwilioAccountSid, AppSettings.TwilioAuthToken);
        PolarisEntities db;
        HipchatClient hipchat = new HipchatClient(AppSettings.HipchatNotificationToken);
        public HipchatController()
        {
            db = PolarisEntities.Create();
        }

        public string Index()
        {
            return "";
        }

        public ActionResult Receive()
        {
            var stream = Request.InputStream;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            string json = Encoding.UTF8.GetString(buffer);
            var data = JsonConvert.DeserializeObject<HipchatWebhook>(json);

            var msg = new string(data.item.message.message.Skip(5).ToArray());

            db.CLC_Custom_SMS_Group.ToList().ForEach(pn => twilio.SendSmsMessage(AppSettings.HipchatSmsNumber, pn.PhoneNumber, data.item.message.from.name + ": " + msg));
            return Content("message sent");
        }

    }


    
}
