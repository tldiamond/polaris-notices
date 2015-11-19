using Clc.Polaris.Api;
using Newtonsoft.Json;
using NoticeSuite.Data;
using NoticeSuite.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.TwiML;

namespace NoticeSuite.Extensions
{
	public static class TwilioResponseExtension
	{
        static object voiceAttribute = new { voice = "alice" };
        static PolarisApiClient polaris = new PolarisApiClient();
		public static TwilioResponse AddMessagePart(this TwilioResponse response, MessagePart part, string additionalText = "")
		{
			if (part.PartType == PartType.File)
			{
				response.Play(part.PartText);
				if (additionalText.Length > 0)
				{
					response.Say(additionalText, voiceAttribute);
				}
			}
			else
			{
				if (additionalText.Length > 0)
				{
					response.Say(string.Format("{0} {1}", part.PartText, additionalText), voiceAttribute);
				}
				else
				{
					response.Say(part.PartText, voiceAttribute);
				}
			}

			return response;
		}

        public static TwilioResponse BuildMainMenu(this TwilioResponse twiml, string callsid)
        {
            var itemsOut = CallVars.ItemsOut[callsid].ToList();

            int almostOverdueCount = 0, overdueCount = 0;
            if (itemsOut.Count() > 0)
            {
                almostOverdueCount = itemsOut.Count(item => item.DueDate <= DateTime.Today.AddDays(7) && item.DueDate >= DateTime.Today);
                overdueCount = itemsOut.Count(i => i.DueDate <= DateTime.Now);
            }

            twiml.BeginGather(new { action = "/dialin/index?node=default", numDigits = "1" });

            CallVars.SkipIntro[callsid] = CallVars.SkipIntro.Keys.Contains(callsid) ? CallVars.SkipIntro[callsid] : false;

            // Only play the intro if the patron is initially calling in and has not heard it.
            if (!CallVars.SkipIntro[callsid])
            {
                CallVars.SkipIntro[callsid] = true;
                twiml.Say(string.Format("Hello {0}, thank you for calling the {1}.", CallVars.PatronData[callsid].NameFirst.ToLower(), CallVars.OtherPatronInfo[callsid].AssignedBranchName), voiceAttribute);
                twiml.Say("Press 0 at any time to be connected to your library or press 7 to be taken to this menu.", voiceAttribute);

                var itemsMsg = string.Format("You currently have {0} {1} checked out.", itemsOut.Count(), "item".Pluralize(itemsOut.Count()));
                if (overdueCount > 0) itemsMsg += string.Format(" {0} {1} currently overdue.", overdueCount, overdueCount == 1 ? "item is" : "items are");
                if (almostOverdueCount > 0) itemsMsg += string.Format(" {0} {1} due within the next 7 days.", almostOverdueCount, almostOverdueCount == 1 ? "item is" : "items are");

                twiml.Say(itemsMsg, voiceAttribute);
            }
            else
            {
                twiml.Say("Main menu.", voiceAttribute);
            }

            CallVars.Holds[callsid] = CallVars.Holds.Keys.Contains(callsid) ?
                CallVars.Holds[callsid] :
                polaris.PatronHoldRequestsGet(CallVars.PatronBarcodes[callsid], CallVars.PatronPINs[callsid], HoldStatus.held);

            var holdsCount = CallVars.Holds[callsid].PatronHoldRequestsGetRows.Count;
            if (holdsCount > 0)
            {
                twiml.Say(string.Format("You have {0} {1} ready for pickup. To hear hold information, press 4.", holdsCount, "item".Pluralize(holdsCount)), voiceAttribute);
            }
            twiml.Say("To renew all items, press 1.", voiceAttribute);
            twiml.Say("To list the items you currently have checked out and renew individual items, press 2.", voiceAttribute);
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=default");

            return twiml;
        }

        public static TwilioResponse ListHolds(this TwilioResponse twiml, string callsid)
        {
            //var test = polaris.PatronHoldRequestsGet(CallVars.PatronBarcodes[callsid], CallVars.PatronPINs[callsid], HoldStatus.held);
            // If there is a list of holds for the current call then use that, otherwise use the Polaris API
            // to get the holds for current patron.
            CallVars.Holds[callsid] = CallVars.Holds.Keys.Contains(callsid) ?
                CallVars.Holds[callsid] :
                polaris.PatronHoldRequestsGet(CallVars.PatronBarcodes[callsid], CallVars.PatronPINs[callsid], HoldStatus.held);

            var holds = CallVars.Holds[callsid];
            CallVars.HoldIndex[callsid] = CallVars.HoldIndex.Keys.Contains(callsid) ? CallVars.HoldIndex[callsid] : 0;

            if (holds.PatronHoldRequestsGetRows == null)
            {
                twiml.Say("You currently have no holds ready for pickup. Returning to main menu", voiceAttribute);
                twiml.Redirect("/dialin/index?node=default");
                return twiml;
            }

            if (CallVars.HoldIndex[callsid] + 1 > CallVars.Holds[callsid].PatronHoldRequestsGetRows.Count)
            {
                CallVars.HoldIndex[callsid] = 0;
                twiml.Say("You have no more holds to list. Returning to main menu.", voiceAttribute);
                twiml.Redirect("/dialin/index?node=default");
                return twiml;
            }

            if (CallVars.HoldIndex[callsid] == 0)
            {
                twiml.Say(string.Format("You currently have {0} {1} ready for pickup.", holds.PatronHoldRequestsGetRows.Count, "hold".Pluralize(holds.PatronHoldRequestsGetRows.Count)), voiceAttribute);
            }

            var currentHold = holds.PatronHoldRequestsGetRows[CallVars.HoldIndex[callsid]];

            twiml.BeginGather(new { action = "/dialin/index?node=holdOptions", numDigits = "1", timeout = "2" });
            twiml.Say(String.Format("{0}, {1}. available for pickup at {2} until {3}.",
                holds.PatronHoldRequestsGetRows.Count == CallVars.HoldIndex[callsid] + 1 ?
                    holds.PatronHoldRequestsGetRows.Count == 1 ?
                        "Your hold ready for pickup is" :
                        "Your final hold ready for pickup is" :
                    string.Format("Your {0} hold ready for pickup is", IntToString((CallVars.HoldIndex[callsid] + 1))),
                FormatTitle(currentHold.Title),
                currentHold.PickupBranchName,
                currentHold.PickupByDate.HasValue ? currentHold.PickupByDate.Value.ToString("dddd MMMM dd") : ""), voiceAttribute);
            twiml.Pause(1);

            twiml.Say("To hear your next hold, press 1. To return to the main menu, press 7.", voiceAttribute);
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=holdOptions");

            return twiml;
        }

        public static TwilioResponse NextHold(this TwilioResponse twiml, string callsid)
        {
            CallVars.HoldIndex[callsid]++;
            twiml.Redirect("/dialin/index?node=default&digits=4");
            return twiml;
        }

        public static TwilioResponse CancelHold(this TwilioResponse twiml, string callsid)
        {
            twiml.BeginGather(new { action = "/dialin/index?node=holdOptions", numDigits = 1, timeout = 7 });
            twiml.Say("Cancelling this hold will remove you from the queue and you will be unable to reclaim your spot in line. To cancel this hold press 5. To return to your list of holds press 1. To return to the main menu press 7.", voiceAttribute);
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=default&digits=4");
            return twiml;
        }

        public static TwilioResponse ConfirmCancel(this TwilioResponse twiml, string callsid)
        {
            var currentHold = CallVars.Holds[callsid].PatronHoldRequestsGetRows[CallVars.HoldIndex[callsid]];
            var barcode = CallVars.PatronBarcodes[callsid];
            var pin = CallVars.PatronPINs[callsid];


            var result = polaris.HoldRequestCancel(barcode, currentHold.HoldRequestID, pin);

            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                CallVars.Holds[callsid] = polaris.PatronHoldRequestsGet(CallVars.PatronBarcodes[callsid], CallVars.PatronPINs[callsid], HoldStatus.held);
                twiml.Say("Hold successfully cancelled, returning to hold list", voiceAttribute);
                twiml.Redirect("/dialin/index?node=default&digits=4");
                return twiml;
            }

            twiml.Say("Your hold could not be renewed for the following reason: ", voiceAttribute);
            twiml.Say(result.ErrorMessage);
            twiml.Say("Please contact the library for more information. Returning to hold list.", voiceAttribute);
            twiml.Redirect("/dialin/index?node=default&digits=4");
            return twiml;
        }

        public static TwilioResponse BuildItemListMenu(this TwilioResponse twiml, string callsid)
        {
            CallVars.ItemType[callsid] = ItemType.All;
            List<GetCheckedOutItems_Result> overdueItems;
            using (var db = NoticeEntities.Create())
            {
                CallVars.ItemsOut[callsid] = db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, null).ToList();
                overdueItems = db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, 1).ToList();
            }
            CallVars.ItemsOutIndex[callsid] = 0;

            // Tell the patron if they have no items out.
            if (CallVars.ItemsOut[callsid].Count() < 1)
            {
                twiml.Say("You currently have no items checked out. Returning to main menu.", voiceAttribute);
                twiml.Redirect("/dialin/index?node=default");
                return twiml;                
            }

            
            if (overdueItems.Count() > 0)
            {
                twiml.BeginGather(new { action = "/dialin/index?node=overdueOptions", numDigits = 1 });
                twiml.Say(string.Format("You currently have {0} {1} overdue. To list only {2} press 1.", overdueItems.Count(), "item".Pluralize(overdueItems.Count()), overdueItems.Count() == 1 ? "this item" : "these items"), voiceAttribute);
                twiml.Say("To hear a list of all items you currently have checked out press 2.", voiceAttribute);
                twiml.Say("To look up an item by barcode press 3.", voiceAttribute);
                if (!string.IsNullOrWhiteSpace(CallVars.PatronData[callsid].EmailAddress))
                {
                    twiml.Say("To have a list of your checked out titles sent to your email address, press 4", voiceAttribute);
                }

                twiml.Say("To return to the main menu press 7.", voiceAttribute);
                twiml.EndGather();
                twiml.Redirect("/dialin/index?node=listItems");
                return twiml;
            }

            twiml.BeginGather(new { action = "/dialin/index?node=listItems", numDigits = 1 });
            twiml.Say("To hear a list of all titles you currently have checked out press 1.", voiceAttribute);
            twiml.Say("To hear a list of only your overdue items press 2.", voiceAttribute);
            twiml.Say("To look up an item by barcode press 3.", voiceAttribute);
            if (!string.IsNullOrWhiteSpace(CallVars.PatronData[callsid].EmailAddress))
            {
                twiml.Say("To have a list of your checked out titles sent to your email address, press 4", voiceAttribute);
            }
            twiml.Say("To return to the main menu press 7.", voiceAttribute);
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=listItems");
            return twiml;
        }

        public static TwilioResponse ListOverdueItems(this TwilioResponse twiml, string callsid)
        {
            if (CallVars.ItemType[callsid] != ItemType.Overdue)
            {
                CallVars.ItemType[callsid] = ItemType.Overdue;
                using (var db = NoticeEntities.Create())
                {
                    CallVars.ItemsOut[callsid] = db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, 1).ToList();
                }                
                CallVars.ItemsOutIndex[callsid] = 0;
            }

            return twiml.ListItems(callsid);
        }

        public static TwilioResponse EmailTitles(this TwilioResponse twiml, string callsid)
        {
            var items = new List<GetCheckedOutItems_Result>();
            var patron = CallVars.PatronData[callsid];
            using (var db = NoticeEntities.Create())
            {
                items= db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, null).ToList();
            }
            
            if (string.IsNullOrEmpty(patron.EmailAddress))
            {
                twiml.Redirect("/dialin/index?node=default");
                return twiml;
            }

            if (items.Count() < 1)
            {
                twiml.Say("You currently have no items checked out, returning to the main menu.", voiceAttribute);
                twiml.Redirect("/dialin/index?node=default");
                return twiml;
            }

            var sb = new StringBuilder();

            sb.Append(@"<table style=""width:75%; text-align:left;border-collapse:collapse;"">");
            sb.Append(@"<tr><th align=left style=""width:75%;border-bottom: 1px solid #000;"">Title</th><th align=left style=""width:25%; padding-left:15px;border-bottom: 1px solid #000;"">Due Date</th></tr>");
            foreach (var item in items.OrderBy(i=>i.DueDate))
            {
                sb.Append(string.Format(@"<tr><td style=""width:90%; padding-top:4px;vertical-align: top;"">{0}</td><td style=""width:10%; padding-top:4px; padding-left:15px;vertical-align: top;"">{1}</td></tr>", item.BrowseTitle, item.DueDate.ToShortDateString()));
            }
            sb.Append("</table>");

            var client = new RestClient("https://mandrillapp.com/api/1.0/");
            var request = new RestRequest("messages/send-template.json", Method.POST);

            var json = new
            {
                key = AppSettings.MandrillApiKey,
                template_name = AppSettings.EmailTemplateName,
                template_content = new[]
                {
                    new { name = "titles", content = sb.ToString() }
                },
                message = new
                {
                    to = new[]
                    {
                        new
                        {
                        email = patron.EmailAddress,
                        name = patron.NameFirst + " " + patron.NameLast,
                        type = "to"
                        }
                    }
                }
            };

            request.AddParameter("text/json", JsonConvert.SerializeObject(json), ParameterType.RequestBody);
            var result = client.Execute(request);

            twiml.Say("An email has been sent to the address on file and you should receive it in a few minutes. Returning to the main menu.", voiceAttribute);
            twiml.Redirect("/dialin/index?node=default");

            return twiml;
        }

        public static TwilioResponse ItemByBarcode(this TwilioResponse twiml, string callsid)
        {
            twiml.BeginGather(new { action = "/dialin/RenewItemByBarcode", timeout = 10 });
            twiml.Say("Enter the barcode of the item you wish to renew, followed by the pound key. Or press 7 to go back to the main menu.", voiceAttribute);
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=listItems&digits=4");

            return twiml;
        }

        public static TwilioResponse ListAllItems(this TwilioResponse twiml, string callsid)
        {
            if (CallVars.ItemType[callsid] != ItemType.All)
            {
                CallVars.ItemType[callsid] = ItemType.All;
                using (var db = NoticeEntities.Create())
                {
                    CallVars.ItemsOut[callsid] = db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, null).ToList();
                }
                CallVars.ItemsOutIndex[callsid] = 0;
            }
            // Get the position the patron is at in the list of items, if any.
            CallVars.ItemsOutIndex[callsid] = CallVars.ItemsOutIndex.Keys.Contains(callsid) ? CallVars.ItemsOutIndex[callsid] : 0;

            return twiml.ListItems(callsid);
        }

        public static TwilioResponse ListItems(this TwilioResponse twiml, string callsid)
        {
            var itemsOut = CallVars.ItemsOut[callsid];

            if (itemsOut == null || itemsOut.Count() == 0)
            {
                if (CallVars.ItemType[callsid] == ItemType.Overdue)
                {
                    twiml.Say("You currently have no overdue items", voiceAttribute);
                }
                else
                {
                    twiml.Say("You currently have no items checked out. Returning to main menu.", voiceAttribute);
                }

                twiml.Redirect("/dialin/index?node=default");
                return twiml;
            }

            // If somehow the patron gets past the number of items send them back to the menu.
            if (CallVars.ItemsOutIndex[callsid] + 1 > itemsOut.Count())
            {
                twiml.Say("You have no more items to list, returning to the main menu.", voiceAttribute);
                twiml.Redirect("/dialin/index?node=default");
                CallVars.ItemsOutIndex[callsid] = 0;
                return twiml;
            }

            // Get the information for the current item
            var currentItem = itemsOut.ElementAt(CallVars.ItemsOutIndex[callsid]);
            int itemCount = itemsOut.Count();

            // Tell the patron about the current title.
            twiml.BeginGather(new { action = "/dialin/index?node=renewOptions", numDigits = "1", timeout = "2" });
            twiml.Say( string.Format("Your {0} {1} item is, {2}, due back on {3}",
                itemCount == 1 ? "only" : itemCount == CallVars.ItemsOutIndex[callsid] + 1 ? "final" : IntToString((CallVars.ItemsOutIndex[callsid] + 1)),
                CallVars.ItemType[callsid] == ItemType.Overdue ? "overdue" : "checked out",
                FormatTitle(currentItem.BrowseTitle),
                currentItem.DueDate.ToString("dddd MMMM dd")), voiceAttribute);
            twiml.Pause(1);
            twiml.Say("To renew this item, press 1. To hear the next title, press 2. To return to the main menu, press 7.", voiceAttribute);
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=listItems&digits=" + (CallVars.ItemType[callsid] == ItemType.All ? "1" : "2"));

            return twiml;
        }

        public static TwilioResponse RenewItem(this TwilioResponse twiml, string callsid)
        {
            var patronBarcode = CallVars.PatronBarcodes[callsid];
            var patronPIN = CallVars.PatronPINs[callsid];
            var currentItem = CallVars.ItemsOut[callsid].ElementAt(CallVars.ItemsOutIndex[callsid]);

            if (!CallVars.ItemRenewResults.Keys.Contains(callsid))
            {
                CallVars.ItemRenewResults[callsid] = polaris.ItemRenew(patronBarcode, patronPIN, currentItem.ItemRecordID);
            }

            var renewResult = CallVars.ItemRenewResults[callsid];

            if (renewResult.ItemRenewResult.DueDateRows.Count > 0)
            {
                using (var db = NoticeEntities.Create())
                {
                    CallVars.ItemsOut[callsid].Single(p => p.ItemRecordID == currentItem.ItemRecordID).DueDate = db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, null).Single(p => p.ItemRecordID == currentItem.ItemRecordID).DueDate;
                }

                twiml.Say(string.Format("Your item was successfully renewed. This item is due back on {0}.", renewResult.ItemRenewResult.DueDateRows.First().DueDate.ToString("dddd MMMM dd")), voiceAttribute);
                twiml.Redirect("/dialin/index?node=listItems&digits=" + (CallVars.ItemType[callsid] == ItemType.All ? "1" : "2"));
                CallVars.ItemRenewResults.Remove(callsid);
                return twiml;
            }

            if (renewResult.ItemRenewResult.BlockRows.Count > 0)
            {
                twiml.BeginGather(new { action = "/dialin/index?node=renewOptions", numDigits = 1 });
                twiml.Say(string.Format("Your item was unable to be renewed for the following reason, {0}", renewResult.ItemRenewResult.BlockRows.First().ErrorDesc), voiceAttribute);
                twiml.Say("Press 2 to confirm and return to your list of items out.", voiceAttribute);
                twiml.EndGather();
                twiml.Redirect("/dialin/index?node=renewOptions?digits=1");
                CallVars.ItemRenewResults.Remove(callsid);
                return twiml;
            }

            twiml.BeginGather(new { action = "/dialin/index?node=renewOptions", numDigits = 1 });
            twiml.Say("Your item was unable to be renewed due to an unknown error. If this problem persists please contact your library for assistance.", voiceAttribute);
            twiml.Say("Press 2 to acknowledge and return to your list of items out.");
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=renewOptions?digits=1");

            return twiml;
        }

        public static TwilioResponse NextItem(this TwilioResponse twiml, string callsid)
        {
            CallVars.ItemsOutIndex[callsid]++;
            twiml.Redirect("/dialin/index?node=listItems&digits=" + (CallVars.ItemType[callsid] == ItemType.All ? "1" : "2"));
            return twiml;
        }

        public static TwilioResponse RenewAll(this TwilioResponse twiml, string callsid)
        {
            var itemsOut = new List<GetCheckedOutItems_Result>();
            using (var db = NoticeEntities.Create())
            {
                itemsOut = db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, null).ToList();
            }

            if (itemsOut.Count() == 0)
            {
                twiml.Say("You currently have no items to renew, returning to main menu.", voiceAttribute);
                twiml.Redirect("/dialin/index?node=default");
                return twiml;
            }

            var result = polaris.ItemRenewAllForPatron(CallVars.PatronBarcodes[callsid], CallVars.PatronPINs[callsid]);
            twiml.BeginGather(new { action = "/dialin/index?node=menu", numDigits = "1", timeout = "1" });
            twiml.Say(string.Format("{0} {1} successfully renewed and {2} {3} unable to be renewed.",
                result.ItemRenewResult.DueDateRows.Count,
                result.ItemRenewResult.DueDateRows.Count == 1 ? "item was" : "items were",
                result.ItemRenewResult.BlockRows.Count,
                result.ItemRenewResult.BlockRows.Count == 1 ? "item was" : "items were"),
                voiceAttribute);
            // If there were items that were unable to be renewed. Tell the patron which items and why.
            if (result.ItemRenewResult.BlockRows.Count > 0)
            {
                twiml.Say(string.Format("We were unable to renew the following {0}. ", "item".Pluralize(result.ItemRenewResult.BlockRows.Count)), voiceAttribute);
                foreach (var row in result.ItemRenewResult.BlockRows)
                {
                    var item = itemsOut.Single(t => t.ItemRecordID == row.ItemRecordID);
                    twiml.Say(string.Format("{0} because {1}.", FormatTitle(item.BrowseTitle), row.ErrorDesc), voiceAttribute);                   

                    if (row.ItemRecordID != result.ItemRenewResult.BlockRows.Last().ItemRecordID)
                    {
                        twiml.Pause(1);
                    }
                }
            }
            twiml.EndGather();
            twiml.Redirect("/dialin/index?node=default");

            return twiml;
        }

        static string IntToString(int i)
        {
            string[] unitsAndTeens = { "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth", "Eleventh", "Twelfth", "Thirteenth", "Fourteenth",
				"Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth", "Nineteenth", "Twentieth"};

            if (i <= 20)
            {
                return unitsAndTeens[i - 1];
            }

            return i.ToString();
        }

        public static string FormatTitle(string title)
        {
            title = title.PadRight(100);
            return title.Substring(0, title.IndexOf(' ', 50)).Trim();
        }
	}
}
