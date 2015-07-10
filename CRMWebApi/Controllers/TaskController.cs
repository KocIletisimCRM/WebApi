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

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Task")]
    public class TaskController : ApiController
    {
        [Route("getTasks")]
        [HttpPost]
        public HttpResponseMessage getTaskQueues(DTOs.DTOGetTaskQueueRequest request)
        {
            using (var db = new CRMEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                string xmlToTable = @"
                    Select x.t.value('data(.)', 'int') id 
                    From (Select Cast(@{0} as XML) as ids) t
                    Cross Apply t.ids.nodes('/id') x(t)
                "; 
                string sql = @"
                  with
                    {0}
        	         tq as (
		                select row_number() over(order by taskorderno) rn, * from taskqueue tq
		                where 1=1 and {1}
	                       )
	                select [taskorderno],[taskid], [previoustaskorderid], [relatedtaskorderid], [creationdate], [attachedobjectid],
		                   [attachmentdate], [attachedpersonelid], [appointmentdate], [status], [consummationdate], [description],
		                   [lastupdated], [updatedby], [deleted], [assistant_personel], [fault]
	                from tq 
	                where (Cast(rn/{2} as int)={3}-1 or {2} = 0) ";
                List<string> withClauses = new List<string>();
                List<string> whereClauses = new List<string>();
                whereClauses.Add("1=1");
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                string filter = null;
                if (request.filter!=null)
                {
                    TaskFilter taskFilter = new TaskFilter(request.filter.taskFilter.Ids);
                    taskFilter.ApplyFilterByTaskName(request.filter.taskFilter.Name)
                        .ApplyFilterByTaskTypeName(request.filter.taskFilter.TypeName)
                        .ApplyFilterByTaskTypes(request.filter.taskFilter.Ids);
                    string taskIdsXML = taskFilter.getFilterXML();
                    if (taskIdsXML != null) sqlParams.Add(new SqlParameter("taskid_Filter", taskIdsXML));
                    foreach (var param in sqlParams)
                    {
                        withClauses.Add(string.Format(" {0} as ({1})", param.ParameterName, string.Format(xmlToTable, param.ParameterName)));
                        whereClauses.Add(string.Format(" Exists (Select * from {0} ids Where ids.id = tq.{1}) ", param.ParameterName, param.ParameterName.Split('_')[0]));
                    }
                    //Date filtereleri eklenecek
                }
                sqlParams.Add(new SqlParameter("pageNo", request.pageNo));
                sqlParams.Add(new SqlParameter("rowsPerPage", request.rowsPerPage));
                string querySQL = string.Format(sql, string.Join(",", withClauses), string.Join(" and ", whereClauses), request.rowsPerPage, request.pageNo);

                var res = db.taskqueue.SqlQuery(querySQL).ToList();
                var taskIds = res.Select(r => r.taskid).Distinct().ToList();
                var tasks = db.task.Where(t => taskIds.Contains(t.taskid)).ToList();

                var personelIds = res.Select(r => r.attachedpersonelid)
                    .Union(res.Select(r=>r.assistant_personel))
                    .Union(res.Select(r=>r.updatedby)).Distinct().ToList();

                var personels = db.personel.Where(p=>personelIds.Contains(p.personelid)).ToList();

                var customerIds = res.Select(c => c.attachedobjectid).Distinct().ToList();
                var customers=db.customer.Include(c=>c.block.site).Where(c=>customerIds.Contains(c.customerid)).ToList();
                
                var blockIds=res.Select(b=>b.attachedobjectid).Distinct().ToList();
                var blocks = db.block.Include(b=>b.site).Where(b => blockIds.Contains(b.blockid)).ToList();
               
                var taskstateIds = res.Select(tsp=>tsp.status).Distinct().ToList();
                var taskstates = db.taskstatepool.Where(tsp => taskstateIds.Contains(tsp.taskstateid)).ToList();

                res.ForEach(r =>
                {
                    r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                    r.attachedpersonel = personels.Where(p => p.personelid == r.attachedpersonelid).FirstOrDefault();
                    r.attachedcustomer = customers.Where(c => c.customerid == r.attachedobjectid).FirstOrDefault();
                    if (r.attachedcustomer==null)
                    {
                        r.attachedblock = blocks.Where(b => b.blockid == r.attachedobjectid).FirstOrDefault();
                    }
                    r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                    r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                    r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();
                });
                return Request.CreateResponse(HttpStatusCode.OK,
                    res.ToList().Select(tq => tq.toDTO()).ToList(),
                    "application/json"
                );
            }
        }

        //[Route("getTasks")]
        //[HttpGet]
        //public HttpResponseMessage _getTaks([FromUri]int pageNo)
        //{
        //    return getTaskQueues(pageNo, rowsPerPage);
        //}


    }
}
