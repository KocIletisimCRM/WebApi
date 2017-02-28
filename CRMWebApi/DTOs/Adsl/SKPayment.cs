using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKPayment
    { // Hakedişler hesaplanarak döndürülecek olan class
        public double score { get; set; }
        public int sat { get; set; }
        public int sat_kur { get; set; }
        public int kur { get; set; }
        public int ariza { get; set; }
        public int teslimat { get; set; }
        public int evrak { get; set; }
        public int d_sat { get; set; } // donanım satışı
        public int d_sat_tes { get; set; } // donanım sat-tes
    }
}