namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{

    public interface ITaskRequest
    {
        DTOFieldFilter task { get; set; }
        DTOFieldFilter taskType { get; set; }
        DTOFieldFilter taskstate { get; set; }
        DTOFieldFilter objecttype { get; set; }
        DTOFieldFilter personeltype { get; set; }
        bool hasTaskFilter();
        bool hasTypeFilter();
        bool hasTaskstateFilter();
        bool hasObjecttypeFilter();
        bool hasPersoneltypeFilter();
        bool isTaskFilter();
        bool isTypeFilter();
        bool isTaskstateFilter();
        bool isObjecttypeFilter();
        bool isPersonelTypeFilter();
        DTOFilter getFilter();
    }
    /// <summary> 
    /// Web Uygulamasındaki filtreleme bileşenlerinin verilerini çekmek için kullanılır 
    /// </summary>
    public class DTOFilterGetTasksRequest : DTORequestPagination, ITaskRequest
    {
        public DTOFieldFilter task { get; set; }
        public DTOFieldFilter taskType { get; set; }
        public DTOFieldFilter taskstate { get; set; }
        public DTOFieldFilter objecttype { get; set; }
        public DTOFieldFilter personeltype { get; set; }


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
        public bool hasObjecttypeFilter()
        {
            return objecttype != null;
        }

        public bool hasPersoneltypeFilter()
        {
            return personeltype!= null;
        }

        public bool isTaskFilter()
        {
            return (!hasTypeFilter() && !hasTaskstateFilter() && !hasObjecttypeFilter() && !hasPersoneltypeFilter());
        }

        public bool isTypeFilter()
        {
            return !hasTaskstateFilter() && hasTypeFilter();
        }

        public bool isTaskstateFilter()
        {
            return hasTaskstateFilter();
        }

        public bool isPersonelTypeFilter()
        {
            return !hasTaskFilter() && hasPersoneltypeFilter();
        }

        public bool isObjecttypeFilter()
        {
            return !hasTaskstateFilter() && hasObjecttypeFilter();
        }
        public DTOFilter getFilter()
        {

            DTOFilter filter = new DTOFilter("taskstatematches", "id");

            if (hasTaskFilter() || hasTypeFilter() ||hasObjecttypeFilter() || hasPersoneltypeFilter())
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
            if (hasObjecttypeFilter())
            {
                var subFilter = new DTOFilter("objecttypes", "typeid");
                subFilter.fieldFilters.Add(objecttype);
                filter.subTables["taskid"].subTables.Add("attachableobjecttype", subFilter);
            }
            if (hasPersoneltypeFilter())
            {
                var subFilter = new DTOFilter("objecttypes", "typeid");
                subFilter.fieldFilters.Add(personeltype);
                filter.subTables["taskid"].subTables.Add("attachablepersoneltype", subFilter);
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