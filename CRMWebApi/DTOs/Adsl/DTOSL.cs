using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOSL
    {
        public int SLID { get; set; }
        public string SLName { get; set; }
        public string KocSTask { get; set; }
        public string KocETask { get; set; }
        public string BayiSTask { get; set; }
        public string BayiETask { get; set; }
        public DateTime lastupdated { get; set; }
        public int updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
    }
}