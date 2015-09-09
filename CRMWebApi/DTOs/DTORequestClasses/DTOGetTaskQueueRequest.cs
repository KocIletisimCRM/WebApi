using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public class DTOGetTaskQueueRequest : DTORequestPagination, ITaskRequest, ICSBRequest, IPersonelRequest, IAssistantPersonelRequest, IUpdatedByRequest, ITaskStateRequest
    {
        private DTOFilterGetTasksRequest taskRequest = new DTOFilterGetTasksRequest();
        private DTOFilterGetCSBRequest csbRequest = new DTOFilterGetCSBRequest();
        private DTOFilterGetTaskStateRequest taskstateRequest = new DTOFilterGetTaskStateRequest();
        private DTOFilterGetPersonelRequest personelRequest = new DTOFilterGetPersonelRequest();
        private DTOFilterGetPersonelRequest assistantPersonelRequest = new DTOFilterGetPersonelRequest();
        private DTOFilterGetPersonelRequest updatedByRequest = new DTOFilterGetPersonelRequest();

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

        #region IPersonelRequest Implementation
        public DTOFieldFilter personel
        {
            get
            {
                return personelRequest.personel;
            }
            set
            {
                personelRequest.personel = value;
            }
        }

        DTOFilter IPersonelRequest.getFilter()
        {
            return personelRequest.getFilter();
        }
        #endregion

        #region IAssistantPersonelRequest Implementation
        public DTOFieldFilter assistantPersonel { get { return assistantPersonelRequest.personel; } set { assistantPersonelRequest.personel = value; } }
        DTOFilter IAssistantPersonelRequest.getFilter() {
            return assistantPersonelRequest.getFilter();
        }
        #endregion

        #region IUpdatedByRequest Implementation
        public DTOFieldFilter updatedBy { get { return updatedByRequest.personel; } set { updatedByRequest.personel = value; } }
        DTOFilter IUpdatedByRequest.getFilter()
        {
            return updatedByRequest.getFilter();
        }
        #endregion


        #region ITaskStateRequest Implementation
        public DTOFieldFilter taskstate
        {
            get
            {
                return taskstateRequest.taskstate;
            }
            set
            {
                taskstateRequest.taskstate = value;
            }
        }
         DTOFilter ITaskStateRequest.getFilter() 
        {
            return taskstateRequest.getFilter();
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
            var csbFilter = csbRequest.getFilter();
            if (hasTaskFilter()) filter.subTables.Add("taskid", taskRequest.getFilter());
            if (personelRequest.personel != null ) filter.subTables.Add("attachedpersonelid", personelRequest.getFilter());
            if (assistantPersonel != null) filter.subTables.Add("assistant_personel", assistantPersonelRequest.getFilter());
            if (updatedBy != null) filter.subTables.Add("updatedby",updatedByRequest.getFilter());
            if (taskstateRequest.taskstate!=null) filter.subTables.Add("status",taskstateRequest.getFilter());

            //hasCSBFilter her zaman en sonda olmalı. öncesine filtreler eklenecek
            if (hasCSBFilter())
            {
                if (csbRequest.hasSiteFilter())
                {
                    filter.subTables["attachedobjectid"] = csbFilter.subTables["blockid"].subTables["siteid"];
                }
                if (csbRequest.hasBlockFilter() || csbRequest.hasSiteFilter())
                {
                    filter.subTables["attachedobjectid"] = csbFilter.subTables["blockid"];
                }
                filter.subTables["attachedobjectid"] = csbFilter;
            }
            return filter;
        }




      
    }
}