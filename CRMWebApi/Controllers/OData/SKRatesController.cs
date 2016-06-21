﻿using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class SKRatesController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<SKRate>> get(DateTime start, DateTime end)
        {
            var dtr = new DateTimeRange { start = start, end = end };
            var report = (await AdslReportsController.getRates(dtr));
            return report.AsQueryable();
        }
        [EnableQuery]
        public async Task<IQueryable<SKRate>> get()
        {
            var d = DateTime.Now;
            var dtr = new DateTimeRange { start = (d - d.TimeOfDay).AddDays(1 - d.Day), end = (d.AddDays(1 - d.Day).AddMonths(1).AddDays(-1)).Date.AddDays(1) };
            var report = (await AdslReportsController.getRates(dtr));
            return report.AsQueryable();
        }
    }
}
