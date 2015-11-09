namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public   class DTOGetProductFilter:DTORequestPagination
    {
        public DTOFieldFilter product { get; set; }
        public DTOFieldFilter category { get; set; }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("product_service", "productid");

            if (product != null)
            {
                filter.fieldFilters.Add(product);
            }
            if (category != null)
            {
                filter.fieldFilters.Add(category);
            }

            return filter;
        }
    }
}
