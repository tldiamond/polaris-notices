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
    
    public partial class Dialout_String_Types
    {
        public Dialout_String_Types()
        {
            this.Dialout_Strings = new HashSet<Dialout_Strings>();
        }
    
        public int StringTypeID { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Dialout_Strings> Dialout_Strings { get; set; }
    }
}
