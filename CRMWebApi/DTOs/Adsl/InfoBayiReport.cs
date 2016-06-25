using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class InfoBayiReport
    {
        [Key]
        public int personelid { get; set; }
        public string personelname { get; set; }
        public string il { get; set; }
        public string ilce { get; set; }
        public int? worksay { get; set; }
        public int? modemsay { get; set; }
        public string email { get; set; }
    }
}