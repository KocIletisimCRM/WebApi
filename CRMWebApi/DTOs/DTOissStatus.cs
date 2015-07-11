using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOissStatus
    {
        public int id { get; set; }
        public string issText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}