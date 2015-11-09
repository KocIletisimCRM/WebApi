namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public  class DTOGetBlockFilter:DTORequestPagination
    {
     
        public DTOFieldFilter block { get; set; }
        public DTOFieldFilter sitename { get; set; }
        public DTOFieldFilter region { get; set; }
        public DTOFieldFilter telocadia { get; set; }
        public DTOFieldFilter locationid { get; set; }
        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("block", "blockid");
            var subFilter = new DTOFilter("site", "siteid");

            if (block!=null)
            {
                filter.fieldFilters.Add(block);
            }
            if (sitename != null || region!=null)
            {
               if(sitename!=null) subFilter.fieldFilters.Add(sitename);
               filter.subTables.Add("siteid", subFilter);
            }
            if (region!=null)
            {
                subFilter.fieldFilters.Add(region);
            }
            if (telocadia != null) filter.fieldFilters.Add(telocadia);
            if (locationid != null) filter.fieldFilters.Add(locationid);
            return filter;
        }
    }
}
