using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Cari
{
    public class CariHareketReport
    {
        [Key]
        public int periodId { get; set; }
        public int personelid { get; set; }
        public string personelname { get; set; }
        public string period { get; set; }
        public decimal alacak { get; set; }
        public decimal odenen { get; set; }
    }
}