using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs;
using System.Data.SqlClient;

namespace CRMWebApi.DTOs.DTORequestClasses
{

    public interface ITaskRequest
    {
        DTOFieldFilter task { get; set; }
        DTOFieldFilter taskType { get; set; }
        bool hasTaskFilter();
        bool hasTypeFilter();
        bool isTaskFilter();
        bool isTypeFilter();
        DTOFilter getFilter();
    }
    /// <summary> 
    /// Web Uygulamasındaki filtreleme bileşenlerinin verilerini çekmek için kullanılır 
    /// </summary>
    public class DTOFilterGetTasksRequest : ITaskRequest
    {
        public DTOFieldFilter task { get; set; }
        public DTOFieldFilter taskType { get; set; }


        public bool hasTaskFilter()
        {
            return task != null;
        }

        public bool hasTypeFilter()
        {
            return taskType != null;
        }

        public bool isTaskFilter()
        {
            return !hasTypeFilter()||hasTaskFilter();
        }

        public bool isTypeFilter()
        {
            return !isTaskFilter() ;
        }

        public DTOFilter getFilter()
        {
            DTOFilter filter = new DTOFilter("task", "taskid");
            if (task != null) filter.fieldFilters.Add(task);
            if (hasTypeFilter())
            {
                var subFilter = new DTOFilter("tasktypes", "TaskTypeId");
                if (taskType != null) subFilter.fieldFilters.Add(taskType);
                filter.subTables.Add("tasktype", subFilter);
            }
            return filter;
        }
    }
}