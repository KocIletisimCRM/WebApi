//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class taskstatepool
    {
        public taskstatepool()
        {
            this.taskqueues = new HashSet<taskqueue>();
            this.taskstatematches = new HashSet<taskstatematches>();
        }
    
        public int taskstateid { get; set; }
        public string taskstate { get; set; }
        public Nullable<int> statetype { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
    
        public virtual ICollection<taskqueue> taskqueues { get; set; }
        public virtual personel updatedpersonel { get; set; }
        public virtual ICollection<taskstatematches> taskstatematches { get; set; }
    }
}
