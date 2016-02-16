using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOBasvuru
    {
        public int basvuruid { get; set; }
        public string adsoyad { get; set; }
        public string gsm { get; set; }
        public string mail { get; set; }
        public string adres { get; set; }
        public bool called { get; set; }
        public DateTime createtime { get; set; }
        public DateTime calledtime { get; set; }
    }
}