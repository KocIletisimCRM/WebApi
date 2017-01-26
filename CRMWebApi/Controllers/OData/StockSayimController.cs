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
    public class StockSayimController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<stocksayim>> get()
        {
            var report = (await AdslReportsController.stock());
            return report.AsQueryable();
        }
    }
}
