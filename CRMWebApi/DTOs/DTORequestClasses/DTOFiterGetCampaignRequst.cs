// compile with: /doc:DocFileName.xml
using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs;
using System.Data.SqlClient;
using CRMWebApi.DTOs.DTORequestClasses;
namespace CRMWebApi.DTOs.DTORequestClasses
{
    public class DTOFiterGetCampaignRequst
    {
        public DTOFieldFilter category { get; set; }
        public DTOFieldFilter subcategory { get; set; }
        public DTOFieldFilter campaign { get; set; }
        public DTOFieldFilter selectedcampaign { get; set; }

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
         public bool hasSelectedCampaignFilter() 
         {
             return selectedcampaign != null;
         }

        public bool isCategoryFilter() 
        {
            return !hasSubcategoryFilter() && !hasCampaignFilter();
        }
        public bool isSubcategoryFilter() 
        {
            return !hasCampaignFilter();
        }

        public bool isCampaignFilter() 
        {
            return hasCategoryFilter() && hasSubcategoryFilter() && hasCampaignFilter();
        }
        public bool isSelectedCampaignFilter()
        {
            return hasCategoryFilter() && hasSubcategoryFilter() && hasCampaignFilter() && hasSelectedCampaignFilter();
        }

        public DTOFilter getFilter()
        {
            using (var db = new CRMEntities())
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
                //if (hasSelectedCampaignFilter())
                //{
                //    var p = db.campaigns.Where(c => c.name==selectedcampaign.value).FirstOrDefault();
                //    List<int> productids = new List<int>();
                //    foreach (var item in p.products.Split(',').ToList())
                //    {

                //        productids.Add(Convert.ToInt32(item));
                //    }
                //    filter = new DTOFilter("product_service","productid");
                //    int[] pids= productids.ToArray();
                //    filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "productid", value = pids, op = 7 });

                //}
                return filter;
            }
        }
    }
}