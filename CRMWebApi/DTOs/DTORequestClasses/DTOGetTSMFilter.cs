using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.DTORequestClasses
{
   public class DTOGetTSMFilter: DTORequestPagination
    {

        public DTOFieldFilter task { get; set; }
        public DTOFieldFilter taskstate { get; set; }
        public DTOFieldFilter taskstatematches { get; set; }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("taskstatematches", "id");
            if (taskstatematches != null) filter.fieldFilters.Add(taskstatematches);
            if (task != null)
            {
                var subFilter = new DTOFilter("task", "taskid");
                if (task != null) subFilter.fieldFilters.Add(task);
                filter.subTables.Add("taskid", subFilter);
            }
            if (taskstate != null)
            {
                var subFilter = new DTOFilter("taskstatepool", "taskstateid");
                subFilter.fieldFilters.Add(taskstate);
                filter.subTables.Add("stateid", subFilter);
            }

            return filter;
        }
    }
}
