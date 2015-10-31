using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public class DTOGetTSPFilter : DTORequestPagination
    {

        public DTOFieldFilter taskstate { get; set; }
        public DTOFieldFilter statetype { get; set; }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("taskstatepool", "taskstateid");

            if (taskstate != null)
            {
                filter.fieldFilters.Add(taskstate);
            }
            if (statetype != null )
            {
                filter.fieldFilters.Add(statetype);
            }
          
            return filter;
        }
    }
}
