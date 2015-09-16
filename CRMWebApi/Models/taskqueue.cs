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
    
    public partial class taskqueue
    {
        public taskqueue()
        {
            this.customerproduct = new HashSet<customerproduct>();
            this.taskqueue1 = new HashSet<taskqueue>();
        }
    
        public int taskorderno { get; set; }
        public int taskid { get; set; }
        public Nullable<int> previoustaskorderid { get; set; }
        public Nullable<int> relatedtaskorderid { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<int> attachedobjectid { get; set; }
        public Nullable<System.DateTime> attachmentdate { get; set; }
        public Nullable<int> attachedpersonelid { get; set; }
        public Nullable<System.DateTime> appointmentdate { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<System.DateTime> consummationdate { get; set; }
        public string description { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
        public Nullable<int> assistant_personel { get; set; }
        public string fault { get; set; }
        public int taskqueue_taskorderno { get; set; }
    
        public virtual customer attachedcustomer { get; set; }
        public virtual block attachedblock { get; set; }
        public virtual taskstatepool taskstatepool { get; set; }
        public virtual personel attachedpersonel { get; set; }
        public virtual task task { get; set; }
        public virtual personel asistanPersonel { get; set; }
        public virtual personel Updatedpersonel { get; set; }
        public virtual ICollection<customerproduct> customerproduct { get; set; }
        public virtual ICollection<taskqueue> taskqueue1 { get; set; }
        public virtual taskqueue relatedTaskQueue { get; set; }
    }
}
