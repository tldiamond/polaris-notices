using NoticeSuite.Data;
using NoticeSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace NoticeSuite.Extensions
{
    public static class OrganizationExtensions
    {
        public static MessagePart GetString(this PolarisNotification notice, MessageType type, bool tryParentOrg = true)
        {
            var msgString = "";

            int stringType = (int)type;

            var messageBranch = notice.PatronBranch;
            var messageLibrary = notice.PatronLibrary;

            if (type == MessageType.Location)
            {
                messageBranch = notice.ReportingBranchID;
                messageLibrary = notice.ReportingLibraryID;
            }

            try { msgString = Variables.Strings.SingleOrDefault(s => s.OrganizationID == messageBranch && s.StringTypeID == stringType).Value; }
            catch { }

            if (string.IsNullOrWhiteSpace(msgString))
            {
                if (tryParentOrg)
                {
                    try { msgString = Variables.Strings.SingleOrDefault(s => s.OrganizationID == messageLibrary && s.StringTypeID == stringType).Value; }
                    catch { }
                }
            }

            if (string.IsNullOrWhiteSpace(msgString))
                msgString = Variables.Strings.Single(s => s.OrganizationID == 1 && s.StringTypeID == stringType).Value;

            if (msgString.StartsWith("http"))
            {
                return new MessagePart { PartType = PartType.File, PartText = msgString };
            }

            return new MessagePart { PartType = PartType.String, PartText = msgString.Reverse().First() == ' ' ? msgString : msgString + " " };
        }

        
    }
}
