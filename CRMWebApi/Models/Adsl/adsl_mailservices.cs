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
    
    public partial class adsl_mailservices
    {
        public int id { get; set; }
        public string ServiceName { get; set; }
        public string Extention { get; set; }
        public string Address { get; set; }
        public Nullable<int> Port { get; set; }
        public Nullable<bool> UseSSL { get; set; }
    }
}