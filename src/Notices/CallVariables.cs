using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Clc.Polaris.Api;
using NoticeSuite.Data;

namespace NoticeSuite
{
	public static class CallVars
	{
		public static Dictionary<string, string> PatronBarcodes = new Dictionary<string, string>();
		public static Dictionary<string, string> PatronPINs = new Dictionary<string, string>();
        public static Dictionary<string, PatronData> PatronData = new Dictionary<string, PatronData>();
        public static Dictionary<string, PatronValidateResult> OtherPatronInfo = new Dictionary<string,PatronValidateResult>();
        public static Dictionary<string, List<GetCheckedOutItems_Result>> ItemsOut = new Dictionary<string, List<GetCheckedOutItems_Result>>();
        public static Dictionary<string, ItemType> ItemType = new Dictionary<string, ItemType>();
		public static Dictionary<string, PatronHoldRequestsGetResult> Holds = new Dictionary<string, PatronHoldRequestsGetResult>();
		public static Dictionary<string, ItemsOutActionResult> ItemRenewResults = new Dictionary<string, ItemsOutActionResult>();
		public static Dictionary<string, int> ItemsOutIndex = new Dictionary<string, int>();
		public static Dictionary<string, int> HoldIndex = new Dictionary<string, int>();
		public static Dictionary<string, bool> SkipIntro = new Dictionary<string, bool>();
        public static Dictionary<string, int> Timeout = new Dictionary<string, int>();
        public static Dictionary<string, bool> TryItiva = new Dictionary<string, bool>();
	}
}