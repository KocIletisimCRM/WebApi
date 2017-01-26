using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class stocksayim
    {
        [Key]
        public int movementid { get; set; }
        public int id { get; set; }
        public string ad { get; set; }
        public string seri { get; set; }
        public string tur { get; set; }
        public string stock { get; set; }
    }
}