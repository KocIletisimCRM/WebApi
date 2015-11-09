namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public class DTOFilterGetStockCardRequest:DTORequestPagination
    {
        public DTOFieldFilter stockcard { get; set; }
        public DTOFieldFilter category { get; set; }

        public DTOFilter getFilter()
        {
            DTOFilter filter = new DTOFilter("stockcard","stockid");
            if (stockcard != null)
            {
                filter.fieldFilters.Add(stockcard);
               
            }
             if (category != null)
            {
                filter.fieldFilters.Add(category);
              
            }
            
            return filter;
        }
    }
}
