//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRMWebApi.Models.Fiber
{
    using System;
    using System.Collections.Generic;
    
    public partial class product_service
    {
        public product_service()
        {
            this.customerproduct = new HashSet<customerproduct>();
        }
    
        public int productid { get; set; }
        public string productname { get; set; }
        public string category { get; set; }
        public string automandatorytasks { get; set; }
        public Nullable<int> maxduration { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
        public string documents { get; set; }
    
        public virtual ICollection<customerproduct> customerproduct { get; set; }
    }
}
