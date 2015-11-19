using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoticeSuite
{
    public class Sender
    {
        public string address { get; set; }
        public string created_at { get; set; }
        public int sent { get; set; }
        public int hard_bounces { get; set; }
        public int soft_bounces { get; set; }
        public int rejects { get; set; }
        public int complaints { get; set; }
        public int unsubs { get; set; }
        public int opens { get; set; }
        public int clicks { get; set; }
        public int unique_opens { get; set; }
        public int unique_clicks { get; set; }
    }

    public class RejectionListEntry
    {
        public string email { get; set; }
        public string reason { get; set; }
        public string detail { get; set; }
        public string created_at { get; set; }
        public string last_event_at { get; set; }
        public string expires_at { get; set; }
        public bool expired { get; set; }
        public Sender sender { get; set; }
        public string subaccount { get; set; }
    }
}