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
    
    public partial class customer_status
    {
        public customer_status()
        {
            this.customers = new HashSet<customer>();
        }
    
        public int ID { get; set; }
        public string Text { get; set; }
        public Nullable<int> deleted { get; set; }
    
        public virtual ICollection<customer> customers { get; set; }
    }
}
