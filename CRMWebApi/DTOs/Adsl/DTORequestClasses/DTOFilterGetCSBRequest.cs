using System;
using CRMWebApi.Models.Adsl;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public interface ICSBRequest
    {
        DTOFieldFilter region { get; set; }
        DTOFieldFilter ilce { get; set; }
        DTOFieldFilter il { get; set; }
        DTOFieldFilter telocordiaid { get; set; }
        DTOFieldFilter sofiberbaslangic { get; set; }
        DTOFieldFilter customer { get; set; }
        DTOFieldFilter iss { get; set; }
        DTOFieldFilter customerstatus { get; set; }
        DTOFieldFilter superonline { get; set; }

        bool hasIlceFilter();
        bool hasIlFilter();
        bool hasCustomerFilter();
        bool hasIssFilter();
        bool hasCustomerstatusFilter();        
        bool isCustomerFilter();
        bool hasSuperonlineFilter();
        bool isIlFilter();
        bool isIlceFilter();
        bool isRegionFilter();
        bool isIssFilter();
        bool isCustomerstatusFilter();
        DTOFilter getFilter();

    }
    public class DTOFilterGetCSBRequest : ICSBRequest
    {
        public DTOFieldFilter region { get; set; }
        public DTOFieldFilter ilce { get; set; }
        public DTOFieldFilter il { get; set; }
        public DTOFieldFilter telocordiaid { get; set; }
        public DTOFieldFilter sofiberbaslangic { get; set; }
        public DTOFieldFilter customer { get; set; }
        public DTOFieldFilter superonline { get; set; }
        public DTOFieldFilter iss { get; set; }
        public DTOFieldFilter customerstatus { get; set; }
        public bool hasIlceFilter()
        {
            return  ilce != null;
        }
        public bool hasIlFilter()
        {
            return il != null ;
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
            return hasCustomerFilter() || (!hasIlFilter() && !hasIlceFilter());
        }
        public bool hasSuperonlineFilter()
        {
            return superonline != null;
        }
        public bool isIlFilter()
        {
            return !isCustomerFilter() && hasIlFilter();
        }
        public bool isIlceFilter()
        {
            return !isCustomerFilter() && !hasIlFilter() && ilce != null;
        }
        public bool isRegionFilter()
        {
            return !isCustomerFilter() && !hasIlFilter() && ilce == null;
        }
        public DTOFilter getFilter()
        {
            using (var db = new KOCSAMADLSEntities())
            {
                DTOFilter filter = new DTOFilter("customer", "customerid");
                // Ad Soyad Ayrımı Yapılacak 
                if (customer != null) filter.fieldFilters.Add(customer);
                if (superonline != null) filter.fieldFilters.Add(superonline);
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
                if (hasIlFilter())
                {
                    var subFilter = new DTOFilter("il", "kimlikNo");
                    if (il != null) subFilter.fieldFilters.Add(il);                 
                    filter.subTables.Add("ilKimlikNo", subFilter);
                }

                if (hasIlceFilter())
                {
                    var subFilter = new DTOFilter("ilce", "kimlikNo");
                    if (ilce != null) subFilter.fieldFilters.Add(ilce);
                    filter.subTables.Add("ilceKimlikNo", subFilter);
                }
                return filter;
            }
        }

    }

}