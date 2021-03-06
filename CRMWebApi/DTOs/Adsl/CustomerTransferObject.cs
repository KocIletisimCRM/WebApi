﻿using System.Collections.Generic;

namespace CRMWebApi.DTOs.Adsl
{
    public class CustomerTransferObject
    {
        public int customerid { get; set; }
        public int blockid { get; set; }
        public string flat { get; set; }
        public IEnumerable<CustomerTransferObject> CustomerTransfer { get; set; }
    }
}