using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOcustomer_status
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}