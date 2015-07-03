using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOtelStatus
    {
        public int id { get; set; }
        public string telText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}