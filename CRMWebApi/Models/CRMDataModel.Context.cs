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
            this.Configuration.LazyLoadingEnabled = false;
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
        public virtual DbSet<tasktypes> tasktypes { get; set; }
        public virtual DbSet<customerinfo> customerinfo { get; set; }
    
        [DbFunction("CRMEntities", "sf_taskqueue")]
        public virtual IQueryable<taskqueue> sf_taskqueue(Nullable<int> pageNo, Nullable<int> rowsPerPage, string taskFilter, string attachedobjectFilter, string personelFilter, string taskstateFilter, Nullable<System.DateTime> attachmentdate, Nullable<System.DateTime> creationdate, Nullable<System.DateTime> consummationdate)
        {
            var pageNoParameter = pageNo.HasValue ?
                new ObjectParameter("pageNo", pageNo) :
                new ObjectParameter("pageNo", typeof(int));
    
            var rowsPerPageParameter = rowsPerPage.HasValue ?
                new ObjectParameter("rowsPerPage", rowsPerPage) :
                new ObjectParameter("rowsPerPage", typeof(int));
    
            var taskFilterParameter = taskFilter != null ?
                new ObjectParameter("taskFilter", taskFilter) :
                new ObjectParameter("taskFilter", typeof(string));
    
            var attachedobjectFilterParameter = attachedobjectFilter != null ?
                new ObjectParameter("attachedobjectFilter", attachedobjectFilter) :
                new ObjectParameter("attachedobjectFilter", typeof(string));
    
            var personelFilterParameter = personelFilter != null ?
                new ObjectParameter("personelFilter", personelFilter) :
                new ObjectParameter("personelFilter", typeof(string));
    
            var taskstateFilterParameter = taskstateFilter != null ?
                new ObjectParameter("taskstateFilter", taskstateFilter) :
                new ObjectParameter("taskstateFilter", typeof(string));
    
            var attachmentdateParameter = attachmentdate.HasValue ?
                new ObjectParameter("attachmentdate", attachmentdate) :
                new ObjectParameter("attachmentdate", typeof(System.DateTime));
    
            var creationdateParameter = creationdate.HasValue ?
                new ObjectParameter("creationdate", creationdate) :
                new ObjectParameter("creationdate", typeof(System.DateTime));
    
            var consummationdateParameter = consummationdate.HasValue ?
                new ObjectParameter("consummationdate", consummationdate) :
                new ObjectParameter("consummationdate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<taskqueue>("[CRMEntities].[sf_taskqueue](@pageNo, @rowsPerPage, @taskFilter, @attachedobjectFilter, @personelFilter, @taskstateFilter, @attachmentdate, @creationdate, @consummationdate)", pageNoParameter, rowsPerPageParameter, taskFilterParameter, attachedobjectFilterParameter, personelFilterParameter, taskstateFilterParameter, attachmentdateParameter, creationdateParameter, consummationdateParameter);
        }
    
        [DbFunction("CRMEntities", "sf_task")]
        public virtual IQueryable<task> sf_task(Nullable<int> pageNo, Nullable<int> rowsPerPage, string taskFilter)
        {
            var pageNoParameter = pageNo.HasValue ?
                new ObjectParameter("pageNo", pageNo) :
                new ObjectParameter("pageNo", typeof(int));
    
            var rowsPerPageParameter = rowsPerPage.HasValue ?
                new ObjectParameter("rowsPerPage", rowsPerPage) :
                new ObjectParameter("rowsPerPage", typeof(int));
    
            var taskFilterParameter = taskFilter != null ?
                new ObjectParameter("taskFilter", taskFilter) :
                new ObjectParameter("taskFilter", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<task>("[CRMEntities].[sf_task](@pageNo, @rowsPerPage, @taskFilter)", pageNoParameter, rowsPerPageParameter, taskFilterParameter);
        }
    }
}
