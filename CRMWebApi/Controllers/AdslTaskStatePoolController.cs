using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/TaskStatePool")]
    public class AdslTaskDefinationController : ApiController
    {
        #region Task Durum Havuzu
        [Route("getTaskState")]
        [HttpPost]
        public HttpResponseMessage getTaskstate(DTOGetTSPFilter request)
        {
            using (var db = new KOCSAMADLSEntities())
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
                var res = db.taskstatepool.SqlQuery(querySql).OrderBy(o => o.taskstateid).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo), "application/json");

            }
        }

        [Route("saveTaskState")]
        [HttpPost]
        public HttpResponseMessage saveTaskstate(DTOtaskstatepool request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var dtsp = db.taskstatepool.Where(t => t.taskstateid == request.taskstateid).FirstOrDefault();
                dtsp.taskstate = request.taskstate;
                dtsp.statetype = request.statetype;
                dtsp.lastupdated = DateTime.Now;
                dtsp.updatedby = 7;
                db.SaveChanges();
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertTaskState")]
        [HttpPost]
        public HttpResponseMessage insertTaskState(DTOtaskstatepool request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var tsp = new adsl_taskstatepool
                {
                    taskstate = request.taskstate,
                    statetype = request.statetype,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    updatedby = 7,
                    deleted = false
                };
                db.taskstatepool.Add(tsp);
                db.SaveChanges();
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        #endregion
    }
}
