using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class Cozum
    {
        [Key]
        public int eleh { get; set; }
        public DateTime sdate { get; set; }
        public DateTime fdate { get; set; }
        public double s { get; set; }
    }
}