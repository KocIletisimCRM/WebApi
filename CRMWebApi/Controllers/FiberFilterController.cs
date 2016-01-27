// compile with: /doc:DocFileName.xml
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs.Fiber;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using CRMWebApi.Models.Fiber;

namespace CRMWebApi.Controllers
{
    /// <summary> 
    /// Web Uygulamasındaki filtreleme bileşenlerinin verilerini çekmek için kullanılır 
    /// </summary>
    [RoutePrefix("api/Fiber/Filter")]
    public class FiberFilterController : ApiController
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
                {
                    filter.subTables["taskid"].fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                    return Request.CreateResponse(HttpStatusCode.OK, db.task.SqlQuery(
                       filter.subTables["taskid"].getFilterSQL())
                           .OrderBy(t => t.taskname)
                           .Select(t => new
                           {
                               t.taskid,
                               t.taskname,
                               t.attachableobjecttype
                           }).ToList(), "application/json");
                }
                    
                 if (request.isTaskstateFilter())
                 {
                     var acik = new taskstatepool { taskstate = "AÇIK", taskstateid = 0 };
                    var query = filter.getFilterSQL();
                    var tspIds =  db.taskstatematches.SqlQuery(query)
                         .Select(t => t.stateid).ToList();

                     var res = db.taskstatepool.Where(tsp => tspIds.Contains(tsp.taskstateid) && tsp.deleted==false).OrderBy(tsp => tsp.taskstate).ToList();
                     res.Insert(0, acik);
                     return Request.CreateResponse(HttpStatusCode.OK, res.Select(r=>r.toDTO()).ToList(), "application/json");
                 }
                if (request.isObjecttypeFilter())
                {
                    var objids = db.task.SqlQuery(filter.subTables["taskid"].getFilterSQL()).Select(s => s.attachableobjecttype).Distinct().ToList();
                    var res = db.objecttypes.Where(o => objids.Contains(o.typeid))
                               .OrderBy(t => t.typname)
                               .ToList();
                    return Request.CreateResponse(HttpStatusCode.OK,res.Select(s=> new { s.typeid, s.typname }).ToList() , "application/json");
                }
                if (request.isPersonelTypeFilter())
                {
                    var pids= db.task.SqlQuery(filter.subTables["taskid"].getFilterSQL()).Select(s => s.attachablepersoneltype).Distinct().ToList();
                    var res = db.objecttypes.Where(p => pids.Contains(p.typeid)).OrderBy(p => p.typname).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.Select(t => new
                               {
                                   t.typeid,
                                   t.typname
                               }).ToList(), "application/json");
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
                                    .Select(r => new {r.customerid, r.customername, r.customersurname,r.flat})
                                    .ToList(),
                                "application/json");

                else if (request.isBlockFilter())
                    return Request.CreateResponse(HttpStatusCode.OK,
                               db.block.SqlQuery(filter.subTables["blockid"].getFilterSQL())
                                   .Where(r => r.deleted == false)
                                   .Select(r => new { r.blockid, r.blockname ,r.hp})
                                   .OrderBy(r => r.blockname).ToList(),
                               "application/json");
                else if (request.isSiteFilter())
                    return Request.CreateResponse(HttpStatusCode.OK,
                               db.site.SqlQuery(filter.subTables["blockid"].subTables["siteid"].getFilterSQL())
                                    .Where(r => r.deleted == false)
                                    .Select(r => new { r.siteid, r.sitename,r.siteaddress }).OrderBy(r => r.sitename).ToList()
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
        /// Web Uygulamasındaki iss filtresi bileşeninin verilerini çekmek için kullanılır. <c>getIssStatus</c> 
        /// </summary>
        /// <param name="request">issStatus tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getNetStatus")]
        [HttpPost]
        public HttpResponseMessage getNetStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.netStatus.Where(i => i.deleted == 0).OrderBy(i => i.netText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }
        /// <summary> 
        /// Web Uygulamasındaki iss filtresi bileşeninin verilerini çekmek için kullanılır. <c>getIssStatus</c> 
        /// </summary>
        /// <param name="request">issStatus tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getTelStatus")]
        [HttpPost]
        public HttpResponseMessage getTelStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.telStatus.Where(i => i.deleted == 0).OrderBy(i => i.telText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }
        /// <summary> 
        /// Web Uygulamasındaki iss filtresi bileşeninin verilerini çekmek için kullanılır. <c>getIssStatus</c> 
        /// </summary>
        /// <param name="request">issStatus tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getTvKullanımıStatus")]
        [HttpPost]
        public HttpResponseMessage getTvKullanımıStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.TvKullanımıStatus.Where(i => i.deleted == 0).OrderBy(i => i.tvKullanımıText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }
        /// <summary> 
        /// Web Uygulamasındaki iss filtresi bileşeninin verilerini çekmek için kullanılır. <c>getIssStatus</c> 
        /// </summary>
        /// <param name="request">issStatus tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getTurkcellTvStatus")]
        [HttpPost]
        public HttpResponseMessage getTurkcellTvStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.TurkcellTVStatus.Where(i => i.deleted == 0).OrderBy(i => i.TurkcellTvText).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r => r.toDTO()), "application/json");
            }
        }
        /// <summary> 
        /// Web Uygulamasındaki iss filtresi bileşeninin verilerini çekmek için kullanılır. <c>getIssStatus</c> 
        /// </summary>
        /// <param name="request">issStatus tablosu satırlarının hangilerinin Web filtre bileşeninde görüneceğni belirler</param>
        [Route("getGsmStatus")]
        [HttpPost]
        public HttpResponseMessage getGsmStatus()
        {
            using (var db = new CRMEntities())
            {
                var res = db.gsmKullanımıStatus.Where(i => i.deleted == 0).OrderBy(i => i.gsmText).ToList();
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

        [Route("getAttacheablePersonel")]
        [HttpPost]
    
        public HttpResponseMessage getPersonel(DTOTest request)
        {
            using (var db = new CRMEntities())
            {
                var task = db.taskqueue.Where(t => t.taskorderno == request.taskorderno).Select(s => s.taskid).FirstOrDefault();
                var objects = db.task.Where(s => s.taskid == task).Select(s => s.attachablepersoneltype).FirstOrDefault();
                var res = db.personel.Where(p => (p.roles & objects) == objects && p.deleted==false).OrderBy(o => o.personelname).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
            }
        }

        /// <summary> 
        /// Web Uygulamasındaki kampanya  seçenekleri bileşeninin verilerini çekmek için kullanılır. 
        /// </summary>
        /// <param name="request">kategori alt kategori ve ürün tanımlarını içerir</param>
        [Route("getCampaignInfo")]
        [HttpPost]
        public HttpResponseMessage getCampaignInfo(DTOFiterGetCampaignRequst request)
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
                else if (request.isCampaignFilter())
                    return Request.CreateResponse(HttpStatusCode.OK, db.campaigns.SqlQuery(filter.getFilterSQL()).Select(tt => new { tt.name, tt.id }).OrderBy(t => t.name).ToList(), "application/json");
                else
                {
                    var cids = db.campaigns.SqlQuery(filter.getFilterSQL()).Select(s => s.id).ToList();
                       var pids =db.vcampaignproducts.Where(v=>cids.Contains(v.cid)).Select(s=>s.pid).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK,
                        db.product_service.Where(p => pids.Contains(p.productid)).OrderBy(s => s.category).ToList().GroupBy(g => g.category, g => g.toDTO()).Select(g => new { category = g.Key, products = g })
                        , "application/json");

                }
            }
        }

        [Route("getProductList")]
        [HttpPost]
        public HttpResponseMessage getProduct()
        {
            using (var db = new CRMEntities())
            {

                var res = db.product_service.Where(p => p.deleted == false).OrderBy(o => o.productname).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
            }
        }

        [Route("getObjectType")]
        [HttpPost]
        public HttpResponseMessage getObjectType()
        {
            using (var db=new CRMEntities())
            {
                var res = db.objecttypes.Select(s => new {s.typeid,s.typname }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK,res,"application/json");
            }
        }

        [Route("getObject")]
        [HttpPost]
        public HttpResponseMessage getObject(DTOGetObjectRequest request)
        {
            using (var db=new CRMEntities())
            {
                var filter = request.getFilter();
                var querySql = filter.getFilterSQL();
                var res = new List<idName>();
                if (filter.tableName == "block")
                    res.AddRange(db.block.SqlQuery(querySql).Select(b=> new idName { id = b.blockid, name = b.blockname}));
                else if (filter.tableName == "site")
                    res.AddRange(db.site.SqlQuery(querySql).Select(s => new idName { id = s.siteid, name = s.sitename }));
                else if (filter.tableName == "customer")
                    res.AddRange(db.customer.SqlQuery(querySql).Select(c => new idName { id = c.customerid, name = $"{c.customername} {c.customersurname}" }).Take(10));
                else
                    res.AddRange(db.personel.SqlQuery(querySql).Select(p => new idName { id = p.personelid, name = p.personelname }));
                return Request.CreateResponse(HttpStatusCode.OK,res.Select(r=>r.toDTO()),"application/json");
            }
        }

        [Route("getPersonelStock")]
        [HttpPost]
        public HttpResponseMessage getPersonelStock(DTOGetPersonelStock request)
        {
            using (var db = new CRMEntities())
            {
                var res = db.getPersonelStockFiber(request.personelid).ToList();           
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(r=> new {r.productname,r.stockid,r.amount }), "application/json");
            }
        }

        [Route("getSerialsOnPersonel")]
        [HttpPost]
        public HttpResponseMessage getSerialsOnPersonel(DTOGetSerialOnPersonel request)
        {
            using (var db = new CRMEntities())
            {
                var res = db.getSerialsOnPersonelFiber(request.personelid,request.stockcardid).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res, "application/json");
            }
        }
    }
}