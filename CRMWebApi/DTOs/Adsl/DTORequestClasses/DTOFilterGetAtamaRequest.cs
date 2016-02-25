namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOFilterGetAtamaRequest : DTORequestPagination
    {
        public DTOFieldFilter id { get; set; }
        public DTOFieldFilter task { get; set; }
        public DTOFieldFilter personel { get; set; }
        public DTOFieldFilter personel2 { get; set; }
        public DTOFieldFilter task2 { get; set; }
        public DTOFieldFilter tasktype { get; set; }
        public DTOFieldFilter tasktype2 { get; set; }
        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("atama", "id");
            //var subFilter = new DTOFilter("atama", "closedtask");
            if (id != null) filter.fieldFilters.Add(id);
            if (task != null) filter.fieldFilters.Add(task);
            if (task2 != null) filter.fieldFilters.Add(task2);
            if (personel != null) filter.fieldFilters.Add(personel);
            if (personel2 != null) filter.fieldFilters.Add(personel2);
            if (tasktype2 != null) filter.fieldFilters.Add(tasktype2);
            if (tasktype != null) filter.fieldFilters.Add(tasktype);
            return filter;
        }
    }
}