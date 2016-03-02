using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public class DTOFilterGetCustomerRequest
    {
        public DTOFieldFilter customerid { get; set; }
        public DTOFieldFilter block { get; set; }
        public DTOFieldFilter flatNo { get; set; }
        public DTOFieldFilter siteid { get; set; }
        public DTOFieldFilter deleted { get; set; }
        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("customer", "customerid");
            if (customerid != null) filter.fieldFilters.Add(customerid);
            if (block != null) filter.fieldFilters.Add(block);
            if (flatNo != null) filter.fieldFilters.Add(flatNo);
            if (siteid != null) filter.fieldFilters.Add(siteid);
            if (deleted != null) filter.fieldFilters.Add(deleted);
            return filter;
        }
    }
}