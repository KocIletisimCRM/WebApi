using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public interface ITaskStateRequest 
    {
        DTOFieldFilter taskstate { get; set; }
        DTOFilter getFilter();
    }

    public class DTOFilterGetTaskStateRequest : ITaskStateRequest
    {
     public   DTOFieldFilter taskstate { get; set; }

     public DTOFilter getFilter()
     {
         DTOFilter filter = new DTOFilter("taskstatepool", "taskstateid");
         if (taskstate != null) filter.fieldFilters.Add(taskstate);
         return filter;
     }

    }
}