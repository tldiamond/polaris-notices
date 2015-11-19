using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;

namespace NoticeSuite.Extensions
{
	public static class CallExtension
	{
		public static bool HasRecording(this Call call) 
		{
			return call.StartTime > Variables.RecordingCutOffDate;
		}
	}
}
