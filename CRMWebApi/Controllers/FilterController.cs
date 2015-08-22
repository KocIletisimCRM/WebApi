// compile with: /doc:DocFileName.xml
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
    /// <summary> 
    /// Web Uygulamasındaki filtreleme bileşenlerinin verilerini çekmek için kullanılır 
    /// </summary>
     [RoutePrefix("api/Filter")]
    public class FilterController : ApiController
    {
        /// <summary> 
        /// Web Uygulamasındaki Task filtresi bileşeninin verilerini çekmek için kullanılır. <c>getTasks</c> 
        /// </summary>
        /// <param name="request">Task tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
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
                res.ForEach(r =>
                {
                    r.tasktypes = tasktypes.Where(tt => tt.TaskTypeId == r.tasktype).FirstOrDefault();

                    });
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(t => new
                {
                    t.taskid,
                    t.taskname
                }), "application/json");
            }
        }

        /// <summary> 
        /// Web Uygulamasındaki TaskType filtresi bileşeninin verilerini çekmek için kullanılır. <c>getTaskTypes</c> 
        /// </summary>
        /// <param name="request">TaskType tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getTaskTypes")]
        [HttpPost]
        public HttpResponseMessage getTaskTypes(DTOFilterGetTaskTypesRequest request) {
            using (var db=new CRMEntities())
            {
              var sql = request.Filter.getFilterSQL();
              var res = db.tasktypes.SqlQuery(sql)
                  .OrderBy(t => t.TaskTypeName).ToList();
              return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()),"application/json");
            }
        }


        /// <summary> 
        /// Web Uygulamasındaki Site filtresi bileşeninin verilerini çekmek için kullanılır. <c>getSite</c> 
        /// </summary>
        /// <param name="request">Site tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getSite")] 
        [HttpPost]
        public HttpResponseMessage getSite(DTOFilterGetSitesRequest request) 
        {
            using (var db = new CRMEntities())
            {
                var filter = new DTOFilter("site", "siteid");
                if (request.region != null) filter.fieldFilters.Add(request.region);
                if (request.siteName != null) filter.fieldFilters.Add(request.siteName);
                var sql = filter.getFilterSQL();
                var res = db.site.SqlQuery(sql).Where(r => r.deleted == false);
                if (request.siteName == null)
                    return Request.CreateResponse(HttpStatusCode.OK,
                        res.Select(r => r.region).Distinct().ToList()
                        , "application/json");
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        res.Select(r => new { r.siteid, r.sitename }).ToList()
                        , "application/json");
            }
            }

        /// <summary> 
        /// Web Uygulamasındaki block filtresi bileşeninin verilerini çekmek için kullanılır. <c>getBlock</c> 
        /// </summary>
        /// <param name="request">Block tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getBlock")]
        [HttpPost]
        public HttpResponseMessage getBlock(DTOFilterGetBlockRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.block.SqlQuery(sql).Where(b => b.deleted == false).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r=> new {r.blockid,r.blockname}).ToList()., "application/json");
            }
        }


        /// <summary> 
        /// Web Uygulamasındaki Müşteri durumu filtresi bileşeninin verilerini çekmek için kullanılır. <c>getCustomerStatus</c> 
        /// </summary>
        /// <param name="request">Customer_status tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getCustomerStatus")]
        [HttpPost]
        public HttpResponseMessage getCustomerStatus(DTOFilterGetCustomerStatusRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.customer_status.SqlQuery(sql).Where(c => c.deleted == 0).OrderBy(c => c.Text).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }


        /// <summary> 
        /// Web Uygulamasındaki iss filtresi bileşeninin verilerini çekmek için kullanılır. <c>getIssStatus</c> 
        /// </summary>
        /// <param name="request">issStatus tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getIssStatus")]
        [HttpPost]
        public HttpResponseMessage getIssStatus(DTOFilterGetIssStatusRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.issStatus.SqlQuery(sql).Where(i => i.deleted == 0).OrderBy(i => i.issText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }

        /// <summary> 
        /// Web Uygulamasındaki Task durumu filtresi bileşeninin verilerini çekmek için kullanılır. <c>getTaskStatus</c> 
        /// </summary>
        /// <param name="request">taskstatepool tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getTaskStatus")]
        [HttpPost]
        public HttpResponseMessage getTaskStatus(DTOFilterGetTaskStatusRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.taskstatepool.SqlQuery(sql).Where(tsp => tsp.deleted == false).OrderBy(tsp => tsp.taskstate).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()).ToList(), "application/json");
            }
        }

        /// <summary> 
        /// Web Uygulamasındaki personel  filtresi bileşeninin verilerini çekmek için kullanılır. <c>getPersonel</c> 
        /// </summary>
        /// <param name="request">personel tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getPersonel")]
        [HttpPost]
        public HttpResponseMessage getPersonel(DTOFilterGetPersonelRequest request)
        {
            using (var db = new CRMEntities())
            {
                var sql = request.Filter.getFilterSQL();
                var res = db.personel.SqlQuery(sql).Where(p => p.deleted == false).OrderBy(p => p.personelname).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
            }
        }
    }
}