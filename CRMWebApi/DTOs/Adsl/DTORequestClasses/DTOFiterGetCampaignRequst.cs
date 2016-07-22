// compile with: /doc:DocFileName.xml
using CRMWebApi.Models.Adsl;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOFiterGetCampaignRequst :DTORequestPagination
    {
        public DTOFieldFilter category { get; set; }
        public DTOFieldFilter subcategory { get; set; }
        public DTOFieldFilter campaign { get; set; }
        public DTOFieldFilter products { get; set; }
        public DTOFieldFilter deleted { get; set; }

        public bool hasCategoryFilter() 
        {
            return category != null;
        }
        public bool hasSubcategoryFilter() 
        {
            return subcategory != null;
        }
         public bool hasCampaignFilter() 
        {
            return campaign != null;
        }
         public bool hasProductsFilter() 
         {
             return products != null;
         }

        public bool isCategoryFilter() 
        {
            return hasCategoryFilter() && !(hasSubcategoryFilter() || hasCampaignFilter() || hasProductsFilter());
        }
        public bool isSubcategoryFilter() 
        {
            return hasSubcategoryFilter() && !(hasCampaignFilter() || hasProductsFilter());
        }

        public bool isCampaignFilter() 
        {
            return hasCampaignFilter() && !isProductsFilter();
        }
        public bool isProductsFilter()
        {
            return hasProductsFilter();
        }

        public DTOFilter getFilter()
        {
            using (var db = new KOCSAMADLSEntities())
            {
                DTOFilter filter = new DTOFilter("campaigns", "id");
                if (hasCategoryFilter())
                {
                    filter.fieldFilters.Add(category);
                }
                if (hasSubcategoryFilter())
                {
                    filter.fieldFilters.Add(subcategory);
                }
                if (hasCampaignFilter())
                    filter.fieldFilters.Add(campaign);
                if (deleted != null) filter.fieldFilters.Add(deleted);
                if (hasProductsFilter())
                {
                    var subFilter = new DTOFilter("vcampaignproducts","cid");
                    var psubFilter = new DTOFilter("product_service","productid");
                    psubFilter.fieldFilters.Add(products);
                    subFilter.subTables.Add("pid",psubFilter);
                    filter.subTables.Add("id", subFilter);

                }
                return filter;
            }
        }
    }
}