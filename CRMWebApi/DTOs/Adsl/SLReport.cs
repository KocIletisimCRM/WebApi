using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SLReport
    {
        public DTOtaskqueue s_tq;
        public DTOtaskqueue kr_tq;
        public DTOtaskqueue k_tq;
        public DTOtaskqueue ktk_tq;
        public DTOtaskqueue lasttq;
        public string lastTaskStatus;
        public string lastTaskTypeName;
        //public string lastTaskName;
    }
}