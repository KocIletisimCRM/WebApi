﻿using CRMWebApi.DTOs.Adsl;
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
        [Route("insertAtama")]
        [HttpPost]
        public HttpResponseMessage insertPersonel(DTOatama request)
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
    }
}