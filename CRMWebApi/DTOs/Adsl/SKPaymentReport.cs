﻿using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKPaymentReport
    {
        [Key]
        public int bId { get; set; }
        public string bName { get; set; }
        public string il { get; set; }
        public double bSLOrt { get; set; }
        public double skor { get; set; }
        public int satAdet { get; set; }
        public double sat { get; set; }
        public int stkrAdet { get; set; }
        public double sat_kur { get; set; }
        public int kurAdet { get; set; }
        public double kur { get; set; }
        public int arzAdet { get; set; }
        public double ariza { get; set; }
        public int tesAdet { get; set; }
        public double teslimat { get; set; }
        public int evrAdet { get; set; }
        public int evrV2Adet { get; set; }
        public double evrak { get; set; }
        public int doSatAdet { get; set; }
        public double doSat { get; set; }
        public int doSat_tesAdet { get; set; }
        public double doSat_tes { get; set; }
        public double toplam
        {
            get
            {
                return sat + sat_kur + kur + ariza + teslimat + evrak + doSat + doSat_tes;
            }
            set { }
        }
    }
}