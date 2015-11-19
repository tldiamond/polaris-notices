namespace NoticeSuite.Models
{
	public class CallResponse
	{
		public string sid { get; set; }
		public string date_created { get; set; }
		public string date_updated { get; set; }
		public object parent_call_sid { get; set; }
		public string account_sid { get; set; }
		public string to { get; set; }
		public string formatted_to { get; set; }
		public string from { get; set; }
		public string formatted_from { get; set; }
		public string phone_number_sid { get; set; }
		public string status { get; set; }
		public object start_time { get; set; }
		public object end_time { get; set; }
		public object duration { get; set; }
		public object price { get; set; }
		public string direction { get; set; }
		public object answered_by { get; set; }
		public string api_version { get; set; }
		public object forwarded_from { get; set; }
		public object caller_name { get; set; }
		public string uri { get; set; }
		public SubresourceUris subresource_uris { get; set; }
	}
}
