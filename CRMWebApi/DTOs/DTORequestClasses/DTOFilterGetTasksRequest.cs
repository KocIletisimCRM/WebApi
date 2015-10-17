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
        DTOFieldFilter taskstate { get; set; }
        bool hasTaskFilter();
        bool hasTypeFilter();
        bool hasTaskstateFilter();
        bool isTaskFilter();
        bool isTypeFilter();
        bool isTaskstateFilter();
        DTOFilter getFilter();
    }
    /// <summary> 
    /// Web Uygulamasındaki filtreleme bileşenlerinin verilerini çekmek için kullanılır 
    /// </summary>
    public class DTOFilterGetTasksRequest : ITaskRequest
    {
        public DTOFieldFilter task { get; set; }
        public DTOFieldFilter taskType { get; set; }
        public DTOFieldFilter taskstate { get; set; }


        public bool hasTaskFilter()
        {
            return task != null;
        }

        public bool hasTypeFilter()
        {
            return taskType != null;
        }

        public bool hasTaskstateFilter()
        {
            return taskstate != null;
        }

        public bool isTaskFilter()
        {
            return (!hasTypeFilter() && !hasTaskstateFilter());
        }

        public bool isTypeFilter()
        {
            return !hasTaskstateFilter() && hasTypeFilter();
        }

        public bool isTaskstateFilter()
        {
            return hasTaskstateFilter();
        }
        public DTOFilter getFilter()
        {

            DTOFilter filter = new DTOFilter("taskstatematches", "id");

            if (hasTaskFilter() || hasTypeFilter())
            {
                var subFilter = new DTOFilter("task", "taskid");
                if (task != null) subFilter.fieldFilters.Add(task);
                filter.subTables.Add("taskid", subFilter);
            }
            if (hasTypeFilter())
            {
                var subFilter = new DTOFilter("tasktypes", "TaskTypeId");
                if (taskType != null) subFilter.fieldFilters.Add(taskType);
                filter.subTables["taskid"].subTables.Add("tasktype", subFilter);
            }
            if (hasTaskstateFilter())
            {
                var subFilter = new DTOFilter("taskstatepool", "taskstateid");
                subFilter.fieldFilters.Add(taskstate);
                filter.subTables.Add("stateid", subFilter);
            }
            return filter;
        }
    }
}