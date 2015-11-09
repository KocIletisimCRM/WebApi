using System;
using System.Collections.Generic;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOpersonel
    {

        public int personelid { get; set; }
        public string personelname { get; set; }
        public Nullable<long> category { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string notes { get; set; }
        public Nullable<long> roles { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<long> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
        
        public List<object> stockstatus { get; set; }
    }
}