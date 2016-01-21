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
    [RoutePrefix("api/Fiber/Taskstatematches")]
    public class FiberTaskStateMatchesController : ApiController
    {
        [Route("getTaskStateMatches")]
        [HttpPost]
        public HttpResponseMessage getTaskStateMatches(DTOGetTSMFilter request)
        {

            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();

                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                var res = db.taskstatematches.SqlQuery(querySql).ToList();
                var taskids = res.Select(s => s.taskid).Distinct().ToList();
                var tasks = db.task.Where(t => taskids.Contains(t.taskid)).ToList();

                var stateids = res.Select(s => s.stateid).Distinct().ToList();
                var states = db.taskstatepool.Where(tsp => stateids.Contains(tsp.taskstateid)).ToList();
                res.ForEach(r => {
                    r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                    r.taskstatepool = states.Where(t => t.taskstateid == r.stateid).FirstOrDefault();
                });

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo)
                    , "application/json");

            }
        }

        [Route("saveTaskStateMatches")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage saveTaskStateMatches(DTOtaskstatematches request)
        {
            using (var db = new CRMEntities())
            {
                var dtsm = db.taskstatematches.Where(t => t.id == request.id).FirstOrDefault();
                dtsm.taskid = request.task.taskid;
                dtsm.stateid = request.taskstatepool.taskstateid;
                dtsm.automandatorytasks = request.automandatorytasks;
                dtsm.autooptionaltasks = request.autooptionaltasks;
                dtsm.stockcards = request.stockcards;
                dtsm.documents = request.documents;
                dtsm.lastupdated = DateTime.Now;
                dtsm.updatedby = KOCAuthorizeAttribute.getCurrentUser().userId;
                db.SaveChanges();
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertTaskStateMatches")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage insertTaskState(DTOtaskstatematches request)
        {
            using (var db = new CRMEntities())
            {
                var tsm = new taskstatematches
                {
                    taskid = request.task.taskid,
                    stateid = request.taskstatepool.taskstateid,
                    automandatorytasks = request.automandatorytasks,
                    autooptionaltasks = request.autooptionaltasks,
                    stockcards = request.stockcards,
                    documents = request.documents,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    updatedby = KOCAuthorizeAttribute.getCurrentUser().userId,
                    deleted = false
                };
                db.taskstatematches.Add(tsm);
                db.SaveChanges();
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
    }
}
