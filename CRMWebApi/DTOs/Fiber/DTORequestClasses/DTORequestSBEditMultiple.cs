using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public class DTORequestSBEditMultiple
    {
        public List<int> siteIds { get; set; }
        public List<int> blockIds { get; set; }
        public int personelid { get; set; }
    }
}