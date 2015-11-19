using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoticeSuite.Data
{
    public partial class PolarisNotification
    {
        public bool IsOverdue
        {
            get
            {
                return new int[] { 1, 12, 13 }.Contains(NotificationTypeID);
            }
        }

        public bool IsHold
        {
            get
            {
                return NotificationTypeID == 2;
            }
        }

        public bool Is2ndHold
        {
            get
            {
                return NotificationTypeID == 18;
            }
        }
    }
}