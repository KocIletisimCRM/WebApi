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
        public string gsm { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string tckimlik { get; set; }
        public string telefon { get; set; }
        public string adres { get; set; }
        public string il { get; set; }
        public string ilce { get; set; }
        public string mahalle { get; set; }

    }
}