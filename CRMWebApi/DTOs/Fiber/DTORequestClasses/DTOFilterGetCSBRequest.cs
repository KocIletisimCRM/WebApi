using CRMWebApi.Models.Fiber;

namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public interface ICSBRequest
    {
        DTOFieldFilter region { get; set; }
        DTOFieldFilter site { get; set; }
        DTOFieldFilter block { get; set; }
        DTOFieldFilter telocordiaid { get; set; }
        DTOFieldFilter sofiberbaslangic { get; set; }
        DTOFieldFilter customer { get; set; }
        DTOFieldFilter iss { get; set; }
        DTOFieldFilter customerstatus { get; set; }

        bool hasSiteFilter();
        bool hasBlockFilter();
        bool hasCustomerFilter();
        bool hasIssFilter();
        bool hasCustomerstatusFilter();        
        bool isCustomerFilter();
        bool isBlockFilter();
        bool isSiteFilter();
        bool isRegionFilter();
        bool isIssFilter();
        bool isCustomerstatusFilter();
        DTOFilter getFilter();

    }
    public class DTOFilterGetCSBRequest : ICSBRequest
    {
        public DTOFieldFilter region { get; set; }
        public DTOFieldFilter site { get; set; }
        public DTOFieldFilter block { get; set; }
        public DTOFieldFilter telocordiaid { get; set; }
        public DTOFieldFilter sofiberbaslangic { get; set; }
        public DTOFieldFilter customer { get; set; }
        public DTOFieldFilter iss { get; set; }
        public DTOFieldFilter customerstatus { get; set; }

        public bool hasSiteFilter()
        {
            return region != null || site != null;
        }
        public bool hasBlockFilter()
        {
            return block != null || telocordiaid != null || sofiberbaslangic != null;
        }
        public bool hasCustomerFilter()
        {
            return customer != null;
        }

        public bool hasIssFilter() 
        {
            return iss != null;
        }

        public bool hasCustomerstatusFilter() 
        {
            return customerstatus != null;
        }
        public bool isIssFilter() 
        {
            return hasIssFilter() && !isCustomerFilter();
        }
        public bool isCustomerstatusFilter() 
        {
            return hasCustomerstatusFilter() && !isCustomerFilter();
        }
        public bool isCustomerFilter()
        {
            return hasCustomerFilter() || (!hasBlockFilter() && !hasSiteFilter());
        }

        public bool isBlockFilter()
        {
            return !isCustomerFilter() && hasBlockFilter();
        }

        public bool isSiteFilter()
        {
            return !isCustomerFilter() && !hasBlockFilter() && site != null;
        }

        public bool isRegionFilter()
        {
            return !isCustomerFilter() && !hasBlockFilter() && site == null;
        }

        public DTOFilter getFilter()
        {
            using (var db = new CRMEntities())
            {
                DTOFilter filter = new DTOFilter("customer", "customerid");
                // Ad Soyad Ayrımı Yapılacak 
                if (customer != null)
                {
                    //new DTOFieldFilter { fieldName = "category", op = 2, value = objecttype.value }
                    if (customer.fieldName == "customername")
                    {
                        if (customer.value.ToString().IndexOf(' ') > -1)
                        {
                            string name = customer.value.ToString().Split(' ')[0];
                            string sname = customer.value.ToString().Split(' ')[1];
                            filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "customername", op = 6, value = name });
                            filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "customersurname", op = 6, value = sname });
                        }
                        else
                            filter.fieldFilters.Add(customer);
                    }
                    else
                        filter.fieldFilters.Add(customer);

                }

                if (hasIssFilter())
                {
                    var subFilter = new DTOFilter("issStatus", "id");
                    subFilter.fieldFilters.Add(iss);
                    filter.subTables.Add("iss",subFilter);
                }
                if (hasCustomerstatusFilter()) 
                {
                    var subFilter = new DTOFilter("customer_status","ID");
                    subFilter.fieldFilters.Add(customerstatus);
                    filter.subTables.Add("customerstatus",subFilter);
                }
                if (hasBlockFilter() || hasSiteFilter())
                {
                    var subFilter = new DTOFilter("block", "blockid");
                    if (block != null) subFilter.fieldFilters.Add(block);
                    if (telocordiaid != null) subFilter.fieldFilters.Add(telocordiaid);
                    if (sofiberbaslangic != null) subFilter.fieldFilters.Add(sofiberbaslangic);
                    filter.subTables.Add("blockid", subFilter);
                }

                if (hasSiteFilter())
                {
                    var subFilter = new DTOFilter("site", "siteid");
                    if (region != null) subFilter.fieldFilters.Add(region);
                    if (site != null) subFilter.fieldFilters.Add(site);
                    filter.subTables["blockid"].subTables.Add("siteid", subFilter);
                }
                return filter;
            }
        }
    }

}