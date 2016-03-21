using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SLTime
    {
        public DateTime? KStart { get; set; }
        public DateTime? KEnd { get; set; }
        public DateTime? BStart { get; set; }
        public DateTime? BEnd { get; set; }
        public int? BayiID { get; set; }
    }
}