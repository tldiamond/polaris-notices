using NoticeSuite.Data;
using Twilio;
namespace NoticeSuite.Models
{
	public class TwilioCall
	{
		public int PatronID { get; set; }
		public string CallSid { get; set; }

        public TwilioCall()
        {

        }

        public TwilioCall(int patronId, Call call)
        {
            PatronID = patronId;
            CallSid = call.Sid;
        }
	}
}
