using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public class DTOGetTaskQueueRequest1:ITaskRequest, ICSBRequest
    {
        private DTOFilterGetTasksRequest taskRequest = new DTOFilterGetTasksRequest();
        private DTOFilterGetCSBRequest csbRequest = new DTOFilterGetCSBRequest();

        #region ITaskRequest Implementation
        public DTOFieldFilter task
        {
            get
            {
                return taskRequest.task;
            }
            set
            {
                taskRequest.task = value;
            }
        }

        public DTOFieldFilter taskType
        {
            get
            {
                return taskRequest.taskType;
            }
            set
            {
                taskRequest.taskType = value;
            }
        }

        bool ITaskRequest.hasTaskFilter()
        {
            return taskRequest.hasTaskFilter();
        }

        bool ITaskRequest.hasTypeFilter()
        {
            return taskRequest.hasTypeFilter();
        }

        bool ITaskRequest.isTaskFilter()
        {
            return taskRequest.isTaskFilter();
        }

        bool ITaskRequest.isTypeFilter()
        {
            return taskRequest.isTypeFilter();
        }

        DTOFilter ITaskRequest.getFilter()
        {
            return taskRequest.getFilter();
        }
        #endregion

        #region ICSCRequest Implementation
        public DTOFieldFilter region
        {
            get
            {
                return csbRequest.region;
            }
            set
            {
                csbRequest.region = value;
            }
        }

        public DTOFieldFilter site
        {
            get
            {
                return csbRequest.site;
            }
            set
            {
                csbRequest.site = value;
            }
        }

        public DTOFieldFilter block
        {
            get
            {
                return csbRequest.block;
            }
            set
            {
                csbRequest.block = value;
            }
        }

        public DTOFieldFilter telocordiaid
        {
            get
            {
                return csbRequest.telocordiaid;
            }
            set
            {
                csbRequest.telocordiaid = value;
            }
        }

        public DTOFieldFilter sofiberbaslangic
        {
            get
            {
                return csbRequest.sofiberbaslangic;
            }
            set
            {
                csbRequest.sofiberbaslangic = value;
            }
        }

        public DTOFieldFilter customer
        {
            get
            {
                return csbRequest.customer;
            }
            set
            {
                csbRequest.customer = value;
            }
        }

        public bool hasSiteFilter()
        {
            return csbRequest.hasSiteFilter();
        }

        public bool hasBlockFilter()
        {
            return csbRequest.hasBlockFilter();
        }

        public bool hasCustomerFilter()
        {
            return csbRequest.hasCustomerFilter();
        }

        public bool isCustomerFilter()
        {
            return csbRequest.isCustomerFilter();
        }

        public bool isBlockFilter()
        {
            return csbRequest.isBlockFilter();
        }

        public bool isSiteFilter()
        {
            return csbRequest.isSiteFilter();
        }

        public bool isRegionFilter()
        {
            return csbRequest.isRegionFilter();
        }

        DTOFilter ICSBRequest.getFilter()
        {
            return csbRequest.getFilter();
        }
        #endregion

        public bool hasTaskFilter() {
            return taskRequest.hasTaskFilter() || taskRequest.hasTypeFilter();
        }

        public bool hasCSBFilter()
        {
            return csbRequest.hasCustomerFilter() || csbRequest.isBlockFilter() || csbRequest.hasSiteFilter();
        }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("taskqueue", "taskorderno");
            if (hasTaskFilter()) filter.subTables.Add("taskid", taskRequest.getFilter());
            if (hasCSBFilter())
            {
                if(csbRequest.isCustomerFilter())
                filter.subTables.Add("attachedobjectid", csbRequest.getFilter());
                //else if(csbRequest.isBlockFilter())

            }
            return filter;
        }

    }
}