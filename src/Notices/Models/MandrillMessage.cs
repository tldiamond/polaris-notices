using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoticeSuite.Models
{

    public class SmtpEvent
    {
        public int ts { get; set; }
        public string type { get; set; }
        public string diag { get; set; }
        public string source_ip { get; set; }
        public string destination_ip { get; set; }
        public int size { get; set; }
    }

    public class OpensDetail
    {
        public int ts { get; set; }
        public string ip { get; set; }
        public string ua { get; set; }
        public string location { get; set; }
    }

    public class ClicksDetail
    {
        public int ts { get; set; }
        public string url { get; set; }
        public string ip { get; set; }
        public string location { get; set; }
        public string ua { get; set; }
    }

    public class MandrillMessage
    {
        public int ts { get; set; }
        public string _id { get; set; }
        public string state { get; set; }
        public string subject { get; set; }
        public string email { get; set; }
        public List<object> tags { get; set; }
        public int opens { get; set; }
        public int clicks { get; set; }
        public List<SmtpEvent> smtp_events { get; set; }
        public List<object> resends { get; set; }
        public string sender { get; set; }
        public string template { get; set; }
        public List<OpensDetail> opens_detail { get; set; }
        public List<ClicksDetail> clicks_detail { get; set; }
    }

}