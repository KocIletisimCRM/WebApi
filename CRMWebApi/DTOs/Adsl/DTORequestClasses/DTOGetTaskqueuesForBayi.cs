using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOGetTaskqueuesForBayi : DTORequestPagination, ICSBRequest
    {
        private DTOFilterGetCSBRequest csbRequest = new DTOFilterGetCSBRequest();
        public DTOFieldFilter region
        {
            get
            {
                return csbRequest.region;
            }
            set
            {
                csbRequest.region = value;
            }
        }

        public DTOFieldFilter ilce
        {
            get
            {
                return csbRequest.ilce;
            }
            set
            {
                csbRequest.ilce = value;
            }
        }

        public DTOFieldFilter il
        {
            get
            {
                return csbRequest.il;
            }
            set
            {
                csbRequest.il = value;
            }
        }

        public DTOFieldFilter telocordiaid
        {
            get
            {
                return csbRequest.telocordiaid;
            }
            set
            {
                csbRequest.telocordiaid = value;
            }
        }

        public DTOFieldFilter sofiberbaslangic
        {
            get
            {
                return csbRequest.sofiberbaslangic;
            }
            set
            {
                csbRequest.sofiberbaslangic = value;
            }
        }

        public DTOFieldFilter customer
        {
            get
            {
                return csbRequest.customer;
            }
            set
            {
                csbRequest.customer = value;
            }
        }

        public DTOFieldFilter iss
        {
            get
            {
                return csbRequest.iss;
            }
            set
            {
                csbRequest.iss = value;
            }
        }

        public DTOFieldFilter customerstatus
        {
            get
            {
                return csbRequest.customerstatus;
            }
            set
            {
                csbRequest.customerstatus = value;
            }
        }

        public DTOFieldFilter superonline
        {
            get
            {
                return csbRequest.superonline;
            }
            set
            {
                csbRequest.superonline = value;
            }
        }

        public bool hasIlceFilter()
        {
            return csbRequest.hasIlceFilter();
        }

        public bool hasIlFilter()
        {
            return csbRequest.hasIlFilter();
        }

        public bool hasCustomerFilter()
        {
            return csbRequest.hasCustomerFilter();
        }

        public bool hasIssFilter()
        {
            return csbRequest.hasIssFilter();
        }

        public bool hasCustomerstatusFilter()
        {
            return csbRequest.hasCustomerstatusFilter();
        }

        public bool isCustomerstatusFilter()
        {
            return csbRequest.isCustomerstatusFilter();
        }

        public bool isIssFilter()
        {
            return csbRequest.isIssFilter();
        }

        public bool isCustomerFilter()
        {
            return csbRequest.isCustomerFilter();
        }

        public bool isIlFilter()
        {
            return csbRequest.isIlFilter();
        }

        public bool isIlceFilter()
        {
            return csbRequest.isIlceFilter();
        }

        public bool isRegionFilter()
        {
            return csbRequest.isRegionFilter();
        }

        DTOFilter ICSBRequest.getFilter()
        {
            return csbRequest.getFilter();
        }
        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("taskqueue", "taskorderno");
            var csbFilter = csbRequest.getFilter();
            filter.subTables["attachedobjectid"] = csbFilter;
            return filter;
        }

        public bool hasSuperonlineFilter()
        {
            throw new NotImplementedException();
        }
    }
}