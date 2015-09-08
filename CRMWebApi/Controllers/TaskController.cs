using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs;
using System.Data.SqlClient;
using CRMWebApi.DTOs.DTORequestClasses;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Task")]
    public class TaskController : ApiController
    {
        [Route("getTaskQueues")]
        [HttpPost]
        public HttpResponseMessage getTaskQueues(DTOs.DTORequestClasses.DTOGetTaskQueueRequest request)
        {
            using (var db = new CRMEntities())
            {
                ((IPersonelRequest)request).getFilter();
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
                string querySQL = request.getFilter().getPagingSQL(request.pageNo, request.rowsPerPage);
                if (!request.isCustomerFilter())
                {
                    querySQL = querySQL.Replace("_attachedobjectid.customerid = taskqueue.attachedobjectid))",
                        "_attachedobjectid.customerid = taskqueue.attachedobjectid)) OR (EXISTS (SELECT * from _blockid WHERE _blockid.blockid = taskqueue.attachedobjectid))");
                }
                if (!request.isBlockFilter())
                {
                    querySQL = querySQL.Replace("_blockid.blockid = taskqueue.attachedobjectid))",
                        "_blockid.blockid = taskqueue.attachedobjectid)) OR (EXISTS (SELECT * from _siteid WHERE _siteid.siteid = taskqueue.attachedobjectid))");

                }

                var countSQL = filter.getCountSQL();
                if (!request.isCustomerFilter())
                {
                    countSQL = countSQL.Replace("_attachedobjectid.customerid = taskqueue.attachedobjectid))",
                        "_attachedobjectid.customerid = taskqueue.attachedobjectid)) OR (EXISTS (SELECT * from _blockid WHERE _blockid.blockid = taskqueue.attachedobjectid))");
                }
                if (!request.isBlockFilter())
                {
                    countSQL = countSQL.Replace("_blockid.blockid = taskqueue.attachedobjectid))",
                        "_blockid.blockid = taskqueue.attachedobjectid)) OR (EXISTS (SELECT * from _siteid WHERE _siteid.siteid = taskqueue.attachedobjectid))");

                }

                var res = db.taskqueue.SqlQuery(querySQL).ToList();
                var rowCount = db.Database.SqlQuery<int>(countSQL).First();
                var taskIds = res.Select(r => r.taskid).Distinct().ToList();
                var tasks = db.task.Where(t => taskIds.Contains(t.taskid)).ToList();

                var personelIds = res.Select(r => r.attachedpersonelid)
                    .Union(res.Select(r => r.assistant_personel))
                    .Union(res.Select(r => r.updatedby)).Distinct().ToList();

                var personels = db.personel.Where(p => personelIds.Contains(p.personelid)).ToList();

                var customerIds = res.Select(c => c.attachedobjectid).Distinct().ToList();
                var customers = db.customer.Include(c => c.block.site).Where(c => customerIds.Contains(c.customerid)).ToList();

                var blockIds = res.Select(b => b.attachedobjectid).Distinct().ToList();
                var blocks = db.block.Include(b => b.site).Where(b => blockIds.Contains(b.blockid)).ToList();

                var taskstateIds = res.Select(tsp => tsp.status).Distinct().ToList();
                var taskstates = db.taskstatepool.Where(tsp => taskstateIds.Contains(tsp.taskstateid)).ToList();

                res.ForEach(r =>
                                 {
                                     r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                                     r.attachedpersonel = personels.Where(p => p.personelid == r.attachedpersonelid).FirstOrDefault();
                                     r.attachedcustomer = customers.Where(c => c.customerid == r.attachedobjectid).FirstOrDefault();
                                     if (r.attachedcustomer == null)
                                     {
                                         r.attachedblock = blocks.Where(b => b.blockid == r.attachedobjectid).FirstOrDefault();
                                     }
                                     r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                                     r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                                     r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();
                                 });
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Select(r => r.toDTO()).ToList(), paginginfo, querySQL),
                    "application/json"
                );
            }
        }

    }
}
