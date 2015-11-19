//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoticeSuite.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Patron
    {
        public Patron()
        {
            this.RecordSets = new HashSet<RecordSet>();
        }
    
        public int PatronID { get; set; }
        public int PatronCodeID { get; set; }
        public int OrganizationID { get; set; }
        public int CreatorID { get; set; }
        public Nullable<int> ModifierID { get; set; }
        public string Barcode { get; set; }
        public int SystemBlocks { get; set; }
        public int YTDCircCount { get; set; }
        public int LifetimeCircCount { get; set; }
        public Nullable<System.DateTime> LastActivityDate { get; set; }
        public Nullable<int> ClaimCount { get; set; }
        public Nullable<int> LostItemCount { get; set; }
        public decimal ChargesAmount { get; set; }
        public decimal CreditsAmount { get; set; }
        public int RecordStatusID { get; set; }
        public System.DateTime RecordStatusDate { get; set; }
    
        public virtual PatronRegistration PatronRegistration { get; set; }
        public virtual ICollection<RecordSet> RecordSets { get; set; }
        public virtual PatronNote PatronNote { get; set; }
        public virtual Organization Organization { get; set; }
    }
}