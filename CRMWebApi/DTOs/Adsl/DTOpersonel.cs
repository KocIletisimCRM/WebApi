using System;
using System.Collections.Generic;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOpersonel
    {

        public int personelid { get; set; }
        public string personelname { get; set; }
        public int category { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string notes { get; set; }
        public int roles { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public int updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
        
        public List<object> stockstatus { get; set; }
    }
}