using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.DTORequestClasses
{
   public  class DTOGetSiteFilter:DTORequestPagination
    {
        public DTOFieldFilter sitename { get; set; }
        public DTOFieldFilter region { get; set; }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("site", "siteid");
            if (sitename != null)
            {
                filter.fieldFilters.Add(sitename);
            }
            if (region != null)
            {
                filter.fieldFilters.Add(region);
            }
            return filter;
        }
    }
}
