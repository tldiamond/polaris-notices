using NoticeSuite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoticeSuite.Extensions
{
    public static class PatronExtension
    {
        public static Patron AddNote(this Patron p, string noteText)
        {
            if (p.PatronNote == null)
            {
                p.PatronNote = new PatronNote { NonBlockingStatusNotes = noteText };
            }
            else
            {
                p.PatronNote.NonBlockingStatusNotes += "";

                if (p.PatronNote.NonBlockingStatusNotes.Length < 3900)
                {
                    p.PatronNote.NonBlockingStatusNotes += noteText;
                }
            }

            return p;
        }

        public static Patron RemoveEmail(this Patron p, string email)
        {
            if (String.Equals(p.PatronRegistration.EmailAddress, email, StringComparison.OrdinalIgnoreCase))
            {
                p.PatronRegistration.EmailAddress = "";
            }

            if (String.Equals(p.PatronRegistration.AltEmailAddress, email, StringComparison.OrdinalIgnoreCase))
            {
                p.PatronRegistration.AltEmailAddress = "";
            }

            if (String.IsNullOrWhiteSpace(p.PatronRegistration.EmailAddress) && String.IsNullOrWhiteSpace(p.PatronRegistration.AltEmailAddress))
            {
                p.PatronRegistration.DeliveryOptionID = string.IsNullOrWhiteSpace(p.PatronRegistration.PhoneVoice1) ? 1 : 3;
            }
            return p;
        }
    }
}