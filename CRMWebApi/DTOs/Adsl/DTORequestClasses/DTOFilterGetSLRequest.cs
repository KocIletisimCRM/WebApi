using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOFilterGetSLRequest : DTORequestPagination
    {
        public DTOFieldFilter SLID { get; set; }
        public DTOFieldFilter SLName { get; set; }
        public DTOFieldFilter KocSTask { get; set; }
        public DTOFieldFilter KocETask { get; set; }
        public DTOFieldFilter KocMaxTime { get; set; }
        public DTOFieldFilter BasyiSTask { get; set; }
        public DTOFieldFilter BayiETask { get; set; }
        public DTOFieldFilter BayiMaxTime { get; set; }
        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("SL", "SLID");
            if (SLID != null) filter.fieldFilters.Add(SLID);
            if (SLName != null) filter.fieldFilters.Add(SLName);
            if (KocSTask != null) filter.fieldFilters.Add(KocSTask);
            if (KocETask != null) filter.fieldFilters.Add(KocETask);
            if (KocMaxTime != null) filter.fieldFilters.Add(KocMaxTime);
            if (BasyiSTask != null) filter.fieldFilters.Add(BasyiSTask);
            if (BayiETask != null) filter.fieldFilters.Add(BayiETask);
            if (BayiMaxTime != null) filter.fieldFilters.Add(BayiMaxTime);
            return filter;
        }
    }
}