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
     [RoutePrefix("api/TaskQueueFilter")]
    public class TaskQueueFilterController :ApiController
    {
        [Route("getTasks")]
        [HttpPost]
        public HttpResponseMessage getTasks()
        {
            using (var db = new CRMEntities())
            {
                var res = db.task.Include(t=>t.tasktypes)
                    .Where(t => t.deleted == false)
                    .OrderBy(t => t.taskname).ToList()
                    .Select(t=>t.toDTO()).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, res, "application/json");
            }
        }

        [Route("getSite")]
        [HttpPost]
        public HttpResponseMessage getSite() {

            using (var db=new CRMEntities())
            {
                var res = db.site.Where(s => s.deleted == false).OrderBy(s => s.sitename).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK,res,"application/json");
            }
        }
        [Route("getCustomerStatus")]
        [HttpPost]
        public HttpResponseMessage getCustomerStatus()
        {
            using (var db=new CRMEntities())
            {
                var res = db.customer_status.Where(c => c.deleted == 0).OrderBy(c=>c.Text).ToList();
                return Request.CreateResponse(HttpStatusCode.OK,res,"application/json");
            }
        }
        [Route("getIssStatus")]
        [HttpPost]
        public HttpResponseMessage getIssStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.issStatus.Where(i => i.deleted == 0).OrderBy(i=>i.issText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res, "application/json");
            }
        }
        [Route("getTaskStatus")]
        [HttpPost]
        public HttpResponseMessage getTaskStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.taskstatepool.Where(tsp => tsp.deleted == false).OrderBy(tsp => tsp.taskstate).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res, "application/json");
            }
        }
        [Route("getPersonel")]
        [HttpPost]
        public HttpResponseMessage getPersonel()
        {
            using (var db = new CRMEntities())
            {
                var res = db.personel.Where(p => p.deleted == false).OrderBy(p => p.personelname).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res, "application/json");
            }
        }


    }
}