using NoticeSuite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NoticeSuite.Extensions;
using NoticeSuite.Models;
using Twilio;

namespace NoticeSuite.Services.SMS
{
    public static class SMS
    {
        public static bool sendingQueue = false;
        static Random r = new Random();
        static TwilioRestClient twilio = new TwilioRestClient(AppSettings.SmsTwilioAccountNumber, AppSettings.SmsTwilioAuthToken);

        public static void SendMessages(IEnumerable<MailEvent> events)
        {
            if (!isValidRunTime)
            {
                SaveToQueue(events);
                return;
            }

            if (!AppSettings.UseWcfForSms)
            {
                foreach (var ev in events)
                {
                    twilio.SendMessage(fromNumber, ev.Msg.CleanTo, ev.Msg.CleanBody);
                }
            }

            new SmsService.SmsServiceClient().Using(client =>
            {
                if (!client.CheckIfRunning())
                {
                    Notifier.Create("SMS WCF").Notify("Check SMS WCF", NotificationMethod.SMS);
                }                

                foreach (var ev in events)
                {
                    client.SendSms(ev.Msg.CleanTo, ev.Msg.CleanBody, AppSettings.SmsTwilioAccountNumber, AppSettings.SmsTwilioAuthToken, "");
                }
            });
        }

        public static void SaveToQueue(IEnumerable<MailEvent> events)
        {
            if (!AppSettings.EnableSmsProcessing) return;

            using (var db = NoticeEntities.Create())
            {
                foreach (var ev in events)
                {
                    db.SMS_Queue.Add(new SMS_Queue
                    {
                        PhoneNumber = ev.Msg.CleanTo,
                        Message = ev.Msg.CleanBody,
                        InsertDate = DateTime.Now
                    });
                    //logger.LogInfo("Message queued", ev.Msg.CleanTo, ev.Msg.CleanBody);
                }
                db.SaveChanges();
            }
        }

        public static void SendTestMessage()
        {
            if (!AppSettings.EnableSmsProcessing) return;

            SaveToQueue(
                new List<MailEvent>() 
                { 
                    new MailEvent 
                    { 
                        Msg = new NoticeSuite.Models.Message 
                        {
                            To = new string[][] { new string[] { AppSettings.SmsTestNumber, "" } },
                            Text = "test sms message" 
                        }
                    }
                }
            );

            SendQueue();
        }

        public static void SendQueue()
        {
            if (!AppSettings.EnableSmsProcessing) return;

            if (SMS.sendingQueue) return;

            SMS.sendingQueue = true;
            using (var db = NoticeEntities.Create())
            {
                if (!AppSettings.UseWcfForSms)
                {
                    foreach (var msg in db.SMS_Queue)
                    {
                        twilio.SendMessage(fromNumber, msg.PhoneNumber, msg.Message);
                    }
                }
                else
                {
                    new SmsService.SmsServiceClient().Using(client =>
                    {
                        if (!client.CheckIfRunning())
                        {
                            Notifier.Create("SMS WCF").Notify("Check SMS WCF", NotificationMethod.SMS);
                        }

                        foreach (var msg in db.SMS_Queue)
                        {
                            client.SendSms(msg.PhoneNumber, msg.Message, AppSettings.SmsTwilioAccountNumber, AppSettings.SmsTwilioAuthToken, "");

                            //logger.LogInfo("Message sent from queue", msg.PhoneNumber, msg.Message, result.Sid);
                            db.SMS_Queue.Remove(msg);
                        }
                    });
                }

                db.SaveChanges();
            }
            SMS.sendingQueue = false; 
        }

        public static string fromNumber
        {
            get
            {
                var numbers = twilio.ListIncomingPhoneNumbers();
                return numbers.IncomingPhoneNumbers[r.Next(0, numbers.IncomingPhoneNumbers.Count())].PhoneNumber;
            }
        }

        public static bool isValidRunTime
        {
            get
            {
                if (AppSettings.SMSStartHour == 0 || AppSettings.SMSStopHour == 0) { return false; }
                return DateTime.Now.Hour >= AppSettings.SMSStartHour && DateTime.Now.Hour < AppSettings.SMSStopHour;
            }
        }
    }
}