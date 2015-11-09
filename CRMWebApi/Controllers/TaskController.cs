using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs.Fiber;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using System.Diagnostics;
using CRMWebApi.Models.Fiber;

namespace CRMWebApi.Controllers
{

    [RoutePrefix("api/Fiber/Task")]
    public class TaskController : ApiController
    {
        [Route("getTaskQueues")]
        [HttpPost]
        public HttpResponseMessage getTaskQueues(DTOGetTaskQueueRequest request)
        {
            using (var db = new CRMEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                string querySQL = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
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

                List<personel> personels;// personelin stok durumuna ihtiyaç yoksa sorgulanmasın
                if (request.taskOrderNo != null) personels = db.personel.Include(p => p.stockstatus).Where(p => personelIds.Contains(p.personelid)).ToList();
                else personels = db.personel.Where(p => personelIds.Contains(p.personelid)).ToList();

                var customerIds = res.Select(c => c.attachedobjectid).Distinct().ToList();
                var customers = db.customer.Include(c => c.block.site).Where(c => customerIds.Contains(c.customerid)).ToList();

                var blockIds = res.Select(b => b.attachedobjectid).Distinct().ToList();
                var blocks = db.block.Include(b => b.site).Where(b => blockIds.Contains(b.blockid)).ToList();

                var taskstateIds = res.Select(tsp => tsp.status).Distinct().ToList();
                var taskstates = db.taskstatepool.Where(tsp => taskstateIds.Contains(tsp.taskstateid)).ToList();

                var issIds = res.Where(i => i.attachedcustomer != null && i.attachedcustomer.iss != null).Select(i => i.attachedcustomer.iss).Distinct().ToList();
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
                                     if (r.attachedcustomer != null) r.attachedcustomer.issStatus = isss.Where(i => i.id == (r.attachedcustomer.iss ?? 0)).FirstOrDefault();
                                     if (r.attachedcustomer != null) r.attachedcustomer.customer_status = cststatus.Where(c => c.ID == (r.attachedcustomer.customerstatus ?? 0)).FirstOrDefault();
                                     r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                                     r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                                     r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();
                                     if (request.taskOrderNo != null)
                                     {
                                         var customerid = res.Select(c => c.attachedobjectid).FirstOrDefault();
                                         var salestaskorderno = db.taskqueue.Where(t => t.taskid == 3 && t.attachedobjectid == customerid)
                                             .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();

                                         //taska bağlı müşteri kampanyası ve bilgileri
                                         r.customerproduct = db.customerproduct.Include(c => c.campaigns).Where(c => c.taskid == salestaskorderno).ToList();
                                         //taska bağlı stock hareketleri
                                         r.stockmovement = db.stockmovement.Include(s => s.stockcard).Where(s => s.relatedtaskqueue == r.taskorderno).ToList();
                                         var stockcardids = db.taskstatematches.Where(tsm => tsm.taskid == r.taskid && tsm.stateid == r.status && tsm.stockcards != null).ToList()
                                         .SelectMany(s => s.stockcards.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(ss => Convert.ToInt32(ss))).ToList();
                                         r.stockcardlist = db.stockcard.Where(s => stockcardids.Contains(s.stockid)).ToList();
                                         //sadece task durumuyla ilişkili stockcardid'ler seçiliyor
                                         if (stockcardids.Count() > 0)
                                         {
                                             r.attachedpersonel.stockstatus = r.attachedpersonel.stockstatus.
                                                Where(ss => stockcardids.Contains(ss.stockcardid)).ToList();
                                             foreach (var ss in r.attachedpersonel.stockstatus)
                                             {
                                                 ss.serials = db.getSerialsOnPersonel(ss.toobject, ss.stockcardid).ToList();
                                             }
                                         }
                                     }

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
                    QuerSQLyDuration = qd,
                    CountSQLDuration = cd,
                    LookupDuration = ld
                };
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySQL, qp),
                    "application/json"
                );
            }
        }

        [Route("saveTaskQueues")]
        [HttpPost]
        public HttpResponseMessage saveTaskQueues(DTOtaskqueue tq)
        {

            // 7 User.Identity.PersonelID
            using (var db = new CRMEntities())
            {
                var tsm = db.taskstatematches.Include(t => t.taskstatepool).Where(r => r.taskid == tq.task.taskid && r.stateid == tq.taskstatepool.taskstateid).FirstOrDefault();
                var dtq = db.taskqueue.Include(t => t.task)
                                      .Include(p => p.attachedpersonel)
                                      .Include(tsp => tsp.taskstatepool)
                                      .Include(ap => ap.asistanPersonel)
                                      .Include(rt=>rt.relatedTaskQueue)
                                      .Include(pt=>pt.previousTaskQueue)
                                      .Where(r => r.taskorderno == tq.taskorderno).First();



                #region Taskın durumu değişmişse (Aynı Zamanda Taskın durumu açığa alınacaksa) yapılacaklar
                if ((dtq.status != tq.taskstatepool.taskstateid) && (tq.taskstatepool.taskstateid != 0 || tq.taskstatepool.taskstate != null))
                {
                    if (tq.taskstatepool.taskstateid == 0) dtq.status = null;// taskın durumunu açığa alma
                    else dtq.status = tq.taskstatepool.taskstateid;

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
                                attachedpersonelid = (item == 8147) ? null : (personel_id ? dtq.attachedpersonelid : (null)),
                                attachmentdate = personel_id ? ((dtq.taskstatepool != null ? ((dtq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : (DateTime?)DateTime.Now)) : (null),
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
                    foreach (var hps in (db.customer.Where(r => r.blockid == blok && r.deleted == false)))
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
                            updatedby = 7, // User.Identity.PersonelID,
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
                    foreach (var hps in (db.customer.Where(r => r.blockid == blok && r.deleted == false)))
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
                            updatedby = 7,// User.Identity.PersonelID,
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

                        var cust_pro = db.customerproduct.Where(r => r.taskid == ttq.taskorderno && r.deleted == false).ToList();
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
                                    updatedby = 7,// User.Identity.PersonelID,
                                    relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                                });
                            }
                        db.SaveChanges();
                    }
                }
                #endregion

                dtq.description = tq.description != null ? tq.description : dtq.description;
                dtq.appointmentdate = (tq.appointmentdate != null) ? tq.appointmentdate : dtq.appointmentdate;
                dtq.creationdate = (tq.creationdate != null) ? tq.creationdate : dtq.creationdate;
                dtq.assistant_personel = (tq.asistanPersonel.personelid != 0) ? tq.asistanPersonel.personelid : dtq.assistant_personel;
                dtq.consummationdate = tq.consummationdate != null ? tq.consummationdate : (dtq.consummationdate != null) ? dtq.consummationdate : DateTime.Now;
                dtq.lastupdated = DateTime.Now;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            }
        }

        [Route("closeTaskQueues")]
        [HttpPost]
        public HttpResponseMessage closeTaskQueues(DTORequestCloseTaskqueue request)
        {
            using (var db = new CRMEntities())
            {
                var tq = db.taskqueue.Include(t => t.attachedpersonel).Where(r => r.taskorderno == request.taskorderno).First();
                var cdocs = tq.customerproduct.Where(r => r.deleted == false);
                var olddocs = (cdocs.Any()) ? (cdocs.First().campaigns.documents ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)).ToList() : new List<int>();
                var newdocs = (db.campaigns.Where(r => r.id == request.campaignid).First().documents ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r));
                #region Eski kampanya belgeleri siliniyor
                foreach (var item in olddocs.Where(r => !newdocs.Contains(r)))
                {
                    foreach (var cdc in db.customerdocument.Where(r => r.documentid == item && r.taskqueueid == tq.taskorderno && r.deleted == false))
                    {
                        cdc.updatedby = 7;// User.Identity.PersonelID;
                        cdc.deleted = true;
                        cdc.lastupdated = DateTime.Now;
                    }
                }
                #endregion
                #region Yeni kampanya belgeleri ekleniyor
                foreach (var item in newdocs.Where(r => !olddocs.Contains(r)))
                {
                    db.customerdocument.Add(new customerdocument
                    {
                        creationdate = DateTime.Now,
                        customerid = tq.attachedobjectid,
                        deleted = false,
                        documentid = item,
                        lastupdated = DateTime.Now,
                        taskqueueid = tq.taskorderno,
                        updatedby = 7,// User.Identity.PersonelID,
                        attachedobjecttype = tq.task.attachableobjecttype ?? 0
                    });
                }
                #endregion
                var tsm = db.taskstatematches.Where(r => r.taskid == tq.taskid && r.stateid == tq.status).FirstOrDefault();
                #region Eski ürünler siliniyor
                db.Database.ExecuteSqlCommand("update customerproduct set deleted=1, lastupdated=GetDate(), updatedby={0} where taskid={1}", new object[] { 7, tq.taskorderno });
                #endregion
                #region  Yeni ürün ekleniyor
                foreach (var p in request.selectedProductsIds.Where(s => s != 0))
                {

                    db.customerproduct.Add(new customerproduct
                    {
                        campaignid = request.campaignid,
                        creationdate = DateTime.Now,
                        customerid = tq.attachedobjectid,
                        deleted = false,
                        lastupdated = DateTime.Now,
                        productid = p,
                        taskid = tq.taskorderno,
                        updatedby = 7,// User.Identity.PersonelID
                    });
                    #region Ek ürün için otomatik zorunlu taskların türetilmesi-- OZAL 10.10.2014 17:45 ve Retention
                    if (tq.taskid == 6115 || tq.taskid == 6117)
                    {
                        foreach (var item in (db.product_service.Where(r => r.productid == p).First().automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                        {
                            var personel_id = (db.task.Where(t => t.attachablepersoneltype == tq.attachedpersonel.category && t.taskid == item).Any());
                            db.taskqueue.Add(new taskqueue
                            {

                                appointmentdate = ((item == 4 || item == 55 || item == 72 || item == 68) && (item == 5 || item == 18 || item == 73 || item == 69 || item == 55)) ? (tq.appointmentdate) : (null),

                                attachmentdate = (item == 73) ? (DateTime?)DateTime.Now : (personel_id ? ((tq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : tq.attachmentdate),

                                attachedobjectid = tq.attachedobjectid,
                                taskid = item,
                                creationdate = DateTime.Now,
                                deleted = false,
                                lastupdated = DateTime.Now,
                                previoustaskorderid = tq.taskorderno,
                                updatedby = 7, //User.Identity.PersonelID,
                                /*25.10.2014 17:33 OZAL  Önceki kod kısmında alttaki satır kapalıydı ve task oluşurken ilişkilendirme yapılamıyordu*/
                                relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? tq.taskorderno : tq.relatedtaskorderid
                                /*25.10.2014 17:33 */
                            });
                        }
                    }
                    #endregion
                }
                #endregion
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            }
        }

        [Route("saveSalesTask")]
        [HttpPost]
        public HttpResponseMessage saveSalesTask(DTOSaveFiberSalesTask request)
        {
            using (var db = new CRMEntities())
            {
                var taskqueue = new taskqueue
                {
                    appointmentdate = request.appointmentdate != null ? request.appointmentdate : DateTime.Now,
                    attachedobjectid = request.customerid ?? 0,
                    attachedpersonelid = request.attachedpersonelid,
                    attachmentdate = DateTime.Now,
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
                return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
            }
        }

        [Route("saveFaultTask")]
        [HttpPost]
        public HttpResponseMessage SaveFaultTask(DTORequestSaveFaultTaks request)
        {
            using (var db = new CRMEntities())
            {
                var taskqueue = new taskqueue
                {
                    appointmentdate = request.appointmentdate,
                    attachedobjectid = request.customerid ?? 0,
                    attachedpersonelid = request.attachedpersonelid,
                    attachmentdate = DateTime.Now,
                    creationdate = DateTime.Now,
                    deleted = false,
                    description = request.description,
                    fault = request.fault,
                    lastupdated = DateTime.Now,
                    status = null,
                    taskid = 66,
                    updatedby = 7,//User.Identity.PersonelID
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
            }
        }

        [Route("personelattachment")]
        [HttpPost]
        public HttpResponseMessage personelattachment(DTORequestAttachmentPersonel request)
        {
            if (request.ids.Count() > 0)
            {
                using (var db = new CRMEntities())
                {
                    var tqs = db.taskqueue.Where(t => request.ids.Contains(t.taskorderno) && t.status != null).Count();
                    if (tqs > 0)
                    {
                        DTOResponseError re = new DTOResponseError
                        {
                            errorCode = -1,
                            errorMessage = "Sonlandırılmış Tasklara Personel Atanamaz"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, re, "application/json");
                    }
                }
                using (var db = new CRMEntities())
                {
                    var tqs = db.taskqueue.Where(t => request.ids.Contains(t.taskorderno))
                             .Select(t => t.task.attachablepersoneltype).Distinct().ToList();
                    var cnt = tqs.Count();
                    if (cnt > 1)
                    {
                        DTOResponseError re = new DTOResponseError
                        {
                            errorCode = -1,
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

        [Route("saveCustomerCard")]
        [HttpPost]
        public HttpResponseMessage saveCustomerCard(DTOKatZiyareti ct)
        {
            using (var db = new CRMEntities())
            {
                if (db.customer.Any(c => c.customerid == ct.customerid))
                {
                    var item = db.customer.Where(c => c.customerid == ct.customerid).First();

                    item.customername = ct.customername;
                    item.customersurname = ct.customersurname;
                    item.gsm = ct.gsm;
                    if (ct.netStatus.id != 0)
                        item.netstatu = ct.netStatus.id;
                    if (ct.telStatus.id!=0)
                        item.telstatu = ct.telStatus.id;
                    if (ct.gsmKullanımıStatus.id!=0)
                        item.gsmstatu = ct.gsmKullanımıStatus.id;
                    if (ct.issStatus.id!=0)
                        item.iss = ct.issStatus.id;
                    if(ct.TvKullanımıStatus.id!=0)
                         item.tvstatu = ct.TvKullanımıStatus.id;
                    if (ct.telStatus.id != 0)
                        item.telstatu = ct.telStatus.id;
                    item.description = ct.description;
                    item.lastupdated = DateTime.Now;
                    item.updatedby = 9;

                }
                if (ct.closedKatZiyareti == true)
                {
                    var res = db.taskqueue.Where(tq => tq.attachedobjectid == ct.customerid && tq.taskid == 86 && tq.status == null).FirstOrDefault();
                    res.status = 1079;
                }

                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            }
        }

        #region Task Tanımlamaları Sayfası
        [Route("getTaskList")]
        [HttpPost]
        public HttpResponseMessage getTaskList(DTOFilterGetTasksRequest request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                var querySql = filter.subTables["taskid"].getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.task.SqlQuery(querySql).ToList();
                var ptypeids = res.Select(r => r.attachablepersoneltype).Distinct().ToList();
                var personels = db.objecttypes.Where(p => ptypeids.Contains(p.typeid)).ToList();

                var obtpyeids = res.Select(r => r.attachableobjecttype).Distinct().ToList();
                var objects = db.objecttypes.Where(o => obtpyeids.Contains(o.typeid)).ToList();

                var ttypeids = res.Select(s => s.tasktype).Distinct().ToList();
                var tasktypess = db.tasktypes.Where(t => ttypeids.Contains(t.TaskTypeId)).ToList();
                res.ForEach(r =>
                {
                    r.objecttypes = objects.Where(t => t.typeid == r.attachableobjecttype).FirstOrDefault();
                    r.personeltypes = personels.Where(p => p.typeid == r.attachablepersoneltype).FirstOrDefault();
                    r.tasktypes = tasktypess.Where(t => t.TaskTypeId == r.tasktype).FirstOrDefault();
                });


                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
               
            }
        }

        [Route("saveTask")]
        [HttpPost]
        public HttpResponseMessage saveTask(DTOtask request)
        {
            using (var db=new CRMEntities())
            {
                var dt = db.task.Where(t => t.taskid == request.taskid).FirstOrDefault();
                var errormessage = new DTOResponseError();

                dt.taskname = request.taskname;
                dt.performancescore = request.performancescore;
                dt.tasktype = request.tasktypes.TaskTypeId;
                dt.attachableobjecttype = request.objecttypes.typeid;
                dt.attachablepersoneltype = request.personeltypes.typeid;
                dt.updatedby = 7;
                dt.lastupdated = DateTime.Now;
                db.SaveChanges();
                errormessage.errorCode = 1;
                errormessage.errorMessage = "İşlem Başarılı";
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");

            }
        }
        [Route("insertTask")]
        [HttpPost]
        public HttpResponseMessage insertTask(DTOtask request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError();
                var t = new task {
                    taskname = request.taskname,
                    attachableobjecttype=request.objecttypes.typeid,
                    attachablepersoneltype=request.personeltypes.typeid,
                    performancescore=request.performancescore,
                    tasktype=request.tasktypes.TaskTypeId,
                    deleted=false,
                    creationdate=DateTime.Now,
                    lastupdated=DateTime.Now,
                    updatedby=7

                };
                db.task.Add(t);
                db.SaveChanges();
                errormessage.errorCode = 1;
                errormessage.errorMessage = "İşlem Başarılı";
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");

            }
        }
        #endregion
        #region Task Durum Havuzu
        [Route("getTaskState")]
        [HttpPost]
        public HttpResponseMessage getTaskstate(DTOGetTSPFilter request)
        {
            using (var db=new CRMEntities())
            {
                var filter = request.getFilter();
                var querySql = filter.getPagingSQL(request.pageNo,request.rowsPerPage);
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First() ;

                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                var res = db.taskstatepool.SqlQuery(querySql).OrderBy(o=>o.taskstateid).ToList();
                return Request.CreateResponse(HttpStatusCode.OK,  new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo), "application/json");

            }
        }

        [Route("saveTaskState")]
        [HttpPost]
        public HttpResponseMessage saveTaskstate(DTOtaskstatepool request)
        {
            using (var db = new CRMEntities())
            {
                var dtsp = db.taskstatepool.Where(t => t.taskstateid == request.taskstateid).FirstOrDefault();
                dtsp.taskstate = request.taskstate;
                dtsp.statetype = request.statetype;
                dtsp.lastupdated = DateTime.Now;
                dtsp.updatedby = 7;
                db.SaveChanges();
                var errormessage = new DTOResponseError { errorCode=1,errorMessage="İşlem Başarılı"};
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertTaskState")]
        [HttpPost]
        public HttpResponseMessage insertTaskState(DTOtaskstatepool request)
        {
            using (var db=new CRMEntities())
            {
                var tsp = new taskstatepool {
                    taskstate = request.taskstate,
                    statetype = request.statetype,
                    creationdate = DateTime.Now,
                    lastupdated=DateTime.Now,
                    updatedby=7,
                    deleted =false
                };
                db.taskstatepool.Add(tsp);
                db.SaveChanges();
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        #endregion
        [Route("getTaskStateMatches")]
        [HttpPost]
        public HttpResponseMessage getTaskStateMatches(DTOGetTSMFilter request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();

                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                var res = db.taskstatematches.SqlQuery(querySql).ToList();
                var taskids = res.Select(s => s.taskid).Distinct().ToList();
                var tasks = db.task.Where(t => taskids.Contains(t.taskid)).ToList();

                var stateids = res.Select(s => s.stateid).Distinct().ToList();
                var states = db.taskstatepool.Where(tsp => stateids.Contains(tsp.taskstateid)).ToList();
                res.ForEach(r=> {
                    r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                    r.taskstatepool = states.Where(t => t.taskstateid == r.stateid).FirstOrDefault();
                });

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo)
                    , "application/json");

            }
        }

        #region Document sayfası için
        [Route("getDocuments")]
        [HttpPost]
        public HttpResponseMessage getDocuments(DTOGetDocumentFilter request)
        {
            using (var db=new CRMEntities())
            {
                var filter = request.getFilter();
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.document.SqlQuery(querySql).ToList();

                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo)
                    , "application/json");
            }
        }

        [Route("saveDocument")]
        [HttpPost]
        public HttpResponseMessage saveDocument(DTOdocument request)
        {
            using (var db = new CRMEntities())
            {
                var ddoc = db.document.Where(d => d.documentid == request.documentid).FirstOrDefault();
                ddoc.documentname = request.documentname;
                ddoc.documentdescription = request.documentdescription;
                ddoc.lastupdated = DateTime.Now;
                ddoc.updatedby = 7;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK,DTOResponseError.NoError(),"application/json");
            }
        }

        [Route("insertDocument")]
        [HttpPost]
        public HttpResponseMessage insertDocument(DTOdocument request)
        {
            using (var db = new CRMEntities())
            {
                var d = new document {
                    documentname = request.documentname,
                    documentdescription = request.documentdescription,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby=7
                };
                db.document.Add(d);
                db.SaveChanges();
                DTOResponseError errormessage = new DTOResponseError { errorCode=1,errorMessage="İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion

        //#region Kampanya Sayfası 
        //[Route("getCampaigns")]
        //[HttpPost]
        //public HttpResponseMessage getCampaigns(DTOFiterGetCampaignRequst request)
        //{
        //    using (var db=new CRMEntities())
        //    {
        //        var filter = request.getFilter();
        //        filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
        //        var countSql = filter.getCountSQL();
        //        var rowCount = db.Database.SqlQuery<int>(countSql).First();
        //        var querySql = filter.getPagingSQL(request.pageNo,request.rowsPerPage);
        //        var res = db.campaigns.SqlQuery(querySql).ToList();
        //        DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
        //        {
        //            pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
        //            pageNo = request.pageNo,
        //            rowsPerPage = request.rowsPerPage,
        //            totalRowCount = rowCount
        //        };

        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySql)
        //            , "application/json");
        //    }
        //}

        //[Route("saveCampaigns")]
        //[HttpPost]
        //public HttpResponseMessage saveCampaigns(DTOcampaigns request)
        //{
        //    using (var db = new CRMEntities())
        //    {
        //        var errormessage = new DTOResponseError {errorCode=1,errorMessage="İşlem Başarılı" };
        //        var dcamp = db.campaigns.Where(t => t.id == request.id).FirstOrDefault();

        //        dcamp.name = request.name;
        //        dcamp.category = request.category;
        //        dcamp.subcategory = request.subcategory;
        //        dcamp.products = request.products;
        //        dcamp.documents = request.documents;
        //        dcamp.lastupdated = DateTime.Now;
        //        dcamp.updatedby = 7;
        //        db.SaveChanges();
        //        return Request.CreateResponse(HttpStatusCode.OK,errormessage, "application/json");
        //    }
        //}

        //[Route("insertCampaigns")]
        //[HttpPost]
        //public HttpResponseMessage insertCampaigns(DTOcampaigns request)
        //{
        //    using (var db = new CRMEntities())
        //    {
        //        var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
        //        var c = new campaigns {
        //            name=request.name,
        //            category=request.category,
        //            subcategory=request.subcategory,
        //            products=request.products,
        //            documents=request.documents,
        //            creationdate=DateTime.Now,
        //            lastupdated=DateTime.Now,
        //            deleted=false,
        //            updatedby=7
        //        };
        //        db.campaigns.Add(c);
        //        db.SaveChanges();
        //        return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
        //    }
        //}
        //#endregion

        #region Ürün Tanımlamaları Sayfası

        [Route("getProducts")]
        [HttpPost]
        public HttpResponseMessage getProducts(DTOGetProductFilter request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.product_service.SqlQuery(querySql).ToList();
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySql)
                    , "application/json");
            }
        }

        [Route("saveProduct")]
        [HttpPost]
        public HttpResponseMessage saveProduct(DTOProduct_service request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var dpro = db.product_service.Where(t => t.productid == request.productid).FirstOrDefault();

                dpro.productname = request.productname;
                dpro.category = request.category;
                dpro.maxduration = request.maxduration;
                dpro.automandatorytasks = request.automandatorytasks;
                dpro.lastupdated = DateTime.Now;
                dpro.updatedby = 7;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertProduct")]
        [HttpPost]
        public HttpResponseMessage insertProduct(DTOProduct_service request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var p = new product_service
                {
                    productname = request.productname,
                    category = request.category,
                    automandatorytasks = request.automandatorytasks,
                    maxduration = request.maxduration,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = 7
                };
                db.product_service.Add(p);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion

        //#region Personel Tanımlama Sayfası

        //[Route("getPersonels")]
        //[HttpPost]
        //public HttpResponseMessage getPersonels(DTOFilterGetPersonelRequest request)
        //{
        //    using (var db = new CRMEntities())
        //    {
        //        var filter = request.getFilter();
        //        filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
        //        var countSql = filter.getCountSQL();
        //        var rowCount = db.Database.SqlQuery<int>(countSql).First();
        //        var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
        //        var res = db.personel.SqlQuery(querySql).ToList();
        //        DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
        //        {
        //            pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
        //            pageNo = request.pageNo,
        //            rowsPerPage = request.rowsPerPage,
        //            totalRowCount = rowCount
        //        };

        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySql)
        //            , "application/json");
        //    }
        //}

        //[Route("savePersonel")]
        //[HttpPost]
        //public HttpResponseMessage savePersonel(DTOpersonel request)
        //{
        //    using (var db = new CRMEntities())
        //    {
        //        var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
        //        var dp = db.personel.Where(t => t.personelid == request.personelid).FirstOrDefault();

        //        dp.personelname = request.personelname;
        //        dp.category =(int) request.category;
        //        dp.mobile = request.mobile;
        //        dp.email = request.email;
        //        dp.password = request.password;
        //        dp.notes = request.notes;
        //        dp.lastupdated = DateTime.Now;
        //        dp.updatedby = 7;
        //        db.SaveChanges();
        //        return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
        //    }
        //}


        //[Route("insertPersonel")]
        //[HttpPost]
        //public HttpResponseMessage insertPersonel(DTOpersonel request)
        //{
        //    using (var db = new CRMEntities())
        //    {
        //        var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
        //        var p = new personel
        //        {
        //            personelname = request.personelname,
        //            category = (int)request.category,
        //            mobile = request.mobile,
        //            email = request.email,
        //            password = request.password,
        //            notes=request.notes,
        //            roles=(int)request.category,
        //            creationdate = DateTime.Now,
        //            lastupdated = DateTime.Now,
        //            deleted = false,
        //            updatedby = 7
        //        };
        //        db.personel.Add(p);
        //        db.SaveChanges();
        //        return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
        //    }
        //}
        //#endregion

        #region Depo Kart Tanımlamaları Sayfası

        [Route("getStockCards")]
        [HttpPost]
        public HttpResponseMessage getStockCads(DTOFilterGetStockCardRequest request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.stockcard.SqlQuery(querySql).ToList();
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySql)
                    , "application/json");
            }
        }

        [Route("saveStockCard")]
        [HttpPost]
        public HttpResponseMessage saveStockCard(DTOstockcard request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var ds = db.stockcard.Where(t => t.stockid == request.stockid).FirstOrDefault();

                ds.productname = request.productname;
                ds.category = request.category;
                ds.hasserial = request.hasserial;
                ds.unit = request.unit;
                ds.description = request.description;
                ds.lastupdated = DateTime.Now;
                ds.updatedby = 7;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }


        [Route("insertStockCard")]
        [HttpPost]
        public HttpResponseMessage insertStockCard(DTOstockcard request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var s = new stockcard
                {
                    productname = request.productname,
                    category = request.category,
                    hasserial = request.hasserial,
                    unit = request.unit,
                    description = request.description,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = 7
                };
                db.stockcard.Add(s);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion

        //#region Adsl Satış İşlemleri

        //[Route("getAdress")]
        //[HttpPost]
        //public HttpResponseMessage getAdress (DTOGetAdressFilter request)
        //{
        //    using (var db=new CRMEntities())
        //    {
        //        var filter = request.getFilter();
        //        var querySql = filter.getFilterSQL();
        //        if (filter.tableName=="ilce")
        //        {
        //          var  res = db.ilce.SqlQuery(querySql).ToList();
        //          return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
        //        }
        //        else if (filter.tableName == "mahalleKoy")
        //        {
        //            var res = db.mahalleKoy.SqlQuery(querySql).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
        //        }
        //        else if (filter.tableName == "cadde")
        //        {
        //            var res = db.cadde.SqlQuery(querySql).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
        //        }
        //        else if (filter.tableName == "bina")
        //        {
        //            var res = db.bina.SqlQuery(querySql).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
        //        }
        //        else if (filter.tableName == "daire")
        //        {
        //            var res = db.daire.SqlQuery(querySql).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
        //        }
        //        else
        //        {
        //            var res = db.il.SqlQuery(querySql).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
        //        }

        //    }

        //}

        //#endregion

        #region Personel Tanımlama Sayfası

        [Route("getPersonels")]
        [HttpPost]
        //[KOCAuthorize]
        public HttpResponseMessage getPersonels(DTOFilterGetPersonelRequest request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.personel.SqlQuery(querySql).ToList();
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySql)
                    , "application/json");
            }
        }

        [Route("savePersonel")]
        [HttpPost]
        public HttpResponseMessage savePersonel(DTOpersonel request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var dp = db.personel.Where(t => t.personelid == request.personelid).FirstOrDefault();

                dp.personelname = request.personelname;
                dp.category = (int)request.category;
                dp.mobile = request.mobile;
                dp.email = request.email;
                dp.password = request.password;
                dp.notes = request.notes;
                dp.lastupdated = DateTime.Now;
                dp.updatedby = 7;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }


        [Route("insertPersonel")]
        [HttpPost]
        public HttpResponseMessage insertPersonel(DTOpersonel request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var p = new personel
                {
                    personelname = request.personelname,
                    category = (int)request.category,
                    mobile = request.mobile,
                    email = request.email,
                    password = request.password,
                    notes = request.notes,
                    roles = (int)request.category,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = 7
                };
                db.personel.Add(p);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion


        #region Kampanya Sayfası 
        [Route("getCampaigns")]
        [HttpPost]
        public HttpResponseMessage getCampaigns(DTOFiterGetCampaignRequst request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.campaigns.SqlQuery(querySql).ToList();
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySql)
                    , "application/json");
            }
        }

        [Route("saveCampaigns")]
        [HttpPost]
        public HttpResponseMessage saveCampaigns(DTOcampaigns request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var dcamp = db.campaigns.Where(t => t.id == request.id).FirstOrDefault();

                dcamp.name = request.name;
                dcamp.category = request.category;
                dcamp.subcategory = request.subcategory;
                dcamp.products = request.products;
                dcamp.documents = request.documents;
                dcamp.lastupdated = DateTime.Now;
                dcamp.updatedby = 7;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertCampaigns")]
        [HttpPost]
        public HttpResponseMessage insertCampaigns(DTOcampaigns request)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var c = new campaigns
                {
                    name = request.name,
                    category = request.category,
                    subcategory = request.subcategory,
                    products = request.products,
                    documents = request.documents,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = 7
                };
                db.campaigns.Add(c);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion
    }
}
