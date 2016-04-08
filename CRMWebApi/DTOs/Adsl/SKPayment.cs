using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKPayment
    { // Hakedişler hesaplanarak döndürülecek olan class
        public int sat { get; set; }
        public int sat_kur { get; set; }
        public int kur { get; set; }
        public int ariza { get; set; }
        public int teslimat { get; set; }
        public int evrak { get; set; }
    }
}