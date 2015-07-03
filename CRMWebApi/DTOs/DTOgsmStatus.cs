using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOgsmStatus
    {
        public int id { get; set; }
        public string gsmText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}