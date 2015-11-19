using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio;

namespace NoticeSuite.Model
{
	public class callListModel
	{
		public List<Call> Calls { get; set; }
		public string PhoneNumber { get; set; }
	}
}