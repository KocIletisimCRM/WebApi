﻿using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs;
using System.Data.SqlClient;
namespace CRMWebApi.DTOs.DTORequestClasses
{
    public interface ICSBRequest
    {
        DTOFieldFilter region { get; set; }
        DTOFieldFilter site { get; set; }
        DTOFieldFilter block { get; set; }
        DTOFieldFilter telocordiaid { get; set; }
        DTOFieldFilter sofiberbaslangic { get; set; }
        DTOFieldFilter customer { get; set; }

        bool hasSiteFilter();
        bool hasBlockFilter();
        bool hasCustomerFilter();
        bool isCustomerFilter();
        bool isBlockFilter();
        bool isSiteFilter();
        bool isRegionFilter();
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
                if (customer != null) filter.fieldFilters.Add(customer);

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