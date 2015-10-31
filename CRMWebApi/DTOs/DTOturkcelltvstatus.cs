using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOturkcelltvstatus
    {
        public int id { get; set; }
        public string TurkcellTvText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}