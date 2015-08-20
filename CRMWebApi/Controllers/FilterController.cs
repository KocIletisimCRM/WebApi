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

     [RoutePrefix("api/Filter")]
    public class FilterController :ApiController
    {
         /*task tipi
          *task adı
          */
        [Route("getTasks")]
        [HttpPost]
        public HttpResponseMessage getTasks(DTOFilterGetTasksRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.task.SqlQuery(sql)
                    .Where(t => t.deleted == false)
                    .OrderBy(t => t.taskname).ToList();
                var taskTypeIds = res.Select(t => t.tasktype).Distinct().ToList();
                var tasktypes = db.tasktypes.Where(tt => taskTypeIds.Contains(tt.TaskTypeId)).ToList();
                res.ForEach(r =>{
                    r.tasktypes = tasktypes.Where(tt => tt.TaskTypeId == r.tasktype).FirstOrDefault();

                    });
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(t=>t.toDTO()), "application/json");
            }
        }
         /*site adı
          * öbek
          */
        [Route("getSite")] 

        [HttpPost]
        public HttpResponseMessage getSite(DTOFilterGetSitesRequest request) 
        {
            using (var db=new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.site.SqlQuery(sql).Where(r => r.deleted == false).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()).ToList(), "application/json");
            }

        }
        /*block adı
         *siteid 
         */
        [Route("getBlock")]
        [HttpPost]
        public HttpResponseMessage getBlock(DTOFilterGetBlockRequest request) {
            using (var db=new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.block.SqlQuery(sql).Where(b => b.deleted == false).ToList();
                return Request.CreateResponse(HttpStatusCode.OK,res.Select(r=>r.toDTO()).ToList(),"application/json");
            }
        }
         /*abone durumu adı*/
        [Route("getCustomerStatus")]
        [HttpPost]
        public HttpResponseMessage getCustomerStatus(DTOFilterGetCustomerStatusRequest request)
        {
            using (var db=new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.customer_status.SqlQuery(sql).Where(c => c.deleted == 0).OrderBy(c=>c.Text).ToList();
                return Request.CreateResponse(HttpStatusCode.OK,res.Select(r=>r.toDTO()),"application/json");
            }
        }
        /*iss durumu adı*/
         [Route("getIssStatus")]
        [HttpPost]
        public HttpResponseMessage getIssStatus(DTOFilterGetIssStatusRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.issStatus.SqlQuery(sql).Where(i => i.deleted == 0).OrderBy(i=>i.issText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r=>r.toDTO()), "application/json");
            }
        }
         /*task durumu adı
          * task durum tipi
          */
        [Route("getTaskStatus")]
        [HttpPost]
        public HttpResponseMessage getTaskStatus(DTOFilterGetTaskStatusRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.taskstatepool.SqlQuery(sql).Where(tsp => tsp.deleted == false).OrderBy(tsp => tsp.taskstate).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r=>r.toDTO()).ToList(), "application/json");
            }
        }
         /*personel adı*/
        [Route("getPersonel")]
        [HttpPost]
        public HttpResponseMessage getPersonel(DTOFilterGetPersonelRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.personel.SqlQuery(sql).Where(p => p.deleted == false).OrderBy(p => p.personelname).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s=>s.toDTO()).ToList(), "application/json");
            }
        }
       
        
    }
}