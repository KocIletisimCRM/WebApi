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
using System.Diagnostics;

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
                var countSQL = filter.getCountSQL();

                #region scb filter düzeltmesi
                if (request.hasCSBFilter())
                {
                    var sqlPart = "(EXISTS (SELECT * from _attachedobjectid WHERE _attachedobjectid.customerid = taskqueue.attachedobjectid))";
                    querySQL = querySQL.Replace(sqlPart, "@");
                    List<string> sbcExistsClauses = new List<string>(new string[] { sqlPart });
                    if (!request.isCustomerFilter())
                    {
                        sbcExistsClauses.Add("(EXISTS (SELECT * from _blockid WHERE _blockid.blockid = taskqueue.attachedobjectid))");
                        if (!request.isBlockFilter()) sbcExistsClauses.Add("(EXISTS (SELECT * from _siteid WHERE _siteid.siteid = taskqueue.attachedobjectid))");
                    }
                    querySQL = querySQL.Replace("@", string.Format("({0})", string.Join(" OR ", sbcExistsClauses)));

                    countSQL = countSQL.Replace(sbcExistsClauses[0], string.Format("({0})", string.Join(" OR ", sbcExistsClauses)));
                }
                #endregion
                
                var perf = Stopwatch.StartNew();
                var res = db.taskqueue.SqlQuery(querySQL).ToList();
                var qd = perf.Elapsed;
                perf.Restart();
                var rowCount = db.Database.SqlQuery<int>(countSQL).First();
                var cd = perf.Elapsed;
                perf.Restart();
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

                var issIds = res.Where(i=>i.attachedcustomer!=null && i.attachedcustomer.iss != null).Select(i => i.attachedcustomer.iss).Distinct().ToList();
                var isss = db.issStatus.Where(i => issIds.Contains(i.id)).ToList();

                var cststatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.customerstatus != null).Select(c => c.attachedcustomer.customerstatus).Distinct().ToList();
                var cststatus = db.customer_status.Where(c => cststatusIds.Contains(c.ID)).ToList();

                res.ForEach(r =>
                                 {
                                     r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                                     r.attachedpersonel = personels.Where(p => p.personelid == r.attachedpersonelid).FirstOrDefault();
                                     r.attachedcustomer = customers.Where(c => c.customerid == r.attachedobjectid).FirstOrDefault();
                                     if (r.attachedcustomer == null)
                                     {
                                         r.attachedblock = blocks.Where(b => b.blockid == r.attachedobjectid).FirstOrDefault();
                                     }
                                     if(r.attachedcustomer!=null)r.attachedcustomer.issStatus =isss.Where(i => i.id == (r.attachedcustomer.iss??0)).FirstOrDefault();
                                     if (r.attachedcustomer != null) r.attachedcustomer.customer_status = cststatus.Where(c => c.ID == (r.attachedcustomer.customerstatus ?? 0)).FirstOrDefault();
                                     r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                                     r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                                     r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();

                                 });
                var ld = perf.Elapsed;
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                DTOQueryPerformance qp = new DTOQueryPerformance
                {
                    QuerSQLyDuration = qd, CountSQLDuration = cd, LookupDuration = ld
                };
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r=>r.deleted==false).Select(r => r.toDTO()).ToList(), paginginfo, querySQL, qp),
                    "application/json"
                );
            }
        }

    }
}
