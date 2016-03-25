using CRMWebApi.DTOs.Fiber;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using CRMWebApi.Models.Fiber;
using CRMWebApi.KOCAuthorization;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/Task")]
    public class FiberTaskController : ApiController
    {
        #region Task Tanımlamaları Sayfası
        [Route("getTaskList")]
        [HttpPost]
        public HttpResponseMessage getTaskList(DTOFilterGetTasksRequest request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                filter.subTables["taskid"].fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });

                var querySql = filter.subTables["taskid"].getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.task.SqlQuery(querySql).ToList();
                //var ptypeids = res.Select(r => r.attachablepersoneltype).Distinct().ToList();
                //var personels = db.objecttypes.Where(p => ptypeids.Contains(p.typeid)).ToList();

                var obtpyeids = res.Select(r => r.attachableobjecttype).Distinct().ToList();
                var objects = db.objecttypes.Where(o => obtpyeids.Contains(o.typeid)).ToList();

                var ttypeids = res.Select(s => s.tasktype).Distinct().ToList();
                var tasktypess = db.tasktypes.Where(t => ttypeids.Contains(t.TaskTypeId)).ToList();
                res.ForEach(r =>
                {
                    r.objecttypes = objects.Where(t => t.typeid == r.attachableobjecttype).FirstOrDefault();
                    r.tasktypes = tasktypess.Where(t => t.TaskTypeId == r.tasktype).FirstOrDefault();
                });


                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");

            }
        }

        [Route("saveTask")]
        [HttpPost]
        public HttpResponseMessage saveTask(DTOtask request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var dt = db.task.Where(t => t.taskid == request.taskid).FirstOrDefault();
                var errormessage = new DTOResponseError();
                dt.description = request.description;
                dt.taskname = request.taskname;
                dt.performancescore = request.performancescore;
                dt.tasktype = request.tasktypes.TaskTypeId;
                dt.attachableobjecttype = request.objecttypes.typeid;
                dt.attachablepersoneltype = request.personeltypes.typeid;
                dt.updatedby = user.userId;
                dt.lastupdated = DateTime.Now;
                db.SaveChanges();
                errormessage.errorCode = 1;
                errormessage.errorMessage = "İşlem Başarılı";
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        [Route("insertTask")]
        [HttpPost]
        public HttpResponseMessage insertTask(DTOtask request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError();
                var t = new task
                {
                    taskname = request.taskname,
                    attachableobjecttype = request.objecttypes.typeid,
                    attachablepersoneltype = request.personeltypes.typeid,
                    performancescore = request.performancescore,
                    tasktype = request.tasktypes.TaskTypeId,
                    deleted = false,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    updatedby = user.userId,
                    description = request.description,
                };
                db.task.Add(t);
                db.SaveChanges();
                errormessage.errorCode = 1;
                errormessage.errorMessage = "İşlem Başarılı";
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");

            }
        }
        #endregion
    }
}
