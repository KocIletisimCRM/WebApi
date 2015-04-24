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
                
                string filter = null;
                if (request.filter!=null)
                {
                    DTOs.TaskFilter taskFilter = new DTOs.TaskFilter(request.filter.taskIds);
                     filter = taskFilter.ApplyFilterByTaskName(request.filter.taskName)
                        .ApplyFilterByTaskTypeName(request.filter.taskTypeName)
                        .ApplyFilterByTaskTypes(request.filter.taskTypeIds).getFilterXML();
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
