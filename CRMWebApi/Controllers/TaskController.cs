using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

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
                string sql = @"with
	taskTypeIds as (
		select x.t.value('data(.)', 'int') id 
		from( Select CAST(@taskFilter as XML) as ids ) t
		cross apply t.ids.nodes('/id') x(t)	
	),
	tsk as (
		select row_number() over(order by taskid) rn, * from task
		Where (@taskFilter is null or Exists(select * from taskTypeIds t where t.id = tasktype))
	)
	SELECT taskid, taskname, tasktype, attachablepersoneltype, attachableobjecttype, performancescore, creationdate, lastupdated, updatedby, deleted
	FROM tsk
	where (Cast(rn/IIF(@rowsPerPage=0, 1, @rowsPerPage) as int)=@pageNo-1 or @rowsPerPage = 0) ";

                string filter = null;
                if (request.filter!=null)
                {
                    DTOs.TaskFilter taskFilter = new DTOs.TaskFilter(request.filter.TaskIds);
                     filter = taskFilter.ApplyFilterByTaskName(request.filter.TaskName)
                        .ApplyFilterByTaskTypeName(request.filter.TaskTypeName)
                        .ApplyFilterByTaskTypes(request.filter.TaskTypeIds).getFilterXML();
                }
                
                var res = db.sf_taskqueue(request.pageNo, request.rowsPerPage, filter)
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
