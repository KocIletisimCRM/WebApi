using System;
using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKRate
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public double? k_SuccesRate { get; set; }
        public double? k_SLOrt { get; set; }
        public int? k_completed { get; set; }
        public int? k_total { get; set; }
        public double? e_SuccesRate { get; set; }
        public double? e_SLOrt { get; set; }
        [Key]
        public DateTime date { get; set; }
    }
}