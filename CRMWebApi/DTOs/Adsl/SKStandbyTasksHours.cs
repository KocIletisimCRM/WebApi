using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKStandbyTasksHours: SKStandbyTaskReport
    {
        [Key]
        public int taskno { get; set; }
        public int? kyear { get; set; }
        public int? kmonth { get; set; }
        public int? kday { get; set; }
        public string ktime { get; set; }
        public DateTime? kconsummationtime { get; set; }
        public int? ktkyear { get; set; }
        public int? ktkmonth { get; set; }
        public int? ktkday { get; set; }
        public string ktktime { get; set; }
        public DateTime? ktkconsummationtime { get; set; }
    }
}