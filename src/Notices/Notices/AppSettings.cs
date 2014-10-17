using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Notices
{
    public static class AppSettings
    {
        public static string TwilioAccountSid { get { return ConfigurationManager.AppSettings["twilio_account_sid"]; } }
        public static string TwilioAuthToken { get { return ConfigurationManager.AppSettings["twilio_auth_token"]; } }
        public static string DbServer { get { return ConfigurationManager.AppSettings["server_name"]; } }
        public static int StartHour { get { int i; int.TryParse(ConfigurationManager.AppSettings["start_hour"], out i); return i; } }
        public static int StopHour { get { int i; int.TryParse(ConfigurationManager.AppSettings["stop_hour"], out i); return i; } }
        public static string AuthString { get { return ConfigurationManager.AppSettings["auth_string"]; } }
        public static string HipchatNotificationToken { get { return ConfigurationManager.AppSettings["hipchat_auth_token"]; } }
        public static string HipchatSmsNumber { get { return ConfigurationManager.AppSettings["hipchat_sms_number"]; } }
        public static string MandrillApiKey { get { return ConfigurationManager.AppSettings["mandrill_api_key"]; } }
        public static string SmsTestNumber { get { return ConfigurationManager.AppSettings["sms_test_number"]; } }
    }
}