﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRMWebApi.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class CRMEntities : DbContext
    {
        public CRMEntities()
            : base("name=CRMEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<task> task { get; set; }
        public virtual DbSet<block> block { get; set; }
        public virtual DbSet<site> site { get; set; }
        public virtual DbSet<taskqueue> taskqueue { get; set; }
        public virtual DbSet<taskstatepool> taskstatepool { get; set; }
        public virtual DbSet<customer> customer { get; set; }
        public virtual DbSet<personel> personel { get; set; }
        public virtual DbSet<customer_status> customer_status { get; set; }
    
        [DbFunction("CRMEntities", "sf_taskqueue")]
        public virtual IQueryable<taskqueue> sf_taskqueue(Nullable<int> pageNo, Nullable<int> rowsPerPage)
        {
            var pageNoParameter = pageNo.HasValue ?
                new ObjectParameter("pageNo", pageNo) :
                new ObjectParameter("pageNo", typeof(int));
    
            var rowsPerPageParameter = rowsPerPage.HasValue ?
                new ObjectParameter("rowsPerPage", rowsPerPage) :
                new ObjectParameter("rowsPerPage", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<taskqueue>("[CRMEntities].[sf_taskqueue](@pageNo, @rowsPerPage)", pageNoParameter, rowsPerPageParameter);
        }
    }
}
