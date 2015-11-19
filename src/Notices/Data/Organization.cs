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
    
    public partial class Organization
    {
        public Organization()
        {
            this.Organizations1 = new HashSet<Organization>();
            this.RecordSets = new HashSet<RecordSet>();
            this.Patrons = new HashSet<Patron>();
            this.PatronNotes = new HashSet<PatronNote>();
            this.PatronNotes1 = new HashSet<PatronNote>();
            this.PatronRegistrations = new HashSet<PatronRegistration>();
        }
    
        public int OrganizationID { get; set; }
        public Nullable<int> ParentOrganizationID { get; set; }
        public int OrganizationCodeID { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public int SA_ContactPersonID { get; set; }
        public int CreatorID { get; set; }
        public Nullable<int> ModifierID { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> ModificationDate { get; set; }
        public string DisplayName { get; set; }
    
        public virtual ICollection<Organization> Organizations1 { get; set; }
        public virtual Organization ParentOrganization { get; set; }
        public virtual ICollection<RecordSet> RecordSets { get; set; }
        public virtual ICollection<Patron> Patrons { get; set; }
        public virtual ICollection<PatronNote> PatronNotes { get; set; }
        public virtual ICollection<PatronNote> PatronNotes1 { get; set; }
        public virtual ICollection<PatronRegistration> PatronRegistrations { get; set; }
    }
}