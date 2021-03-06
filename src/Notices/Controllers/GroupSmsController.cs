﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.TwiML;
using HipchatApiV2;
using RestSharp;
using Newtonsoft.Json;
using NoticeSuite.Data;

namespace NoticeSuite.Controllers
{
    public class GroupSmsController : Controller
    {
        //
        // GET: /Message/

        NoticeEntities db;
        TwilioRestClient twilio = Methods.CreateTwilioClient();
        private static readonly Random r = new Random();
        HipchatClient hipchat = new HipchatClient(AppSettings.HipchatNotificationToken);
        public GroupSmsController()
        {
            db = NoticeEntities.Create();
        }

        public ActionResult Index()
        {
            return View();
        }

        [ValidateInput(false)]
        public string Receive(string Body, string From)
        {            
            switch (Body.ToLower().Split(' ')[0])
            {
                case "join":
                    //Add user here
                    AddUser(From, Body.Split(' ')[1]);
                    break;
                case "leave":
                    goto case "stop";
                case "stop":
                    RemoveUser(From);
                    break;
                default:
                    SendMessage(Body, From);
                    break;
            }

            return new TwilioResponse().ToString();
        }

        void AddUser(string phoneNumber, string name)
        {
            db.SMS_Group.Add(new SMS_Group
            {
                PhoneNumber
                    = phoneNumber,
                Name = name
            });
            db.SaveChanges();
            twilio.SendMessage(AppSettings.HipchatSmsNumber, phoneNumber, "You have been added to the list.");
        }

        void RemoveUser(string phoneNumber)
        {
            db.SMS_Group.Remove(db.SMS_Group.Single(pn => pn.PhoneNumber == phoneNumber));
            db.SaveChanges();
            twilio.SendMessage(AppSettings.HipchatSmsNumber, phoneNumber, "You have been removed from the list.");
        }

        void SendMessage(string message, string from)
        {
            var sender = db.SMS_Group.Single(pn => pn.PhoneNumber == from).Name;
            db.SMS_Group.Where(pn => pn.PhoneNumber != from).ToList().ForEach(pn => twilio.SendSmsMessage(AppSettings.HipchatSmsNumber, pn.PhoneNumber, sender + ": " + message));
            var htmlMessage = string.Format("<b>{0}: </b>{1}", sender, message);
            var result = hipchat.SendNotification("PUG2014", htmlMessage, HipchatApiV2.Enums.RoomColors.Random, true);            
        }

    }
}
