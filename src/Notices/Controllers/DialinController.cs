using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.TwiML;
using Clc.Polaris.Api;
using System.Text;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using NoticeSuite;
using NoticeSuite.Data;
using NoticeSuite.Models;
using NoticeSuite.Extensions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NoticeSuite.Controllers
{
	public class DialinController : Controller
	{
        string hangupResponse = new TwilioResponse().Hangup().ToString();
		PolarisApiClient polaris = new PolarisApiClient();
        NoticeEntities db;
		object voiceAttribute = new { voice = "alice" };
        private static MyLogger logger = MyLogger.GetLogger();

        // Create the dictionary to hold all of the menu options.
        Dictionary<string, string[]> menu = new Dictionary<string, string[]>
			{				
			    {"default", new string[] {"yourlibrary", "renewall", "listitems", "", "listholds"} },
                {"listItems", new string[] {"yourlibrary", "listallitems", "listoverdueitems", "itembybarcode", "emailitems", "", "", "mainmenu"} },
                {"overdueOptions", new string[] { "yourlibrary", "listoverdueitems", "listallitems","itembybarcode", "emailitems",  "", "", "mainmenu" } },
                {"renewOptions", new string[] { "yourlibrary", "renewItem", "nextItem", "", "", "", "", "mainmenu"} },
                {"holdOptions", new string[] { "yourlibrary", "nextHold", "", "", "", "", "", "mainmenu" } }
			};

		public DialinController()
		{
            db = NoticeEntities.Create();
            var twilio = Methods.CreateTwilioClient();
		}

        async Task updateNotices(string patronBarcode, string callsid)
        {
            await Task.Run(() =>
            {
                var data = polaris.staff_PatronBasicDataGet(patronBarcode).PatronBasicData;
                var notices = Variables.Notifications.Where(n => n.PatronID == data.PatronID).ToList();
                Variables.Notifications.RemoveAll(n => n.PatronID == data.PatronID);

                Variables.TransferredCalls.Add(callsid);

                foreach (var notice in notices)
                {
                    notice.NotificationUpdate(1, callsid);
                }
            });
        }

		public string Index(int? digits, string barcode2, string node = "", string callsid = "", string from = "", bool test = false)
		{
            if (!AppSettings.EnableDialin) return "dialin disabled in config";

            var twiml = new TwilioResponse();

            if (test)
            {
                ProcessBarcode(AppSettings.TestBarcode, callsid);
                ProcessPin(AppSettings.TestPin, callsid);
                LookupAccount(callsid, from);
            }

            if (!string.IsNullOrEmpty(barcode2))
            {
                logger.Info("", callsid, null, barcode2, Actions.TransferFromDialout);
                updateNotices(barcode2, callsid).Forget();
                ProcessBarcode(barcode2, callsid);
            }

            if (HandleTimeout(digits.ToString(), callsid)) return hangupResponse;

			// Checks to see if the list of patron barcodes has an entry for the current callsid and prompts the user if needed
			if (!CallVars.PatronBarcodes.Keys.Contains(callsid))
			{
			    if (db.Dialin_Patrons.Any(p => p.PhoneNumber == from) && !db.Dialin_Ignore_List.Any(p=>p.PhoneNumber == from))
			    {
                    return PromptRememberedPatron(callsid, from);
			    }

                return PromptBarcode(callsid);
			}
			// Checks to see if the list of patron PINs contains an entry for the current callsid and prompts the user if needed

			if (!CallVars.PatronPINs.Keys.Contains(callsid))
			{
                return PromptPIN(callsid);
			}

            var output = HandleMenu(callsid, digits, node);
            return output;
		}

        public string PromptPIN(string callsid)
        {
            var twiml = new TwilioResponse();
            twiml.BeginGather(new { action = "/dialin/processpin", numDigits = AppSettings.PinDigits, timeout = 10 })
                    .Say("Please enter your pin", voiceAttribute);
            twiml.EndGather();
            twiml.Say("Sorry, I didn't get that.", voiceAttribute);
            twiml.Redirect("/dialin/index");
            return twiml.ToString();
        }

        public string PromptBarcode(string callsid)
        {
            var twiml = new TwilioResponse();
            logger.Info("", callsid, null, "", Actions.Dial);
            // Prompts the user to enter their barcode and sends the input to be processed.
            // This adds the barcode to the list of barcodes and redirects the user back to
            // the index page to collect their PIN.

            // If the user does not enter a barcode they will be sent back to the index page 
            // and prompted again for their barcode.
            twiml.BeginGather(new { action = "/dialin/processbarcode" })
                .Say("Hello, thank you for calling the library. Please enter your library account barcode, followed by the pound key", voiceAttribute);
            twiml.EndGather();
            twiml.Say("Sorry, I didn't get that.", voiceAttribute);
            twiml.Redirect("/dialin/index");
            return twiml.ToString();
        }

        public string PromptRememberedPatron(string callsid, string from)
        {
            var twiml = new TwilioResponse();
            var tmpPatron = polaris.staff_PatronBasicDataGet(db.Dialin_Patrons.Single(p => p.PhoneNumber == from).Barcode);

            logger.Info("", callsid, tmpPatron.PatronBasicData.PatronID, tmpPatron.PatronBasicData.Barcode, Actions.RetrievedRememberedPatron);

            if (tmpPatron.PatronBasicData.Barcode != null)
            {
                twiml.BeginGather(new { action = "/dialin/processrememberedpatron", numDigits = 1 });
                twiml.Say(string.Format("Hello. Are you calling about {0}'s library account? Press 1 to confirm or press 2 to enter your library account barcode.", tmpPatron.PatronBasicData.NameFirst), voiceAttribute);
                twiml.EndGather();
            }
            else
            {
                try
                {
                    db.Dialin_Patrons.Remove(db.Dialin_Patrons.Single(p => p.PhoneNumber == from));
                    db.SaveChanges();
                }
                catch
                {
                }
                CallVars.PatronBarcodes.Remove(callsid);
                CallVars.PatronPINs.Remove(callsid);
                twiml.Redirect("/dialin/index");
            }
            return twiml.ToString();
        }

        public string HandleMenu(string callsid, int? digits, string node="")
        {
            var twiml = new TwilioResponse();
            // If the user pressed a key at one of the menus then use that input to determine where 
            // they need to go
            int index = digits ?? 0;

            // Create the variable to determine where the user is going to end up
            string destination = "";

            // Checks to see if the node specified in the URL is a valid menu node
            if (menu.Keys.Contains(node))
            {
                // Send the user to the proper menu option, based on the number they pressed.

                // CHANGED FROM: if (menu[node].Length >= index && digits.HasValue)
                //if (menu[node].Length > index)
                if (menu[node].Length + 1 >= index && digits.HasValue)
                {
                    if (menu[node].Length + 1 > digits.Value)
                    {
                        destination = menu[node][digits.Value];
                    }
                    else
                    {
                        destination = "mainmenu";
                    }
                }
            }

            // Put the items out and holds into local variables to make them easier to use.
            //var itemsOut = CallVars.ItemsOut[callsid];
            //var holds = CallVars.Holds[callsid];
            var _patron = CallVars.PatronData[callsid];

            // Send the patron where they need to go.
            var w = Stopwatch.StartNew();
            switch (destination)
            {
                // Connect the patron to their library.
                case "yourlibrary":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.TransferredToLibrary);
                    twiml.Say("Connecting you to your library.", voiceAttribute);
                    twiml.Dial(polaris.SA_GetValueByOrg("ORGPHONE1", CallVars.OtherPatronInfo[callsid].AssignedBranchID).Value);
                    break;
                case "listallitems":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.ListAllItems);
                    twiml.ListAllItems(callsid);
                    break;
                case "listitems":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.ListItems);
                    twiml.BuildItemListMenu(callsid);
                    break;
                case "listoverdueitems":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.ListOverdueItems);
                    twiml.ListOverdueItems(callsid);
                    break;
                case "emailitems":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.EmailItems);
                    twiml.EmailTitles(callsid);
                    break;
                case "itembybarcode":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.ItemByBarcode);
                    twiml.ItemByBarcode(callsid);
                    break;
                case "renewall":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.RenewAll);
                    twiml.RenewAll(callsid);
                    break;
                case "listholds":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.ListHolds);
                    twiml.ListHolds(callsid);
                    break;
                case "nextHold":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.NextHold);
                    twiml.NextHold(callsid);
                    break;
                case "cancelHold":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.CancelHold);
                    twiml.CancelHold(callsid);
                    break;
                case "confirmCancel":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.ConfirmCancel);
                    twiml.ConfirmCancel(callsid);
                    break;
                case "renewItem":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.RenewIndividual);
                    twiml.RenewItem(callsid);
                    break;
                case "nextItem":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.NextItem);
                    twiml.NextItem(callsid);
                    break;
                case "mainmenu":
                    logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.MainMenu);
                    twiml.BuildMainMenu(callsid);
                    break;
                default:
                    CallVars.ItemRenewResults.Remove(callsid);
                    if (CallVars.SkipIntro.Keys.Contains(callsid))
                    {
                        if (HandleTimeout(digits.ToString(), callsid))
                        {
                            logger.Info("", callsid, _patron.PatronID, _patron.Barcode, Actions.Timeout);
                            return hangupResponse;
                        }
                    }
                    //if (HandleTimeout(digits.ToString(), callsid)) return hangupResponse;
                    switch (node)
                    {
                        case "default":
                            twiml.BuildMainMenu(callsid);
                            break;
                        case "listItems":
                            twiml.BuildItemListMenu(callsid);
                            break;
                        case "holdOptions":
                            twiml.ListHolds(callsid);
                            break;
                        default:
                            twiml.BuildMainMenu(callsid);
                            break;
                    }
                    break;
            }
            w.Stop();
            Debug.WriteLine("Menu: {0}ms", w.ElapsedMilliseconds);
            return twiml.ToString();
        }

		public string ProcessBarcode(string digits, string callsid)
		{
            if (HandleTimeout(digits, callsid)) return hangupResponse;
            if (AppSettings.EnableItivaTransfer)
            {
                if (!CallVars.TryItiva.Keys.Contains(callsid) && digits.Length == 10)
                {
                    var dpPatronCheckData = polaris.staff_PatronValidate(digits);
                    var tempOrg = db.PolarisOrganizations.SingleOrDefault(o => o.OrganizationID == dpPatronCheckData.AssignedBranchID);

                    if (tempOrg != null)
                    {
                        if (new int[] { 32, 39 }.Contains(tempOrg.ParentOrganizationID ?? 0))
                        {
                            logger.Info("", callsid, dpPatronCheckData.PatronID, "", Actions.TransferToItiva);
                            //Transfer them to the iTiva system
                            return new TwilioResponse().Dial(AppSettings.ItivaNumber).ToString();
                        }
                    }

                    CallVars.TryItiva[callsid] = false;
                }
            }
			CallVars.PatronBarcodes[callsid] = digits;            
			return new TwilioResponse().Redirect("/dialin/index").ToString();
		}

		public string ProcessPin(string digits, string callsid)
		{
            if (HandleTimeout(digits, callsid)) return hangupResponse;
            logger.Info("PIN Entered: " + digits, callsid, null, CallVars.PatronBarcodes[callsid], Actions.PinEntered);
			CallVars.PatronPINs[callsid] = digits;
            return new TwilioResponse()
                .Say("One moment while I look up your account.", voiceAttribute)
                .Redirect("/dialin/lookupaccount").ToString();
		}

        public string LookupAccount(string callsid, string from)
        {
            var twiml = new TwilioResponse();
            // Grabs the patron barcode for the current call
            var barcode = CallVars.PatronBarcodes[callsid];
            // Grabs the patron PIN for the current call
            var pin = CallVars.PatronPINs[callsid];

            
            // Gets the patron information for the patron on the current call
            var patron = polaris.PatronBasicDataGet(barcode, pin);

            // Get the patron's registered branch because Polaris is dumb and doesn't
            // include it in PatronBasicDataGet
            var otherPatronInfo = polaris.PatronValidate(barcode, pin);

            // Look and see if we were able to successfully get the information for the specified
            // patron information.
            if (patron.PatronBasicData == null)
            {
                // If we were unable to get a patron from Polaris with the specified barcode and PIN
                // then remove the barcode and PIN for the current call
                CallVars.PatronBarcodes.Remove(callsid);
                CallVars.PatronPINs.Remove(callsid);

                logger.Info("PIN = " + pin, callsid, null, barcode, Actions.IncorrectInfoEntered);

                // Send the patron back to the index to be prompted for their barcode and PIN again
                twiml.Say("Incorrect barcode or pin", voiceAttribute);
                twiml.Redirect("/dialin/index");
                return twiml.ToString();
            }

            SetRememberedPatron(barcode, from);

            // Populate call variables to hold patron info
            var w = Stopwatch.StartNew();
            CallVars.PatronData[callsid] = patron.PatronBasicData;
            CallVars.OtherPatronInfo[callsid] = otherPatronInfo;
            w.Stop();
            // Set up some initial variable values
            //CallVars.ItemsOut[callsid] = polaris.PatronItemsOutGetAll(barcode, pin);
            var w2 = Stopwatch.StartNew();
            CallVars.ItemsOut[callsid] = db.GetCheckedOutItems(patron.PatronBasicData.PatronID, 0).ToList();
            w2.Stop();

            Debug.WriteLine(String.Format("Patron info: {0}ms | Items out: {1}ms", w.ElapsedMilliseconds, w2.ElapsedMilliseconds));
            CallVars.ItemType[callsid] = ItemType.All;

            return new TwilioResponse().Redirect("/dialin/index").ToString();
        }

        void SetRememberedPatron(string barcode, string phonenumber)
        {
            if (!string.IsNullOrWhiteSpace(phonenumber) || !db.Dialin_Patrons.Any(p => p.PhoneNumber == phonenumber))
            {
                db.Dialin_Patrons.Where(p => p.PhoneNumber == phonenumber).ToList().ForEach(p => db.Dialin_Patrons.Remove(p));
                db.Dialin_Patrons.Add(new Dialin_Patrons { Barcode = barcode, PhoneNumber = phonenumber });
                db.SaveChanges();
            }
        }

		public string RenewItemByBarcode(string digits, string callsid)
		{
            var twiml = new TwilioResponse();

            var itemBarcode = digits;

            var item = db.GetCheckedOutItems(CallVars.PatronData[callsid].PatronID, null).SingleOrDefault(i => i.Barcode == itemBarcode);
            

            if (item == null)
            {
                twiml.Say("The barcode you entered is not valid. Please try again.", voiceAttribute);
                twiml.Redirect("/dialin/index?node=listItems&digits=3");
                return twiml.ToString();
            }

            var result = polaris.ItemRenew(CallVars.PatronBarcodes[callsid], CallVars.PatronPINs[callsid], item.ItemRecordID);

            if (result.PAPIErrorCode == 0)
            {
                twiml.Say("Successfully renewed " + TwilioResponseExtension.FormatTitle(item.BrowseTitle) + ". The new due date is " + result.ItemRenewResult.DueDateRows.Single().DueDate.ToString("dddd MMMM dd"), voiceAttribute);
                twiml.Redirect("/dialin/index?node=listItems");
                return twiml.ToString();
            }

            var error = "";

            if (result.ItemRenewResult.BlockRows.Count > 1)
            {
                result.ItemRenewResult.BlockRows.ForEach(b => error += (b.ErrorDesc + ", "));                
            }
            error = result.ItemRenewResult.BlockRows.Single().ErrorDesc;
            twiml.Say("Unable to renew " + TwilioResponseExtension.FormatTitle(item.BrowseTitle) + " for the following reason.", voiceAttribute);
            twiml.Say(error, voiceAttribute);
            twiml.Redirect("/dialin/index?node=listItems");


            return twiml.ToString();
		}

        public string ProcessRememberedPatron(string digits, string callsid, string from)
        {
            if (HandleTimeout(digits, callsid)) return new TwilioResponse().Hangup().ToString();
            
            if (digits == "1")
            {
                CallVars.PatronBarcodes[callsid] = db.Dialin_Patrons.Single(p => p.PhoneNumber == from).Barcode;
            }
            else
            {                
                db.Dialin_Patrons.Remove(db.Dialin_Patrons.Single(p => p.PhoneNumber == from));
                db.SaveChanges();
            }          

            return new TwilioResponse().Redirect("/dialin/index").ToString();
        }

        public bool HandleTimeout(string digits, string callsid)
        {
            // If the user doesn't have an entry in the timeout dictionary add one.
            if (!CallVars.Timeout.Keys.Contains(callsid))
            {
                CallVars.Timeout.Add(callsid, -1);
            }

            // If the user didn't enter a value then increment their value
            if (string.IsNullOrWhiteSpace(digits))
            {
                CallVars.Timeout[callsid]++;
                // If the user is at or over the limit then hang up the call.
                if (CallVars.Timeout[callsid] >= AppSettings.TimeoutLimit)
                {
                    return true;
                }
                return false;
            }
            // If the user did enter something then reset their value
            else
            {
                CallVars.Timeout.Remove(callsid);
                return false;
            }            
        }        
	}
}
