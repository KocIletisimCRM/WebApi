using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class StockMovementBackSeri
    {
        [Key]
        public int customerid { get; set; }
        public string customername { get; set; }
        public string superonlineno { get; set; }
        public int taskNo { get; set; }
        public string task { get; set; }
        public int personelid { get; set; }
        public string personelname { get; set; }
        public string seri { get; set; }
    }
}