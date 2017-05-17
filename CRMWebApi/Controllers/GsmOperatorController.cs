using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/GsmOperator")]
    [KOCAuthorize]
    public class GsmOperatorController : ApiController
    {
        [Route("getGsmOperators"), HttpGet, HttpPost]
        public HttpResponseMessage getGsmOperators()
        {
            using (var db = new KOCSAMADLSEntities())
                return Request.CreateResponse(HttpStatusCode.OK, db.GSMO.ToList(), "application/json");
        }
    }
}