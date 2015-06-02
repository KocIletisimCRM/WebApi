using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class CustomerTransferObject
    {
        public int customerid { get; set; }
        public int blockid { get; set; }
        public string flat { get; set; }
        public IEnumerable<CustomerTransferObject> CustomerTransfer { get; set; }
    }
}