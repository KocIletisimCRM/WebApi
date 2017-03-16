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
    }
}
