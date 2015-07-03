using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOnetStatus
    {
        public int id { get; set; }
        public string netText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}