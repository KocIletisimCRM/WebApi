using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOTaskqueueLast : DTOtaskqueue
    {
        public string laststatus { get; set; }
    }
}