using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Notices
{
    public class RejectionRemovalFailure
    {
        public string status { get; set; }
        public int code { get; set; }
        public string name { get; set; }
        public string message { get; set; }        
    }
}