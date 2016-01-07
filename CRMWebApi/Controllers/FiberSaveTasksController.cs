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

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/SaveTasks")]
    public class FiberSaveTasksController : ApiController
    {
        [Route("savePenetrasyon")]
        [HttpPost]
        public HttpResponseMessage savePE(DTOSavePenetrasyon request)
        {
                using (var db = new CRMEntities())
                {
                    var taskqueue = new taskqueue
                    {
                        attachedobjectid =request.blockid,
                        attachedpersonelid = request.attachedpersonelid,
                        creationdate = request.date!=null?request.date:DateTime.Now,
                        deleted = false,
                        description = "İnternet Penetrasyon Taskı",
                        lastupdated = DateTime.Now,
                        status = null,
                        taskid = 8164,
                        updatedby = 7
                    };
                    db.taskqueue.Add(taskqueue);
                    db.SaveChanges();
                }
         
            return Request.CreateResponse(HttpStatusCode.OK,"İşlem Başarılı","application/json");
        }
    }
}
