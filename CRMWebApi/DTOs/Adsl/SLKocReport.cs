using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SLKocReport : SLBayiReport
    {
        public DateTime KocSLStart { get; set; }

        public DateTime? kocSLEnd = DateTime.Now;
        public DateTime? KocSLEnd {
            get { return kocSLEnd; }
            set { if (value != null) kocSLEnd = value; }
        }
        public TimeSpan KocSLTime { get { return KocSLEnd.Value - KocSLStart; } } //SLEnd - SLStart
        public int KocSLMaxTime { get; set; } //bitirilmesi gereken azami süre
    }
}