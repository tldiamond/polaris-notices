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
    
    public partial class PolarisNotification
    {
        public int NotificationTypeID { get; set; }
        public int PatronID { get; set; }
        public string PatronBarcode { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public int ItemRecordID { get; set; }
        public int PatronBranch { get; set; }
        public string PatronBranchAbbr { get; set; }
        public Nullable<int> PatronLibrary { get; set; }
        public string PatronLibraryAbbr { get; set; }
        public int DeliveryOptionID { get; set; }
        public string DeliveryString { get; set; }
        public int ReportingBranchID { get; set; }
        public string ReportingBranchAbbr { get; set; }
        public int ReportingLibraryID { get; set; }
        public Nullable<System.DateTime> HoldTillDate { get; set; }
    }
}
