using Mandrill;
using Newtonsoft.Json;
using NLog;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Net;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using Twilio;

namespace Notices.Controllers
{	
    public class HomeController : Controller
    {		
        public HomeController()
        {
        }

        public ActionResult Index()
        {
            return Content("home");
        }
	}
}




