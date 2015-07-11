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
    
    public partial class customer
    {
        public customer()
        {
            this.taskqueues = new HashSet<taskqueue>();
        }
    
        public int customerid { get; set; }
        public Nullable<int> blockid { get; set; }
        public string tckimlikno { get; set; }
        public string customername { get; set; }
        public string customersurname { get; set; }
        public string flat { get; set; }
        public string gsm { get; set; }
        public string phone { get; set; }
        public Nullable<System.DateTime> birthdate { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
        public Nullable<int> customerstatus { get; set; }
        public Nullable<int> telstatu { get; set; }
        public string turkcellTv { get; set; }
        public Nullable<int> netstatu { get; set; }
        public string description { get; set; }
        public Nullable<int> tvstatu { get; set; }
        public Nullable<int> gsmstatu { get; set; }
        public Nullable<int> iss { get; set; }
        public Nullable<int> emptorcustomernum { get; set; }
    
        public virtual customer_status customer_status { get; set; }
        public virtual ICollection<taskqueue> taskqueues { get; set; }
        public virtual block block { get; set; }
        public virtual personel updatedpersonel { get; set; }
        public virtual issStatus issStatus { get; set; }
        public virtual netStatus netStatus { get; set; }
        public virtual TvKullanımıStatus TvKullanımıStatus { get; set; }
        public virtual telStatus telStatus { get; set; }
        public virtual gsmKullanımıStatus gsmKullanımıStatus { get; set; }
    }
}
