using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public class DTOFilterGetBlockRequest:DTOFilterRequestBase
    {
        public DTOFieldFilter obek { get; set; }
        public DTOFieldFilter site { get; set; }
        public DTOFieldFilter telocordiaid { get; set; }
        public DTOFieldFilter sofiberbaslangic { get; set; }
    }
}