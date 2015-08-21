using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public class DTOFilterGetSitesRequest :DTOFilterRequestBase
    {
        public DTOFieldFilter siteName { get; set; }
        public DTOFieldFilter region { get; set; }
    }
}