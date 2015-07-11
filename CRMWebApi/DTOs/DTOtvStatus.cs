using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOtvStatus
    {
        public int id { get; set; }
        public string tvKullanımıText { get; set; }
        public Nullable<int> deleted { get; set; }
    
    }
}