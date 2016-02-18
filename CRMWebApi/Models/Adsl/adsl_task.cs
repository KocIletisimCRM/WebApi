//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRMWebApi.Models.Adsl
{
    using System;
    using System.Collections.Generic;
    
    public partial class adsl_task
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public adsl_task()
        {
            this.taskqueue = new HashSet<adsl_taskqueue>();
            this.taskstatematches = new HashSet<adsl_taskstatematches>();
            this.atama = new HashSet<atama>();
            this.atama1 = new HashSet<atama>();
        }
    
        public int taskid { get; set; }
        public string taskname { get; set; }
        public int tasktype { get; set; }
        public Nullable<int> attachablepersoneltype { get; set; }
        public Nullable<int> attachableobjecttype { get; set; }
        public Nullable<double> performancescore { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<adsl_taskqueue> taskqueue { get; set; }
        public virtual adsl_objecttypes personeltypes { get; set; }
        public virtual adsl_objecttypes objecttypes { get; set; }
        public virtual adsl_tasktypes tasktypes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<adsl_taskstatematches> taskstatematches { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<atama> atama { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<atama> atama1 { get; set; }
    }
}
