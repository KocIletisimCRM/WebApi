using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Cari")]
    public class AdslCariController : ApiController
    {
        
        [Route("Bayiler")]
        [HttpGet]
        public HttpResponseMessage getCustomer()
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                var res = db.personel.Where(p => !p.email.Contains("@kociletisim.com.tr") && p.deleted==false && p.ilKimlikNo!=null).ToList();
                var ilIds = res.Select(s => s.ilKimlikNo).Distinct().ToList();
                var ils = db.il.Where(s => ilIds.Contains(s.kimlikNo)).ToList();

                var ilceIds = res.Select(s => s.ilceKimlikNo).Distinct().ToList();
                var ilces = db.ilce.Where(s => ilceIds.Contains(s.kimlikNo)).ToList();

                res.ForEach(s => {
                    s.il = ils.Where(i => i.kimlikNo == s.ilKimlikNo).FirstOrDefault();
                });
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => new {
                    s.personelid, 
                    s.personelname,
                    s.il.ad
                }), "application/json");
            }
        }
    }
}
