using CRMWebApi.DTOs;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Teknar_Proxy_Lib;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Address")]
    public class ADSLAddressController : ApiController
    {
        [Route("getAdress")]
        [HttpPost]
        public async Task<HttpResponseMessage>  getAdress(DTOGetAdressFilter request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                //var il = TeknarProxyService.DeserializeJSON<il>();
                var filter = request.getFilter();
                var querySql = filter.getFilterSQL();
                if (filter.tableName == "ilce")
                {
                    var res = db.ilce.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else if (filter.tableName == "mahalleKoy")
                {
                    var res = db.mahalleKoy.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else if (filter.tableName == "cadde")
                {
                    var res = db.cadde.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else if (filter.tableName == "bina")
                {
                    var res = db.bina.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else if (filter.tableName == "daire")
                {
                    var res = db.daire.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else
                {
                    var res = db.il.SqlQuery(querySql).ToList();
                    if (res.Count<10)
                    {
                        var ilData = "xMf1xgor4FG+LM9PJdCPruS0m2015111822y3yj7YxRo232eFmoz/WNhy11S9YjcSvl8NMI1rT2WwjdmPRLtK6lxgOEhjoQr+ezQ==&t=il&u=0";
                        var url = "http://adreskodu.dask.gov.tr/site-element/control/load.ashx";
                        var dres =(await TeknarProxyService.DeserializeJSON<daskResponseJSON>(url, ilData));
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }

            }

        }
    }
}
