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
    
    public partial class customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public customer()
        {
            this.taskqueue = new HashSet<adsl_taskqueue>();
            this.adsl_stockmovement = new HashSet<adsl_stockmovement>();
            this.adsl_stockmovement1 = new HashSet<adsl_stockmovement>();
        }
    
        public int customerid { get; set; }
        public string customername { get; set; }
        public string tc { get; set; }
        public string gsm { get; set; }
        public string phone { get; set; }
        public Nullable<int> ilKimlikNo { get; set; }
        public Nullable<int> ilceKimlikNo { get; set; }
        public Nullable<int> mahalleKimlikNo { get; set; }
        public Nullable<int> yolKimlikNo { get; set; }
        public Nullable<int> binaKimlikNo { get; set; }
        public Nullable<int> daire { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public string description { get; set; }
        public Nullable<bool> deleted { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> birthdate { get; set; }
        public Nullable<int> customerstatus { get; set; }
        public Nullable<int> bucakKimlikNo { get; set; }
        public string email { get; set; }
        public string superonlineCustNo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<adsl_taskqueue> taskqueue { get; set; }
        public virtual il il { get; set; }
        public virtual ilce ilce { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<adsl_stockmovement> adsl_stockmovement { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<adsl_stockmovement> adsl_stockmovement1 { get; set; }
    }
}
