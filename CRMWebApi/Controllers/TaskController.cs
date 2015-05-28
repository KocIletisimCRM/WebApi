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
		                where {1}
	                       )
	                select [taskorderno],[taskid], [previoustaskorderid], [relatedtaskorderid], [creationdate], [attachedobjectid],
		                   [attachmentdate], [attachedpersonelid], [appointmentdate], [status], [consummationdate], [description],
		                   [lastupdated], [updatedby], [deleted], [assistant_personel], [fault]
	                from tq 
	                where (Cast(rn/@rowsPerPage as int)=@pageNo-1 or @rowsPerPage = 0) ";
                List<string> withClauses = new List<string>();
                List<string> whereClauses = new List<string>();
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                string filter = null;
                if (request.filter!=null)
                {
                    TaskFilter taskFilter = new TaskFilter(request.filter.taskIds);
                    taskFilter.ApplyFilterByTaskName(request.filter.taskName)
                        .ApplyFilterByTaskTypeName(request.filter.taskTypeName)
                        .ApplyFilterByTaskTypes(request.filter.taskTypeIds);
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
                string querySQL = string.Format(sql, string.Join(",", withClauses), string.Join(" and ", whereClauses));
                var res = db.Database.SqlQuery<taskqueue>(querySQL, sqlParams)
                    //bu noktada kaldık hata veriyor include edemiyoruz
                    .Include(tq => tq.task)
                    .Include(tq => tq.taskstatepool)
                    .Include(tq => tq.attachedblock)
                    .Include(tq => tq.attachedcustomer)
                    .Include(tq => tq.Updatedpersonel)
                    .Include(tq => tq.attachedblock.site)
                    .Include(tq => tq.attachedblock)
                    .Include(tq => tq.attachedcustomer.block)
                    .Include(tq => tq.attachedcustomer.block.site)
                    .Include(tq => tq.asistanPersonel)
                    .Include(tq => tq.attachedpersonel);
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
