using NoticeSuite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NoticeSuite.Extensions;
using Clc.Polaris.Api;
using NoticeSuite.Models;
using Twilio;

namespace NoticeSuite.Services.Dialout
{
    public static class Dialout
    {
        private static MyLogger logger = MyLogger.GetLogger();
        static Notifier notifier = new Notifier("Dialout");

        public static string Run(bool startedByJob = false, bool ignoreTime = false)
        {
            if (!AppSettings.EnableDialout) return "dialout disabled in config";
            // Check to see if we are already making calls to avoid making duplicates and generally screwing things up.
            if (Variables.InProcess)
            {
                // If this run was initiate by the task scheduler job we want to cancel all in process calls and force the process
                // to start once again.
                if (startedByJob)
                {
                    //logger.Info(string.Format(Internal.CancellingCalls, Variables.CallsInProcess), action:Actions.None);
                    Methods.CancelCallsInProcess();
                }
                // If this run was not started by the task scheduler job we don't want to force the calls. This provides some
                // extra protection if we for some reason have to start it manually as it won't cancel all the calls and hang
                // up on patrons or give them a generic awful message.
                else
                {
                    //EventWriter.WriteError("Calls already in process");
                    logger.Debug(Internal.AlreadyRunning);
                    return Internal.AlreadyRunning;
                }
            }

            // Check that we should make calls at the current hour.
            if (!IsValidCallTime() && !ignoreTime)
            {
                logger.Debug(Internal.OutsideRunHours);
                return Internal.OutsideRunHours;
            }

            // If most libraries are closed today don't make calls.
            if (!IsValidCallDate() && !ignoreTime)
            {
                logger.Debug(Internal.ClosedToday);
                return Internal.ClosedToday;
            }

            if (!CheckPolarisApi())
            {
                logger.Error(Internal.ApiDown);
                notifier.Notify(Internal.ApiDown, NotificationMethod.All);
                return Internal.ApiDown;
            }

            //var testResult = DoTests();
            //if (!String.IsNullOrWhiteSpace(testResult))
            //{
            //    notifier.Notify("Error in dialout application", NotificationMethod.All);
            //    return testResult;
            //}


            // If all of the required conditions are met then finally begin to make phone calls.
            Methods.SetVariablesForNewRun();
            Methods.PopulateStrings();
            LoadNotifications();
            RemovePreviouslyCalledPatrons();
            logger.Info(string.Format(Internal.CalledPatronCountPre, Variables.Notifications.GroupBy(n => n.PatronID).Count()));
            CallPatrons();
            return String.Format("Called {0} patrons", Variables.CallsPlaced);
        }

        static void LoadNotifications()
        {
            using (var db = NoticeEntities.Create())
            {
                Variables.Notifications = db.PolarisNotifications.ToList();
            }

            if (AppSettings.LibrariesToCall.Count > 1 || AppSettings.LibrariesToCall.First() != 0)
            {
                Variables.Notifications = Variables.Notifications.Where(n => AppSettings.LibrariesToCall.Contains(n.PatronLibrary ?? 0)).ToList();
            }

            if (Variables.Notifications.Count() == 0)
            {
                logger.Info(Internal.NoNotifications);
                return;
            }

            // If we're testing only grab notices for one patron.
            if (AppSettings.Debug) Variables.Notifications = Variables.Notifications.GroupBy(n => n.PatronID).First().ToList();
        }

        static void RemovePreviouslyCalledPatrons()
        {
            List<TodaysHoldCall> calledPatrons;
            using (var db = NoticeEntities.Create())
            {
                calledPatrons = db.TodaysHoldCalls.ToList();
            }

            if (AppSettings.LibrariesToCall.Count > 1 || AppSettings.LibrariesToCall.First() != 0)
            {
                calledPatrons = calledPatrons.Where(n => AppSettings.LibrariesToCall.Contains(n.ReportingLibraryID)).ToList();
            }

            if (calledPatrons.Count() == 0)
            {
                logger.Info(Internal.NoHoldCallsYet);
                return;
            }

            foreach (var calledPatronNotices in Variables.Notifications
                .Where(n => calledPatrons.Any(cp => cp.PatronID == n.PatronID && cp.ReportingOrgId == n.ReportingBranchID))
                .GroupBy(n => n.PatronID))
            {
                foreach (var notice in calledPatronNotices)
                {
                    var result = notice.NotificationUpdate(1, Internal.PatronAlreadyCalledHold);

                    if (result.PAPIErrorCode != 0)
                        logger.Error(result.ErrorMessage, patronid: notice.PatronID, itemrecordid: notice.ItemRecordID);
                    else
                        logger.Info(Internal.PatronAlreadyCalledHold, null, notice.PatronID, null, notice.ItemRecordID, notice.NotificationTypeID);
                }

                Variables.Notifications.RemoveAll(n => n.PatronID == calledPatronNotices.First().PatronID);
            }
        }

        static void CallPatrons()
        {
            var twilio = Methods.CreateTwilioClient();

            foreach (var noticesForPatron in Variables.Notifications.GroupBy(n => n.PatronID))
            {
                var firstNotice = noticesForPatron.First();

                // If we're testing call Mike's google voice number.
                if (AppSettings.Debug) firstNotice.DeliveryString = AppSettings.DebugPhoneNumber;

                var call = twilio.InitiateOutboundCall
                (
                    new CallOptions
                    {
                        From = AppSettings.FromPhone,
                        To = firstNotice.DeliveryString,
                        IfMachine = "Continue",
                        FallbackUrl = AppSettings.TwilioFallbackURL,
                        StatusCallback = AppSettings.TwilioStatusURL,
                        Url = AppSettings.TwilioCallURL,
                        Record = AppSettings.RecordCalls
                    }
                );
                if (!string.IsNullOrWhiteSpace(call.Sid))
                {
                    Variables.CallsPlaced++;
                    Variables.Calls.Add(new TwilioCall(firstNotice.PatronID, call));
                    logger.Debug("Call placed", call.Sid, firstNotice.PatronID);
                }
                else
                {
                    logger.Info("Call unable to be placed - " + call.RestException.Message, null, firstNotice.PatronID, null, null, null);

                    foreach (var notice in noticesForPatron)
                    {
                        notice.NotificationUpdate(7, "Unable to call patron - " + call.RestException.Message);
                    }
                }
            }
            //EventWriter.LogInfo(string.Format("Made {0} calls.", Variables.Calls.Count));
        }

        static bool CheckPolarisApi()
        {
            var papi = new PolarisApiClient();
            var x = papi.OrganizationsGet(OrganizationType.System);

            try
            {
                return x.OrganizationsGetRows.Count == 1;
            }
            catch
            {
                return false;
            }
        } 

        static bool IsValidCallDate()
        {
            using (var db = NoticeEntities.Create())
            {
                return !db.GetClosedDates(AppSettings.ClosedDateBranch).Select(dc => dc.DateClosed).Contains(DateTime.Today);
            }
        }

        static bool IsValidCallTime()
        {
            int startHour = 0, stopHour = 0;
            switch (DateTime.Now.DayOfWeek.ToString().ToLower())
            {
                case "monday":
                    goto case "weekday";
                case "tuesday":
                    goto case "weekday";
                case "wednesday":
                    goto case "weekday";
                case "thursday":
                    goto case "weekday";
                case "friday":
                    goto case "weekday";
                case "saturday":
                    goto case "weekend";
                case "sunday":
                    goto case "weekend";

                case "weekday":
                    startHour = AppSettings.WeekdayStartHour;
                    stopHour = AppSettings.WeekdayStopHour;
                    break;
                case "weekend":
                    startHour = AppSettings.WeekendStartHour;
                    stopHour = AppSettings.WeekendStopHour;
                    break;
                default:
                    logger.Error(String.Format(Internal.DayOfWeekFail, DateTime.Now.DayOfWeek.ToString().ToLower()));
                    goto case "weekend";
            }

            var currentHour = int.Parse(DateTime.Now.ToString("HH"));
            if (currentHour >= startHour && currentHour <= stopHour)
            {
                return true;
            }

            return false;
        }
    }
}