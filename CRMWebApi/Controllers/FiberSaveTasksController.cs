using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs.Fiber;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using System.Diagnostics;
using CRMWebApi.Models.Fiber;
using CRMWebApi.KOCAuthorization;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/SaveTasks")]
    public class FiberSaveTasksController : ApiController
    {
        [Route("savePenetrasyon")]
        [HttpPost]
        public HttpResponseMessage savePE(DTOSavePenetrasyon request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var taskqueue = new taskqueue
                {
                    attachedobjectid = request.blockid,
                    attachedpersonelid = request.attachedpersonelid,
                    creationdate = request.date != null ? request.date : DateTime.Now,
                    deleted = false,
                    description = "İnternet Penetrasyon Taskı",
                    lastupdated = DateTime.Now,
                    status = null,
                    taskid = 8164,
                    updatedby = user.userId
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.03.2017
                db.SaveChanges();
            }

            return Request.CreateResponse(HttpStatusCode.OK, "İşlem Başarılı", "application/json");
        }

        [Route("saveGlobalTask")]
        [HttpPost]
        public HttpResponseMessage savetask(DTOSaveGlobalTask request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                if (request.newcustomer)
                { // işlem eğer sadece mobil satış işlemi ise 
                    var c = db.customer.First(r => r.customerid == request.customerid);
                    var cs = new customer
                    {
                        blockid = c.blockid,
                        customername = request.customername,
                        creationdate = DateTime.Now,
                        description = "Hat Satışı için Oluşturuldu",
                        flat = c.flat,
                        gsm = request.gsm,
                        lastupdated = DateTime.Now,
                        tckimlikno = request.tc,
                        updatedby = user.userId
                    };
                    db.customer.Add(cs);
                    db.SaveChanges();
                    request.customerid = cs.customerid;
                }
                var taskqueue = new taskqueue
                {
                    taskid = request.taskid,
                    creationdate = request.creationdate != null ? request.creationdate : DateTime.Now,
                    attachedobjectid = request.customerid != null ? request.customerid : request.blockid,
                    attachmentdate = request.attachedpersonelid != null ? DateTime.Now : (DateTime?)null,
                    attachedpersonelid = request.attachedpersonelid,
                    description = request.description,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    fault = request.fault,
                    updatedby = user.userId
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.03.2017
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
            }
        }

        [Route("saveTtvTask"), HttpPost]
        public HttpResponseMessage saveTtvTask(DTONewTTVTask request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var c = db.customer.First(r => r.customerid == request.customerid);
                var cs = new customer
                {
                    superonlineCustNo = request.customer.superonlineCustNo,
                    tckimlikno = request.customer.tckimlikno,
                    customername = request.customer.customername,
                    gsm = request.customer.gsm,
                    phone = request.customer.phone,
                    customerstatus = request.customer.customerstatus,
                    iss = request.customer.iss,
                    netstatu = request.customer.netstatu,
                    telstatu = request.customer.telstatu,
                    tvstatu = request.customer.tvstatu,
                    turkcellTv = request.customer.turkcellTv,
                    gsmstatu = request.customer.gsmstatu,
                    description = request.customer.description,
                    blockid = c.blockid,
                    flat = c.flat,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    updatedby = user.userId,
                    deleted = false
                };
                db.customer.Add(cs);
                c.deleted = null;
                db.SaveChanges();
                var taskqueue = new taskqueue
                {
                    taskid = request.taskid,
                    creationdate = request.creationdate != null ? request.creationdate : DateTime.Now,
                    attachedobjectid = cs.customerid,
                    attachmentdate = request.attachedpersonelid != null ? DateTime.Now : (DateTime?)null,
                    attachedpersonelid = request.attachedpersonelid,
                    description = request.description,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = user.userId
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.03.2017
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
            }
        }
    }
}
