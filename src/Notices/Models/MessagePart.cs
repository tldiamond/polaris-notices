using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using RestSharp;
using Twilio.TwiML;

namespace NoticeSuite.Models
{	
	public class MessagePart
	{
		public PartType PartType { get; set; }
		public string PartText { get; set; }
		object voiceAttribute = new { voice = "alice" };

		public override string ToString()
		{
			return PartText;
		}
	}

	public enum PartType
	{
		String,
		File
	}

	public enum MessageType
	{
        Goodbye = 1,
		Greeting,
        HoldDateFormats,
        HoldMessageFirst,
        HoldMessageAdditional,
        placeholder,
		Intro,
		Location,
		Overdue,
		Renewal,
		Repeat  
        
	}
}
