﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PolarisEntities : DbContext
    {
        private PolarisEntities()
            : base("name=PolarisEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<PatronRegistration> PatronRegistrations { get; set; }
        public virtual DbSet<Patron> Patrons { get; set; }
        public virtual DbSet<RecordSet> RecordSets { get; set; }
        public virtual DbSet<PatronNote> PatronNotes { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
    }
}