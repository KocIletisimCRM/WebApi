using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class SLBayiReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<SLBayiReport>> get(int BayiId)
        {
            var d = DateTime.Now;
            var dtr = new DateTimeRange { start = (d - d.TimeOfDay).AddDays(1 - d.Day), end = d.AddDays(1 - d.Day).AddMonths(1).AddDays(-1) };
            var report = (await AdslReportsController.getBayiSLReport(BayiId, dtr));
            return report.AsQueryable();
        }
    }
}