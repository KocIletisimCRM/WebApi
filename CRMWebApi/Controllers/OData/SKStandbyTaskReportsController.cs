using CRMWebApi.DTOs.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class SKStandbyTaskReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<SKStandbyTaskReport>> get()
        {
            var report = (await AdslReportsController.getSKStandbyTaskReport());
            return report.AsQueryable();
        }
    }
}
