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
    public class InfoBayiReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<InfoBayiReport>> get()
        {
            var report = (await AdslReportsController.getInfoBayi());
            return report.AsQueryable();
        }
    }
}
