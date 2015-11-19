using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace NoticeSuite
{
	public static class AppSettings
	{
        public static string TwilioAccountNumber { get { return ConfigurationManager.AppSettings["twilio_account_number"]; } }
        public static string TwilioAuthToken { get { return ConfigurationManager.AppSettings["twilio_auth_token"]; } }
        public static string SmsTwilioAccountNumber 
        { 
            get { return string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["sms_twilio_account_sid"]) ? TwilioAccountNumber : ConfigurationManager.AppSettings["sms_twilio_account_sid"]; } 
        }
        public static string SmsTwilioAuthToken 
        { 
            get { return string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["sms_twilio_auth_token"]) ? TwilioAuthToken : ConfigurationManager.AppSettings["sms_twilio_auth_token"]; } 
        }
        public static string BaseURL { get { return ConfigurationManager.AppSettings["base_url"].TrimEnd('/') + "/"; } }
        public static string FromPhone { get { return ConfigurationManager.AppSettings["from_phone"]; } }
        public static string TestBarcode { get { return ConfigurationManager.AppSettings["test_barcode"]; } }
        public static string TestPin { get { return ConfigurationManager.AppSettings["test_pin"]; } }
		public static string RenewalNumber { get { return ConfigurationManager.AppSettings["renewal_number"]; } }
		public static string AuthString { get { return ConfigurationManager.AppSettings["auth_string"]; } }
        public static bool EnableDialout { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_dialout"]); } }
        public static bool EnableDialin { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_dialin"]); } }
        public static bool Debug { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["debug"]); } }
        public static bool RecordCalls { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["record_calls"]); } }
        public static string DebugPhoneNumber { get { return ConfigurationManager.AppSettings["debug_phone_number"]; } }
		public static int WeekdayStartHour { get { return int.Parse(ConfigurationManager.AppSettings["weekday_start_hour"]); } }
		public static int WeekdayStopHour { get { return int.Parse(ConfigurationManager.AppSettings["weekday_stop_hour"]); } }
		public static int WeekendStartHour { get { return int.Parse(ConfigurationManager.AppSettings["weekend_start_hour"]); } }
		public static int WeekendStopHour { get { return int.Parse(ConfigurationManager.AppSettings["weekend_stop_hour"]); } }
        public static int SMSStartHour { get { return int.Parse(ConfigurationManager.AppSettings["sms_start_hour"]); } }
        public static int SMSStopHour { get { return int.Parse(ConfigurationManager.AppSettings["sms_stop_hour"]); } }
        public static int ClosedDateBranch { get { return int.Parse(ConfigurationManager.AppSettings["closed_date_branch"]); } }
        public static int TimeoutLimit { get { return int.Parse(ConfigurationManager.AppSettings["timeout_limit"]); } }
        public static string ItivaNumber { get { return ConfigurationManager.AppSettings["itiva_number"]; } }
        public static string DbServer { get { return ConfigurationManager.AppSettings["db_server"]; } }
        public static string HipchatNotificationToken { get { return ConfigurationManager.AppSettings["hipchat_auth_token"]; } }
        public static string HipchatSmsNumber { get { return ConfigurationManager.AppSettings["hipchat_sms_number"]; } }
        public static string SmsTestNumber { get { return ConfigurationManager.AppSettings["sms_test_number"]; } }
        public static string MandrillApiKey { get { return ConfigurationManager.AppSettings["mandrill_api_key"]; } }
        public static string EmailTemplateName { get { return ConfigurationManager.AppSettings["email_template_name"]; } }
        public static bool EnableItivaTransfer { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_itiva_transfer"]); } }
        public static int PinDigits { get { return int.Parse(ConfigurationManager.AppSettings["pin_digit_length"]);} }
        public static bool LibrarySpecificSpam { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["library_specific_spam"]); } }
        public static bool EnableEmailProcessing { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_email_processing"]); } }
        public static bool EnableSmsProcessing { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_sms_processing"]); } }
        public static bool UseWcfForSms { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["use_wcf_to_send"]); } }
        public static bool EnableHangfireMsmq { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_hangfire_msmq"]); } }
        public static bool EnableSpamRejectionRemoval { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_spam_rejection_removal"]); } }
        public static string AdminPermissionGroup { get { return ConfigurationManager.AppSettings["admin_permission_group"]; } }


        public static string TwilioCallURL { get { return AppSettings.BaseURL + "dialout/TwilioResponse"; } }
        public static string TwilioStatusURL { get { return AppSettings.BaseURL + "dialout/TwilioStatus"; } }
        public static string TwilioFallbackURL { get { return AppSettings.BaseURL + "dialout/FallbackResponse"; } }
        public static string DialinURL { get { return AppSettings.BaseURL + "dialin/index"; } }

        public static bool EnableNotifier { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["enable_notifier"]); } }
        public static List<string> NotifyEmailAddresses { get { return (ConfigurationManager.AppSettings["notify_email_addresses"] + ";").Split(';').Where(s => s.Length > 0).ToList(); } }
		public static List<string> NotifyPhoneNumbers { get { return (ConfigurationManager.AppSettings["notify_phone_numbers"] + "").Split(';').Where(s=>s.Length > 0).ToList(); } }
		public static List<int> LibrariesToCall 
		{ 
			get 
			{
				var val = ConfigurationManager.AppSettings["libraries_to_call"];
				if (val.Count(t => t == ';') > 0) { return val.Split(';').Select(t => Convert.ToInt32(t)).ToList(); }

                var temp = 0;
                return int.TryParse(val, out temp) ? new List<int> { temp } : new List<int> { 0 };		
			} 
		}
	}
}
