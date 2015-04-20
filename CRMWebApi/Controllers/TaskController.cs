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
                DTOs.TaskFilter taskFilter = new DTOs.TaskFilter(request.filter.TaskIds);
                string filter = taskFilter.ApplyFilterByTaskName(request.filter.TaskName)
                    .ApplyFilterByTaskTypeName(request.filter.TaskTypeName)
                    .ApplyFilterByTaskTypes(request.filter.TaskTypeIds).getFilterXML();
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
        //public HttpResponseMessage _getTaks([FromUri]int pageNo, [FromUri]int rowsPerPage, [FromUri]bool isOpen )
        //{
        //    return getTaks(pageNo, rowsPerPage,new Dictionary<string,string>());
        //}


    }
}
