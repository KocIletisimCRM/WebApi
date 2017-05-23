using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
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
    public class AgentSKReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<SKReport>> get(DateTime start, DateTime end, int perid, string pass)
        {
            var dtr = new DateTimeRange { start = start, end = end };
            var report = (await AdslReportsController.getAgentBayiSKReport(dtr, perid, pass));
            return report.AsQueryable();
        }
        [EnableQuery]
        public async Task<IQueryable<SKReport>> get(int perid, string pass)
        {
            var d = DateTime.Now;
            var dtr = new DateTimeRange { start = (d - d.TimeOfDay).AddDays(1 - d.Day), end = (d.AddDays(1 - d.Day).AddMonths(1).AddDays(-1)).Date.AddDays(1) };
            var report = (await AdslReportsController.getAgentBayiSKReport(dtr, perid, pass));
            return report.AsQueryable();
        }
    }
}