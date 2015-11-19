using Newtonsoft.Json;
using NoticeSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NoticeSuite.Extensions
{
    public static class FormCollectionExtension
    {
        public static IEnumerable<MailEvent> GetEvents(this FormCollection fc)
        {
            string json = fc["mandrill_events"];
            return JsonConvert.DeserializeObject<IEnumerable<MailEvent>>(json);
        }
    }
}