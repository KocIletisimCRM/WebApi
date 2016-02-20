using CRMWebApi.DTOs.Adsl;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Atama")]
    public class AdslAtamaController : ApiController
    {
        [Route("insertPersonelAtama")]
        [HttpPost]
        public HttpResponseMessage insertPersonelAtama(DTOatama request)
        {
            using (var db = new KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                    var p = new atama
                    {
                        closedtasktype = request.closedtasktype,
                        closedtask = request.closedtask,
                        offpersonel = request.offpersonel,
                        formedtasktype = request.formedtasktaype,
                        formedtask = request.formedtask,
                        appointedpersonel = request.appointedpersonel,
                    };
                    db.atama.Add(p);
                    db.SaveChanges();
                    tran.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "Hata" };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
        }
        [Route("updatePersonelAtama")]
        [HttpPost]
        public HttpResponseMessage updatePersonelAtama(DTOatama request)
        {
            using (var db = new KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var upa = db.atama.Where(r => r.id == request.id).FirstOrDefault();
                    upa.closedtasktype = request.closedtasktype;
                    upa.closedtask = request.closedtask;
                    upa.offpersonel = request.offpersonel;
                    upa.formedtasktype = request.formedtasktaype;
                    upa.formedtask = request.formedtask;
                    upa.appointedpersonel = request.appointedpersonel;
                    db.SaveChanges();
                    tran.Commit();
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "Atama Gerçekleştirildi." };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "Atama Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                }
        }
        [Route("deletePersonelAtama")]
        [HttpPost]
        public HttpResponseMessage deletePersonelAtama(int id)
        {
            using (var db = new KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var upa = db.atama.Where(r => r.id == id).FirstOrDefault();
                    //delete
                    db.SaveChanges();
                    tran.Commit();
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "işlem Gerçekleştirildi." };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "işlem Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                }
        }
    }
}