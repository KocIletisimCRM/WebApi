using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public class DTORequestPagination
    {
        public int pageNo { get; set; }
        public int rowsPerPage { get; set; }
    }
}