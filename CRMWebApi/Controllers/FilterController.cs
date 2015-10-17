﻿// compile with: /doc:DocFileName.xml
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
                 var filter = request.getFilter();
                 if (request.isTaskFilter())
                     return Request.CreateResponse(HttpStatusCode.OK, db.task.SqlQuery(filter.subTables["taskid"].getFilterSQL())
                             .OrderBy(t => t.taskname)
                             .Select(t => new
                                     {
                                         t.taskid,
                                         t.taskname
                                     }).ToList(), "application/json");
                 if (request.isTaskstateFilter())
                 {
                     var acik = new taskstatepool { taskstate = "AÇIK", taskstateid = 0 };
                     var tspIds =  db.taskstatematches.SqlQuery(filter.getFilterSQL())
                         .Select(t => t.stateid).ToList();
                     var res = db.taskstatepool.Where(tsp => tspIds.Contains(tsp.taskstateid)).OrderBy(tsp => tsp.taskstate).ToList();
                     res.Insert(0, acik);
                     return Request.CreateResponse(HttpStatusCode.OK, res.Select(r=>r.toDTO()).ToList(), "application/json");
                 }
                 return Request.CreateResponse(HttpStatusCode.OK,
                     db.tasktypes.SqlQuery(filter.subTables["tasktype"].getFilterSQL())
                         .Select(tt => new { tt.TaskTypeId, tt.TaskTypeName })
                         .OrderBy(tt => tt.TaskTypeName).ToList(),
                     "application/json"
                 );
             }
         }

        /// <summary> 
        /// Web Uygulamasındaki Site filtresi bileşeninin verilerini çekmek için kullanılır. <c>getSite</c> 
        /// </summary>
        /// <param name="request">Site tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getCSB")]
        [HttpPost]
        public HttpResponseMessage getCSB(DTOFilterGetCSBRequest request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                if (request.isCustomerFilter())
                    return Request.CreateResponse(HttpStatusCode.OK,
                                db.customer.SqlQuery(filter.getFilterSQL())
                                    .Where(r => r.deleted == false)
                                    .Select(r => new {r.customerid, r.customername, r.customersurname })
                                    .ToList(),
                                "application/json");

                else if (request.isBlockFilter())
                    return Request.CreateResponse(HttpStatusCode.OK,
                               db.block.SqlQuery(filter.subTables["blockid"].getFilterSQL())
                                   .Where(r => r.deleted == false)
                                   .Select(r => new { r.blockid, r.blockname })
                                   .OrderBy(r => r.blockname).ToList(),
                               "application/json");
                else if (request.isSiteFilter())
                    return Request.CreateResponse(HttpStatusCode.OK,
                               db.site.SqlQuery(filter.subTables["blockid"].subTables["siteid"].getFilterSQL())
                                    .Where(r => r.deleted == false)
                                    .Select(r => new { r.siteid, r.sitename }).OrderBy(r => r.sitename).ToList()
                                      , "application/json");

                    return Request.CreateResponse(HttpStatusCode.OK,
                                 db.site.SqlQuery(filter.subTables["blockid"].subTables["siteid"].getFilterSQL())
                                         .Where(r => r.deleted == false)
                                         .Select(r => new { r.region }).Distinct().OrderBy(r => r.region).ToList()
                                  , "application/json");
            }
        }
         

        /// <summary> 
        /// Web Uygulamasındaki Müşteri durumu filtresi bileşeninin verilerini çekmek için kullanılır. <c>getCustomerStatus</c> 
        /// </summary>
        /// <param name="request">Customer_status tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getCustomerStatus")]
        [HttpPost]
        public HttpResponseMessage getCustomerStatus()
        {
            using (var db = new CRMEntities())
            {
             
                var res = db.customer_status.Where(c => c.deleted == 0).OrderBy(c => c.Text).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }


        /// <summary> 
        /// Web Uygulamasındaki iss filtresi bileşeninin verilerini çekmek için kullanılır. <c>getIssStatus</c> 
        /// </summary>
        /// <param name="request">issStatus tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getIssStatus")]
        [HttpPost]
        public HttpResponseMessage getIssStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.issStatus.Where(i => i.deleted == 0).OrderBy(i => i.issText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }

       

        /// <summary> 
        /// Web Uygulamasındaki personel  filtresi bileşeninin verilerini çekmek için kullanılır. <c>getPersonel</c> 
        /// </summary>
        /// <param name="request">personel tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getPersonel")]
        [HttpPost]
        public HttpResponseMessage getPersonel()
        {
            using (var db = new CRMEntities())
            {
                var atanmamis = new personel {personelid=0,personelname="Atanmamış" };
                var res = db.personel.Where(p => p.deleted == false).OrderBy(p => p.personelname).ToList();
                res.Insert(0, atanmamis);
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
            }
        }

        /// <summary> 
        /// Web Uygulamasındaki kampanya  seçenekleri bileşeninin verilerini çekmek için kullanılır. 
        /// </summary>
        /// <param name="request">kategori alt kategori ve ürün tanımlarını içerir</param>
        [Route("getCampaignInfo")]
        [HttpPost]
        public HttpResponseMessage getCampaignInfo(DTOs.DTORequestClasses.DTOFiterGetCampaignRequst request)
        {
            var filter = request.getFilter();
            filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
           
            using (var db = new CRMEntities())
            {
                //var p = db.campaigns.Where(c => c.id == 6080).FirstOrDefault();
                //List<int> productids = new List<int>();
                //foreach (var item in p.products.Split(',').ToList())
                //{
                //    productids.Add(Convert.ToInt32(item));
                //}
                //var products = db.product_service.Where(pp => productids.Contains(pp.productid)).ToList();
                //return Request.CreateResponse(HttpStatusCode.OK, products.Select(s => new {s.productname,s.productid }), "application/json");              

                if (request.isCategoryFilter())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, db.campaigns.SqlQuery(filter.getFilterSQL()).Select(tt => new { tt.category }).Distinct().OrderBy(t => t.category).ToList(), "application/json");
                }
                else if (request.isSubcategoryFilter())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, db.campaigns.SqlQuery(filter.getFilterSQL()).Select(tt => new { tt.subcategory }).Distinct().OrderBy(t => t.subcategory).ToList(), "application/json");
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, db.campaigns.SqlQuery(filter.getFilterSQL()).Select(tt => new { tt.name,tt.id }).OrderBy(t => t.name).ToList(), "application/json");              
            }
        }
    }
}