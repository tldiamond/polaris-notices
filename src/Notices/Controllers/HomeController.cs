using Newtonsoft.Json;
using NoticeSuite.Data;
using NoticeSuite.Services.Mandrill;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;

namespace NoticeSuite.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public string Index()
        {
            var test = Convert.ToBoolean("true");
            return "index";
        }
    }
}