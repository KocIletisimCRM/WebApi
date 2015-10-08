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
using System.Diagnostics;

namespace CRMWebApi.Controllers
{

    [RoutePrefix("api/Task")]
    public class TaskController : ApiController
    {
        [Route("getTaskQueues")]
        [HttpPost]
        public HttpResponseMessage getTaskQueues(DTOs.DTORequestClasses.DTOGetTaskQueueRequest request)
        {
            using (var db = new CRMEntities())
            {
                ((IPersonelRequest)request).getFilter();
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
                string querySQL = request.getFilter().getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSQL = filter.getCountSQL();

                #region scb filter düzeltmesi
                if (request.hasCSBFilter())
                {
                    var sqlPart = "(EXISTS (SELECT * from _attachedobjectid WHERE _attachedobjectid.customerid = taskqueue.attachedobjectid))";
                    querySQL = querySQL.Replace(sqlPart, "@");
                    List<string> sbcExistsClauses = new List<string>(new string[] { sqlPart });
                    if (!request.isCustomerFilter())
                    {
                        sbcExistsClauses.Add("(EXISTS (SELECT * from _blockid WHERE _blockid.blockid = taskqueue.attachedobjectid))");
                        if (!request.isBlockFilter()) sbcExistsClauses.Add("(EXISTS (SELECT * from _siteid WHERE _siteid.siteid = taskqueue.attachedobjectid))");
                    }
                    querySQL = querySQL.Replace("@", string.Format("({0})", string.Join(" OR ", sbcExistsClauses)));

                    countSQL = countSQL.Replace(sbcExistsClauses[0], string.Format("({0})", string.Join(" OR ", sbcExistsClauses)));
                }
                #endregion
                
                var perf = Stopwatch.StartNew();
                var res = db.taskqueue.SqlQuery(querySQL).ToList();
                var qd = perf.Elapsed;
                perf.Restart();
                var rowCount = db.Database.SqlQuery<int>(countSQL).First();
                var cd = perf.Elapsed;
                perf.Restart();
                var taskIds = res.Select(r => r.taskid).Distinct().ToList();
                var tasks = db.task.Where(t => taskIds.Contains(t.taskid)).ToList();

                var personelIds = res.Select(r => r.attachedpersonelid)
                    .Union(res.Select(r => r.assistant_personel))
                    .Union(res.Select(r => r.updatedby)).Distinct().ToList();

                var personels = db.personel.Where(p => personelIds.Contains(p.personelid)).ToList();

                var customerIds = res.Select(c => c.attachedobjectid).Distinct().ToList();
                var customers = db.customer.Include(c => c.block.site).Where(c => customerIds.Contains(c.customerid)).ToList();

                var blockIds = res.Select(b => b.attachedobjectid).Distinct().ToList();
                var blocks = db.block.Include(b => b.site).Where(b => blockIds.Contains(b.blockid)).ToList();

                var taskstateIds = res.Select(tsp => tsp.status).Distinct().ToList();
                var taskstates = db.taskstatepool.Where(tsp => taskstateIds.Contains(tsp.taskstateid)).ToList();

                var issIds = res.Where(i=>i.attachedcustomer!=null && i.attachedcustomer.iss != null).Select(i => i.attachedcustomer.iss).Distinct().ToList();
                var isss = db.issStatus.Where(i => issIds.Contains(i.id)).ToList();

                var cststatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.customerstatus != null).Select(c => c.attachedcustomer.customerstatus).Distinct().ToList();
                var cststatus = db.customer_status.Where(c => cststatusIds.Contains(c.ID)).ToList();

                res.ForEach(r =>
                                 {
                                     r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                                     r.attachedpersonel = personels.Where(p => p.personelid == r.attachedpersonelid).FirstOrDefault();
                                     r.attachedcustomer = customers.Where(c => c.customerid == r.attachedobjectid).FirstOrDefault();
                                     if (r.attachedcustomer == null)
                                     {
                                         r.attachedblock = blocks.Where(b => b.blockid == r.attachedobjectid).FirstOrDefault();
                                     }
                                     if(r.attachedcustomer!=null)r.attachedcustomer.issStatus =isss.Where(i => i.id == (r.attachedcustomer.iss??0)).FirstOrDefault();
                                     if (r.attachedcustomer != null) r.attachedcustomer.customer_status = cststatus.Where(c => c.ID == (r.attachedcustomer.customerstatus ?? 0)).FirstOrDefault();
                                     r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                                     r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                                     r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();

                                 });
                var ld = perf.Elapsed;
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                DTOQueryPerformance qp = new DTOQueryPerformance
                {
                    QuerSQLyDuration = qd, CountSQLDuration = cd, LookupDuration = ld
                };
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r=>r.deleted==false).Select(r => r.toDTO()).ToList(), paginginfo, querySQL, qp),
                    "application/json"
                );
            }
        }

        [Route("saveTaskQueues")]
        [HttpPost]
        public HttpResponseMessage saveTaskQueues(DTOs.DTOtaskqueue tq)

        {
              
            // 7 User.Identity.PersonelID
            using (var db = new CRMEntities())
            {
                var tsm = db.taskstatematches.Include(t=>t.taskstatepool).Where(r => r.taskid == tq.task.taskid && r.stateid == tq.taskstatepool.taskstateid).FirstOrDefault();
                var dtq = db.taskqueue.Include(p=>p.attachedpersonel)
                                      .Include(tsp=>tsp.taskstatepool)
                                      .Include(ap=>ap.asistanPersonel)
                                      .Include(t=>t.task).Where(r => r.taskorderno == tq.taskorderno).First();
                
                

                #region Taskın durumu değişmişse yapılacaklar
                if (dtq.status != tq.taskstatepool.taskstateid && tq.taskstatepool.taskstateid!=0)
                {
                    dtq.status = tq.taskstatepool.taskstateid;

                    #region Değiştirilen taska bağlı taskların hiyerarşik iptali. Bu kod taskın zorunlu task atamalarından önce çalışmalıdır.
                    foreach (var item in db.taskqueue.Where(r => r.relatedtaskorderid == tq.taskorderno && (r.deleted == false) || r.previoustaskorderid == tq.taskorderno && (r.deleted == false)).ToList())
                    {
                        var stateid = db.taskstatematches.Where(r => r.taskid == item.taskid &&
                            r.automandatorytasks == null && r.taskstatepool.statetype == 2).First().taskstatepool.taskstateid;
                        item.status = stateid;
                        item.consummationdate = DateTime.Now;
                        item.description = "Hiyerarşik Task iptali";
                        db.SaveChanges();
                    }
                    #endregion

                    #region Browser tarafından attachedobjectid gönderilemediği için taskqueue tablosundan attachedobjectid ye erişme işlemi
                    var tqAttachedobjectid = db.taskqueue.Where(t => t.taskorderno == tq.taskorderno).Select(s => s.attachedobjectid).FirstOrDefault();
                    #endregion

                    #region zaten var olan müşteri ürünlerini silme(ilgili taskla ilişkili olanları)...
                    db.Database.ExecuteSqlCommand("update customerproduct set deleted=1, lastupdated=GetDate(), updatedby={0} where deleted=0 and taskid={1} and customerid={2}", new object[] { 7, tq.taskorderno, tqAttachedobjectid });
                    #endregion

                    #region stok hareketlerini silme
                    foreach (var item in db.stockmovement.Where(r => r.relatedtaskqueue == tq.taskorderno && r.deleted == false))
                    {
                        item.deleted = true;
                        item.updatedby = 7;
                        item.lastupdated = DateTime.Now;
                    }
                    #endregion

                    var docs = new List<int>();

                    #region Otomatik zorunlu taklar
                    if (tsm != null)
                    {
                        docs.AddRange((tsm.documents ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)));

                        foreach (var item in (tsm.automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                        {
                            if (db.taskqueue.Where(r => (r.relatedtaskorderid == tq.taskorderno || r.previoustaskorderid == tq.taskorderno) && r.taskid == item).Any())
                                continue;
                            var personel_id = (db.task.Where(t => t.attachablepersoneltype == dtq.attachedpersonel.category && t.taskid == item).Any());
                            var amtq = new taskqueue
                            {                               
                               appointmentdate = ((dtq.task.tasktype == 2 || dtq.taskid == 55 || dtq.taskid == 72 || dtq.taskid == 68 || item == 5 || item == 18 || item == 73 || item == 69 || item == 55)) ? (tq.appointmentdate) : (null),
                               
                               // 73 d smart. ömer adında silinen müşteriye atanıyor. sorulup silinecek
                               attachedpersonelid = (item == 73) ? 65 : ((item == 8147) ? null : (personel_id ? dtq.attachedpersonelid : (null))),
                               
                               //73 d smart kurulumu için sorulup silinecek
                               attachmentdate = (dtq.task.taskid == 73) ? (DateTime?)DateTime.Now : (personel_id ? ((dtq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : (null)),
                               
                                attachedobjectid = dtq.attachedobjectid,
                                taskid = item,
                                creationdate = DateTime.Now,
                                deleted = false,
                                lastupdated = DateTime.Now,
                                previoustaskorderid = dtq.taskorderno,

                                updatedby = 7, //User.Identity.PersonelID,
                                relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                            };
                            db.taskqueue.Add(amtq);
                        }
                    }
                    #endregion

                    #region Müşteri dökümanlarıyla alakalı işlemler
                    //foreach (var item in db.customerdocument.Where(r => r.taskqueueid == tq.taskorderno && r.deleted == false))
                    //{
                    //    if (!docs.Contains(item.documentid))
                    //    {
                    //        //yeni durumda olmayanlar siliniyor.
                    //        item.deleted = true;
                    //        item.lastupdated = DateTime.Now;
                    //        item.updatedby = 7; // User.Identity.PersonelID;
                    //    }
                    //    else docs.Remove(item.documentid);
                    //}
                    #endregion
                }
                #endregion

                #region kurulum işlemleri sonrası müşteriyi abone yapma.    // 23.10.2014 22:10 OZAL tasktype güncellnemesi taskid==5 olanlar tasktype ==3 veya tasktype==4 olarak değiştirildi*
                if ((dtq.taskid == 12 && dtq.status == 3) || ((dtq.task.tasktype == 3 || dtq.task.tasktype == 4) && dtq.status == 39) || (dtq.taskid == 53 && dtq.status == 39)) //status 57 de eklenmeli. tamamlandı güncelleme alınamadı
                {
                    var custom = db.customer.Where(r => r.customerid == dtq.attachedobjectid).FirstOrDefault();
                    if (custom != null)
                        custom.customerstatus = 1000;
                    db.SaveChanges();
                }
                #endregion

                #region saha çalışması kat ziyareti oluşturma

                if (dtq.taskid == 85 && (dtq.status == 75 || dtq.status == 77 || dtq.status == 78))
                {
                    var blok = dtq.attachedobjectid;
                    foreach (var hps in (db.customer.Where(r => r.blockid == blok && r.deleted==false)))
                    {
                        db.taskqueue.Add(new taskqueue
                        {
                            attachedpersonelid = dtq.attachedpersonelid,

                            attachmentdate = (DateTime?)DateTime.Now,
                            attachedobjectid = hps.customerid,
                            taskid = 86,
                            creationdate = DateTime.Now,
                            deleted = false,
                            lastupdated = DateTime.Now,
                            previoustaskorderid = dtq.taskorderno,
                            //Kullanıcı Kontrolü
                            updatedby =7, // User.Identity.PersonelID,
                            relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                        });
                    }

                    db.SaveChanges();
                }
                #endregion

                #region penetrasyon taskı kat ziyareti oluşturma Miraç 06.05.2015
                if (dtq.taskid == 8164 && dtq.status == 8110)
                {
                    var blok = dtq.attachedobjectid;
                    foreach (var hps in (db.customer.Where(r => r.blockid == blok && r.deleted==false)))
                    {
                        db.taskqueue.Add(new taskqueue
                        {
                            attachedpersonelid = dtq.attachedpersonelid,

                            attachmentdate = (DateTime?)DateTime.Now,
                            attachedobjectid = hps.customerid,
                            taskid = 86,
                            creationdate = DateTime.Now,
                            deleted = false,
                            lastupdated = DateTime.Now,
                            previoustaskorderid = dtq.taskorderno,
                            //Kullanıcı Kontrolü
                            updatedby =7,// User.Identity.PersonelID,
                            relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                        });
                    }

                    db.SaveChanges();
                }

                #endregion

                #region Kurulum randevusu kapandığında ikinci donanım tasklarının oluşturulma kodu
                /*25.10.2014 18:43  OZAL Ek ürün ve retention satış taskları ise yeniden
                 * task türemesini önlemek için kontrol bloğu ekledim
                 */
                var test = false;
                var ttqretek = dtq;
                while (ttqretek != null && ttqretek.task.tasktype != 1 && ttqretek.taskid != 65)
                    ttqretek = ttqretek.relatedTaskQueue;
                if (ttqretek != null)
                {
                    if (ttqretek.taskid == 6117 || ttqretek.taskid == 6115)
                        test = true;
                }
                if (!test)//Ek ürün veya retention değilse
                {
                    /* OZAL 25.10.2014 18:45*/
                    if ((dtq.task.tasktype == 3 || dtq.task.tasktype == 4) && (dtq.status != null && dtq.taskstatepool.statetype == 1) && (dtq.task.tasktype != 0))
                    {
                        var ttq = dtq;
                        while (ttq != null && ttq.task.tasktype != 1 && ttq.taskid != 65 && ttq.taskid != 69) /* Satış ziyareti veya Yönetim Odası Satışı--nakil taskı *///&& ttq.taskid != 53 27.12.2014 18:40 OZAL
                            ttq = ttq.relatedTaskQueue;
                        if (ttq == null)
                            throw new Exception("Satış Taskı Bulunamadı.");

                        var cust_pro = db.customerproduct.Where(r => r.taskid == ttq.taskorderno && r.deleted==false).ToList();
                        foreach (var p in cust_pro.Select(r => r.productid))
                            foreach (var item in (db.product_service.Where(r => r.productid == p).First().automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                            {
                                if (db.taskqueue.Where(r => (r.relatedtaskorderid == tq.taskorderno || r.previoustaskorderid == tq.taskorderno) && r.taskid == item).Any())
                                    continue;
                                var personel_id = (db.task.Any(m => m.attachablepersoneltype == dtq.attachedpersonel.category && m.taskid == item));
                                db.taskqueue.Add(new taskqueue
                                {
                                    attachedpersonelid = personel_id ? dtq.attachedpersonelid : (null),

                                    attachmentdate = personel_id ? (DateTime?)DateTime.Now : (null),
                                    attachedobjectid = dtq.attachedobjectid,
                                    taskid = item,
                                    creationdate = DateTime.Now,
                                    deleted = false,
                                    lastupdated = DateTime.Now,
                                    previoustaskorderid = dtq.taskorderno,
                                    //Kullanıcı Kontrolü
                                    updatedby =7,// User.Identity.PersonelID,
                                    relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                                });
                            }
                        db.SaveChanges();
                    }
                }
                #endregion

                dtq.description = tq.description!=null ?tq.description :dtq.description;
                dtq.appointmentdate = (tq.appointmentdate!=null) ? tq.appointmentdate :dtq.appointmentdate;
                dtq.creationdate = (tq.creationdate!=null) ? tq.creationdate : dtq.creationdate;
                dtq.assistant_personel = (tq.asistanPersonel.personelid !=null && tq.asistanPersonel.personelid!=0) ? tq.asistanPersonel.personelid : dtq.assistant_personel;
                dtq.consummationdate = (tq.consummationdate!=null) ? tq.consummationdate : (dtq.status!=null) ? dtq.consummationdate : DateTime.Now;
                dtq.lastupdated = DateTime.Now;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,"ok","application/json");          
                }           
        }

        [Route("saveSalesTask")]
        [HttpPost]
        public HttpResponseMessage saveSalesTask(int? customerid_VI, int? attachedpersonelid_VI, int? assistant_personel_VI, DateTime appointmentdate)
        {
            using (var db=new CRMEntities())
            {
                var taskqueue = new taskqueue
                {
                    appointmentdate = DateTime.Now,
                    attachedobjectid = customerid_VI ?? 0,
                    attachedpersonelid = attachedpersonelid_VI,
                    assistant_personel = assistant_personel_VI,
                    attachmentdate = DateTime.Now,
                    consummationdate = DateTime.Now,
                    creationdate = DateTime.Now,
                    deleted = false,
                    description = "Doğrudan Satış",
                    lastupdated = DateTime.Now,
                    status = null,
                    taskid = 3,
                    updatedby = 7
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,taskqueue.taskorderno,"application/json");
            }
        }

        [Route("deneme")]
        [HttpGet]
        public HttpResponseMessage deneme(BaseAttachedObject t) 
        {
            return Request.CreateResponse(HttpStatusCode.OK,t,"application/json");
        }

        [Route("personelattachment")]
        [HttpPost]
        public HttpResponseMessage personelattachment(DTOs.DTORequestClasses.DTORequestAttachmentPersonel request) 
        {
            if (request.ids.Count()>0)
            {
            using (var db=new CRMEntities())
            {
                var tqs = db.taskqueue.Where(t => request.ids.Contains(t.taskorderno) && t.status != null).Count();
                if (tqs > 0)
                 {
                     DTOResponseError re = new DTOResponseError
                     {
                         errorCode=-1,
                         errorMessage = "Sonlandırılmış Tasklara Personel Atanamaz"
                     };
                     return Request.CreateResponse(HttpStatusCode.OK,re,"application/json");
                 }  
            }
            using (var db=new CRMEntities())
            {
                 var tqs = db.taskqueue.Where(t => request.ids.Contains(t.taskorderno))
                          .Select(t => t.task.attachablepersoneltype).Distinct().ToList();
               var cnt = tqs.Count();
              if (cnt > 1)
                {
                    DTOResponseError re = new DTOResponseError
                    {
                        errorCode=-1,
                        errorMessage = "Çoklu Atama Yapmak için Aynı Tür Taskları Seçmelisiniz..."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, re, "application/json");
                }
              else 
              {
                  if (request.personelname != null)
                  {
                      var apid = db.personel.Where(p => p.personelname.Contains(request.personelname)).Select(s => s.personelid).FirstOrDefault();
                      foreach (var item in request.ids)
                      {
                          var tq = db.taskqueue.Where(t => t.taskorderno == item).FirstOrDefault();
                          tq.attachedpersonelid = apid;
                          tq.attachmentdate = DateTime.Now;
                          tq.lastupdated = DateTime.Now;
                          tq.updatedby = 7;
                          db.SaveChanges();
                      }
                      DTOResponseError re = new DTOResponseError
                      {
                          errorCode = 1,
                          errorMessage = request.ids.Count() + " adet taska atama yapıldı"
                      };
                      return Request.CreateResponse(HttpStatusCode.OK, re, "application/json");
                  }
                  else
                  {
                      DTOResponseError re = new DTOResponseError
                      {
                          errorCode = 0,
                          errorMessage = "Personel Seçimi Yapmadınız!"
                      };
                      return Request.CreateResponse(HttpStatusCode.OK, re, "application/json");
                  }
              }  
           
            }
        }
            else
            {
                DTOResponseError re = new DTOResponseError
                {
                    errorCode = -1,
                    errorMessage = "Atama yapmak için hiç task seçilmemiş"
                };
                return Request.CreateResponse(HttpStatusCode.OK, re, "application/json");
            }
        }
    }
}
