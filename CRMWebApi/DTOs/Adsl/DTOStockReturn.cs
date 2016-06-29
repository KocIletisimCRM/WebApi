using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOStockReturn
    {
        public int stockid { get; set; }
        public string stockname { get; set; }
        public int personelid { get; set; }
        public string serials { get; set; }
    }
}