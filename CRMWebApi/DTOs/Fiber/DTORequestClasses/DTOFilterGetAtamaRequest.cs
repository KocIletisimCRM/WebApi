using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public class DTOFilterGetAtamaRequest : DTORequestPagination
    {
        public DTOFieldFilter id { get; set; }
        public DTOFieldFilter task { get; set; }
        public DTOFieldFilter personel { get; set; }
        public DTOFieldFilter personeloff { get; set; }
        public DTOFieldFilter taskclosed { get; set; }
        public DTOFieldFilter tasktype { get; set; }
        public DTOFieldFilter tasktypeclosed { get; set; }
        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("atama", "id");
            if (id != null) filter.fieldFilters.Add(id);
            if (task != null) filter.fieldFilters.Add(task);
            if (personel != null) filter.fieldFilters.Add(personel);
            if (personeloff != null) filter.fieldFilters.Add(personeloff);
            if (taskclosed != null) filter.fieldFilters.Add(taskclosed);
            if (tasktype != null) filter.fieldFilters.Add(tasktype);
            if (tasktypeclosed != null) filter.fieldFilters.Add(tasktypeclosed);
            return filter;
        }
    }
}