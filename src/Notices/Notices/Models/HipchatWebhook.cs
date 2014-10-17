using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Notices
{
    public class HipchatWebhook
    {
        public string @event { get; set; }
        public Item item { get; set; }
        public string oauth_client_id { get; set; }
        public int webhook_id { get; set; }
    }

    public class Links
    {
        public string self { get; set; }
    }

    public class From
    {
        public int id { get; set; }
        public Links links { get; set; }
        public string mention_name { get; set; }
        public string name { get; set; }
    }

    public class Message
    {
        public string date { get; set; }
        public From from { get; set; }
        public string id { get; set; }
        public List<object> mentions { get; set; }
        public string message { get; set; }
    }

    public class Links2
    {
        public string participants { get; set; }
        public string self { get; set; }
        public string webhooks { get; set; }
    }

    public class Room
    {
        public int id { get; set; }
        public Links2 links { get; set; }
        public string name { get; set; }
    }

    public class Item
    {
        public Message message { get; set; }
        public Room room { get; set; }
    }    
}