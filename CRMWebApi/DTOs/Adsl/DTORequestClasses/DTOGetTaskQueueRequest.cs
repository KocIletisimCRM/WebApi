namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOGetTaskQueueRequest : DTORequestPagination, ITaskRequest, ICSBRequest, IPersonelRequest, IAssistantPersonelRequest, IUpdatedByRequest
    {
        private DTOFilterGetTasksRequest taskRequest = new DTOFilterGetTasksRequest();
        private DTOFilterGetCSBRequest csbRequest = new DTOFilterGetCSBRequest();
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
        public DTOFieldFilter taskstate
        {
            get
            {
                return taskRequest.taskstate;
            }
            set
            {
                taskRequest.taskstate = value;
            }
        }
        public DTOFieldFilter objecttype
        {
            get
            {
                return taskRequest.objecttype;
            }
            set
            {
                taskRequest.objecttype = value;
            }
        }
        public DTOFieldFilter personeltype
        {
            get
            {
                return taskRequest.personeltype;
            }
            set
            {
                taskRequest.personeltype = value;
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
        bool ITaskRequest.hasTaskstateFilter()
        {
            return taskRequest.hasTaskstateFilter();
        }
        bool ITaskRequest.hasObjecttypeFilter()
        {
            return taskRequest.hasObjecttypeFilter();
        }
        bool ITaskRequest.hasPersoneltypeFilter()
        {
            return taskRequest.hasPersoneltypeFilter();
        }
        bool ITaskRequest.isTaskFilter()
        {
            return taskRequest.isTaskFilter();
        }

        bool ITaskRequest.isTypeFilter()
        {
            return taskRequest.isTypeFilter();
        }
        bool ITaskRequest.isTaskstateFilter()
        {
            return taskRequest.isTaskstateFilter();
        }
        bool ITaskRequest.isPersonelTypeFilter()
        {
            return taskRequest.isPersonelTypeFilter();
        }
        bool ITaskRequest.isObjecttypeFilter()
        {
            return taskRequest.isObjecttypeFilter();
        }
        DTOFilter ITaskRequest.getFilter()
        {
            return taskRequest.getFilter();
        }
        #endregion

        #region ICSBRequest Implementation
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

        public DTOFieldFilter ilce
        {
            get
            {
                return csbRequest.ilce;
            }
            set
            {
                csbRequest.ilce = value;
            }
        }

        public DTOFieldFilter il
        {
            get
            {
                return csbRequest.il;
            }
            set
            {
                csbRequest.il = value;
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

        public DTOFieldFilter iss
        {
            get
            {
                return csbRequest.iss;
            }
            set
            {
                csbRequest.iss = value;
            }
        }

        public DTOFieldFilter customerstatus
        {
            get
            {
                return csbRequest.customerstatus;
            }
            set
            {
                csbRequest.customerstatus = value;
            }
        }

        public bool hasIlceFilter()
        {
            return csbRequest.hasIlceFilter();
        }

        public bool hasIlFilter()
        {
            return csbRequest.hasIlFilter();
        }

        public bool hasCustomerFilter()
        {
            return csbRequest.hasCustomerFilter();
        }

        public bool hasIssFilter() 
        {
            return csbRequest.hasIssFilter();
        }

        public bool hasCustomerstatusFilter() 
        {
            return csbRequest.hasCustomerstatusFilter();
        }

        public bool isCustomerstatusFilter() 
        {
            return csbRequest.isCustomerstatusFilter();
        }

        public bool isIssFilter() 
        {
            return csbRequest.isIssFilter();
        }

        public bool isCustomerFilter()
        {
            return csbRequest.isCustomerFilter();
        }

        public bool isIlFilter()
        {
            return csbRequest.isIlFilter();
        }

        public bool isIlceFilter()
        {
            return csbRequest.isIlceFilter();
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




        public bool hasTaskFilter() {
            return taskRequest.hasTaskFilter() || taskRequest.hasTypeFilter() || taskRequest.hasTaskstateFilter();
        }
        public bool hasCSBFilter()
        {
            return csbRequest.hasCustomerFilter() || csbRequest.isIlFilter() || csbRequest.hasIlceFilter() || csbRequest.hasIssFilter() || csbRequest.hasCustomerstatusFilter();
        }
        public string attachmentDate { get; set; }
        public string appointmentDate { get; set; }
        public string consummationDate { get; set; }
        public int? taskOrderNo { get; set; }
        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("taskqueue", "taskorderno");
            var csbFilter = csbRequest.getFilter();
            if (hasTaskFilter())
            {
                var taskFilter = taskRequest.getFilter();
                if (taskRequest.hasTaskFilter() || taskRequest.hasTypeFilter())
                    filter.subTables.Add("taskid", taskFilter.subTables["taskid"]);
                if (taskRequest.hasTaskstateFilter())
                    if (taskRequest.taskstate.op != 8)
                        filter.subTables.Add("status", taskFilter.subTables["stateid"]);
                    else
                        filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "status", op = 8 });
            }
            if (personelRequest.personel != null)
            {
                if (personelRequest.personel.op == 8)
                    filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "attachedpersonelid", op = 8 });
                else
                    filter.subTables.Add("attachedpersonelid", personelRequest.getFilter());
            }
            if (assistantPersonel != null) filter.subTables.Add("assistant_personel", assistantPersonelRequest.getFilter());
            if (updatedBy != null) filter.subTables.Add("updatedby",updatedByRequest.getFilter());
            if (attachmentDate != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "attachmentdate", op = 5, value = attachmentDate });
            if (appointmentDate != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "appointmentdate", op = 5, value = appointmentDate });
            if (consummationDate != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "consummationdate", op = 5, value = consummationDate });
            if (taskOrderNo != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "taskorderno", op = 2, value = taskOrderNo });
            //hasCSBFilter her zaman en sonda olmalı. öncesine filtreler eklenecek
            if (hasCSBFilter())
            {     
                filter.subTables["attachedobjectid"] = csbFilter;
            }
            return filter;
        }




      
    }
}