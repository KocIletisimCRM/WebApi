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

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("customer", "customerid");

            if (customername != null)
            {
                filter.fieldFilters.Add(customername);
            }
           

            return filter;
        }
    }
}
