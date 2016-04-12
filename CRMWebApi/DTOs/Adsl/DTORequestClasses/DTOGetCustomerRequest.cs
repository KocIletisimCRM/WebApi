using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
   public class DTOGetCustomerRequest :DTORequestPagination
    {
        public DTOFieldFilter customername { get; set; }
        public DTOFieldFilter tc { get; set; }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("customer", "customerid");

            if (customername != null) filter.fieldFilters.Add(customername);
            if (tc != null) filter.fieldFilters.Add(tc);

            return filter;
        }
    }
}
