using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/TaskQueues")]

    public class AdslTaskqueueController : ApiController
    {
        [Route("getTaskQueues")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage getTaskQueues(DTOGetTaskQueueRequest request)
        {
            using (var db = new KOCSAMADLSEntities())
            {

                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var user = KOCAuthorizeAttribute.getCurrentUser();
               // var user = new KOCAuthorizedUser { userId = 21204, userRole = 67108896 };
                if (!filter.subTables.ContainsKey("taskid")) filter.subTables.Add("taskid", new DTOFilter("task", "taskid"));

                if((user.userRole & (int)KOCUserTypes.Admin)!=(int)KOCUserTypes.Admin)
                filter.subTables["taskid"].fieldFilters.Add(new DTOFieldFilter {op = 9, value = $"(attachablepersoneltype = (attachablepersoneltype & {user.userRole}))" });

                if ((user.userRole & (int)KOCUserTypes.TeamLeader) != (int)KOCUserTypes.TeamLeader)
                    filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "attachedpersonelid", op = 2, value = user.userId });

                string querySQL = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSQL = filter.getCountSQL();

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

                List<adsl_personel> personels;// personelin stok durumuna ihtiyaç yoksa sorgulanmasın
                if (request.taskOrderNo != null) personels = db.personel.Include(p => p.stockstatus).Where(p => personelIds.Contains(p.personelid)).ToList();
                else
                    personels = db.personel.Where(p => personelIds.Contains(p.personelid)).ToList();

                var customerIds = res.Select(c => c.attachedobjectid).Distinct().ToList();
                var customers = db.customer.Where(c => customerIds.Contains(c.customerid)).ToList();

                var ilIds = res.Select(s => s.attachedcustomer.ilKimlikNo).Distinct().ToList();
                var iller = db.il.Where(i => ilIds.Contains(i.kimlikNo)).ToList();
                
                var ilceIds= res.Select(s => s.attachedcustomer.ilceKimlikNo).Distinct().ToList();
                var ilceler = db.ilce.Where(i => ilceIds.Contains(i.kimlikNo)).ToList();

                var taskstateIds = res.Select(tsp => tsp.status).Distinct().ToList();
                var taskstates = db.taskstatepool.Where(tsp => taskstateIds.Contains(tsp.taskstateid)).ToList();

                //var issIds = res.Where(i => i.attachedcustomer != null && i.attachedcustomer.iss != null).Select(i => i.attachedcustomer.iss).Distinct().ToList();
                //var isss = db.issStatus.Where(i => issIds.Contains(i.id)).ToList();

                //var cststatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.customerstatus != null).Select(c => c.attachedcustomer.customerstatus).Distinct().ToList();
                //var cststatus = db.customer_status.Where(c => cststatusIds.Contains(c.ID)).ToList();

                res.ForEach(r =>
                {
                    r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                    r.attachedpersonel = personels.Where(p => p.personelid == r.attachedpersonelid).FirstOrDefault();
                    r.attachedcustomer = customers.Where(c => c.customerid == r.attachedobjectid).FirstOrDefault();
                    r.attachedcustomer.il = iller.Where(i => i.kimlikNo == r.attachedcustomer.ilKimlikNo).FirstOrDefault();
                    r.attachedcustomer.ilce = ilceler.Where(i => i.kimlikNo == r.attachedcustomer.ilceKimlikNo).FirstOrDefault();
                    //if (r.attachedcustomer != null) r.attachedcustomer.issStatus = isss.Where(i => i.id == (r.attachedcustomer.iss ?? 0)).FirstOrDefault();
                    //if (r.attachedcustomer != null) r.attachedcustomer.customer_status = cststatus.Where(c => c.ID == (r.attachedcustomer.customerstatus ?? 0)).FirstOrDefault();
                    r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                    
                    //r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                    r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();
                    if (request.taskOrderNo != null)
                    {
                        var customerid = res.Select(c => c.attachedobjectid).FirstOrDefault();
                        var salestaskorderno = db.taskqueue.Where(t => t.task.tasktype==1 && t.attachedobjectid == customerid)
                            .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();

                       // taska bağlı müşteri kampanyası ve bilgileri
                           r.customerproduct = db.customerproduct.Include(s => s.campaigns).Where(c => c.taskid == salestaskorderno).ToList();
                       // taska bağlı stock hareketleri
                          r.stockmovement = db.stockmovement.Include(s => s.stockcard).Where(s => s.relatedtaskqueue == r.taskorderno).ToList();
                        var stockcardids = db.taskstatematches.Where(tsm => tsm.taskid == r.taskid && tsm.stateid == r.status && tsm.stockcards != null).ToList()
                        .SelectMany(s => s.stockcards.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(ss => Convert.ToInt32(ss))).ToList();
                        r.stockcardlist = db.stockcard.Where(s => stockcardids.Contains(s.stockid)).ToList();
                        //  sadece task durumuyla ilişkili stockcardid'ler seçiliyor
                        if (stockcardids.Count() > 0)
                        {
                            r.attachedpersonel.stockstatus = r.attachedpersonel.stockstatus.
                               Where(ss => stockcardids.Contains(ss.stockcardid)).ToList();
                            foreach (var ss in r.attachedpersonel.stockstatus)
                            {
                                ss.serials = db.getSerialsOnPersonelAdsl(ss.toobject, ss.stockcardid).ToList();
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
        [KOCAuthorize]
        public HttpResponseMessage saveTaskQueues(DTOtaskqueue tq)
        {

           
            using (var db = new KOCSAMADLSEntities())
            {
                var tsm = db.taskstatematches.Include(t => t.taskstatepool).Where(r => r.taskid == tq.task.taskid && r.stateid == tq.taskstatepool.taskstateid).FirstOrDefault();
                var dtq = db.taskqueue.Include(tt => tt.task)
                                      .Include(p => p.attachedpersonel)
                                      .Include(tsp => tsp.taskstatepool)
                                      .Include(ap => ap.asistanPersonel)
                                      .Include(rt => rt.relatedTaskQueue)
                                      .Include(pt => pt.previousTaskQueue)
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
                    db.Database.ExecuteSqlCommand("update customerproduct set deleted=1, lastupdated=GetDate(), updatedby={0} where deleted=0 and taskid={1} and customerid={2}", new object[] { KOCAuthorizeAttribute.getCurrentUser().userId, tq.taskorderno, tqAttachedobjectid });
                    #endregion

                    #region stok hareketlerini silme
                    foreach (var item in db.stockmovement.Where(r => r.relatedtaskqueue == tq.taskorderno && r.deleted == false))
                    {
                        item.deleted = true;
                        item.updatedby = KOCAuthorizeAttribute.getCurrentUser().userId;
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
                            var amtq = new adsl_taskqueue
                            {
                                appointmentdate = (dtq.task.tasktype == 2) ? (tq.appointmentdate) : (null),
                                attachedpersonelid = (personel_id ? dtq.attachedpersonelid : (null)),
                                attachmentdate = personel_id ? ((dtq.taskstatepool != null ? ((dtq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : (DateTime?)DateTime.Now)) : (null),
                                attachedobjectid = dtq.attachedobjectid,
                                taskid = item,
                                creationdate = DateTime.Now,
                                deleted = false,
                                lastupdated = DateTime.Now,
                                previoustaskorderid = dtq.taskorderno,
                                updatedby = KOCAuthorizeAttribute.getCurrentUser().userId, //User.Identity.PersonelID,
                                relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                            };
                            db.taskqueue.Add(amtq);
                        }
                    }
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


     
                #region Kurulum randevusu kapandığında ikinci donanım tasklarının oluşturulma kodu
                ///*25.10.2014 18:43  OZAL Ek ürün ve retention satış taskları ise yeniden
                // * task türemesini önlemek için kontrol bloğu ekledim
                // */
                //var test = false;
                //var ttqretek = dtq;
                //while (ttqretek != null && ttqretek.task.tasktype != 1 && ttqretek.taskid != 65)
                //    ttqretek = ttqretek.relatedTaskQueue;
                //if (ttqretek != null)
                //{
                //    if (ttqretek.taskid == 6117 || ttqretek.taskid == 6115)
                //        test = true;
                //}
                //if (!test)//Ek ürün veya retention değilse
                //{
                //    /* OZAL 25.10.2014 18:45*/
                //    if ((dtq.task.tasktype == 3 || dtq.task.tasktype == 4) && (dtq.status != null && dtq.taskstatepool.statetype == 1) && (dtq.task.tasktype != 0))
                //    {
                //        var ttq = dtq;
                //        while (ttq != null && ttq.task.tasktype != 1 && ttq.taskid != 65 && ttq.taskid != 69) /* Satış ziyareti veya Yönetim Odası Satışı--nakil taskı *///&& ttq.taskid != 53 27.12.2014 18:40 OZAL
                //            ttq = ttq.relatedTaskQueue;
                //        if (ttq == null)
                //            throw new Exception("Satış Taskı Bulunamadı.");

                //        var cust_pro = db.customerproduct.Where(r => r.taskid == ttq.taskorderno && r.deleted == false).ToList();
                //        foreach (var p in cust_pro.Select(r => r.productid))
                //            foreach (var item in (db.product_service.Where(r => r.productid == p).First().automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                //            {
                //                if (db.taskqueue.Where(r => (r.relatedtaskorderid == tq.taskorderno || r.previoustaskorderid == tq.taskorderno) && r.taskid == item).Any())
                //                    continue;
                //                var personel_id = (db.task.Any(m => m.attachablepersoneltype == dtq.attachedpersonel.category && m.taskid == item));
                //                db.taskqueue.Add(new adsl_taskqueue
                //                {
                //                    attachedpersonelid = personel_id ? dtq.attachedpersonelid : (null),

                //                    attachmentdate = personel_id ? (DateTime?)DateTime.Now : (null),
                //                    attachedobjectid = dtq.attachedobjectid,
                //                    taskid = item,
                //                    creationdate = DateTime.Now,
                //                    deleted = false,
                //                    lastupdated = DateTime.Now,
                //                    previoustaskorderid = dtq.taskorderno,
                //                    //Kullanıcı Kontrolü
                //                    updatedby = KOCAuthorizeAttribute.getCurrentUser().userId,// User.Identity.PersonelID,
                //                    relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                //                });
                //            }
                //        db.SaveChanges();
                //    }
                //}
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
        [KOCAuthorize]
        public HttpResponseMessage closeTaskQueues(DTORequestCloseTaskqueue request)
        {
            using (var db = new KOCSAMADLSEntities())
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
                        cdc.updatedby = KOCAuthorizeAttribute.getCurrentUser().userId;// User.Identity.PersonelID;
                        cdc.deleted = true;
                        cdc.lastupdated = DateTime.Now;
                    }
                }
                #endregion
                #region Yeni kampanya belgeleri ekleniyor
                foreach (var item in newdocs.Where(r => !olddocs.Contains(r)))
                {
                    db.customerdocument.Add(new adsl_customerdocument
                    {
                        creationdate = DateTime.Now,
                        customerid = tq.attachedobjectid,
                        deleted = false,
                        documentid = item,
                        lastupdated = DateTime.Now,
                        taskqueueid = tq.taskorderno,
                        updatedby = KOCAuthorizeAttribute.getCurrentUser().userId,// User.Identity.PersonelID,
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

                    db.customerproduct.Add(new adsl_customerproduct
                    {
                        campaignid = request.campaignid,
                        creationdate = DateTime.Now,
                        customerid = tq.attachedobjectid,
                        deleted = false,
                        lastupdated = DateTime.Now,
                        productid = p,
                        taskid = tq.taskorderno,
                        updatedby = KOCAuthorizeAttribute.getCurrentUser().userId,// User.Identity.PersonelID
                    });
                    #region Ek ürün için otomatik zorunlu taskların türetilmesi-- OZAL 10.10.2014 17:45 ve Retention
                    if (tq.taskid == 6115 || tq.taskid == 6117)
                    {
                        foreach (var item in (db.product_service.Where(r => r.productid == p).First().automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                        {
                            var personel_id = (db.task.Where(t => t.attachablepersoneltype == tq.attachedpersonel.category && t.taskid == item).Any());
                            db.taskqueue.Add(new adsl_taskqueue
                            {

                                appointmentdate = ((item == 4 || item == 55 || item == 72 || item == 68) && (item == 5 || item == 18 || item == 73 || item == 69 || item == 55)) ? (tq.appointmentdate) : (null),

                                attachmentdate = (item == 73) ? (DateTime?)DateTime.Now : (personel_id ? ((tq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : tq.attachmentdate),

                                attachedobjectid = tq.attachedobjectid,
                                taskid = item,
                                creationdate = DateTime.Now,
                                deleted = false,
                                lastupdated = DateTime.Now,
                                previoustaskorderid = tq.taskorderno,
                                updatedby = KOCAuthorizeAttribute.getCurrentUser().userId, //User.Identity.PersonelID,
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

        [Route("saveAdslSalesTask")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage saveSalesTask(DTOcustomer request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            {
                var customer = new customer
                {
                    customername = request.customername,
                    tc = request.tc,
                    gsm = request.gsm,
                    phone = request.phone,
                    ilKimlikNo = request.ilceKimlikNo,
                    ilceKimlikNo = request.ilceKimlikNo,
                    mahalleKimlikNo = request.mahalleKimlikNo,
                    yolKimlikNo = request.yolKimlikNo,
                    binaKimlikNo = request.binaKimlikNo,
                    daire = request.daire,
                    updatedby = user.userId,
                    lastupdated = DateTime.Now,
                    creationdate=DateTime.Now,
                    description = request.description,
                    deleted = false
                };
                db.customer.Add(customer);
                db.SaveChanges();

                var cust = db.customer.Where(c => c.tc == request.tc && c.customername == request.customername).FirstOrDefault();
               
                var taskqueue = new adsl_taskqueue
                {
                    appointmentdate = null,
                    attachedobjectid = cust.customerid,
                    attachedpersonelid =request.salespersonel ?? user.userId,
                    attachmentdate = DateTime.Now,
                    creationdate = DateTime.Now,
                    deleted = false,
                    description = "ADSL Satış",
                    lastupdated = DateTime.Now,
                    status = null,
                    taskid = request.taskid,
                    updatedby = user.userId
                };
                
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
            }
        }

        [Route("saveFaultTask")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage SaveFaultTask(DTORequestSaveFaultTaks request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var taskqueue = new adsl_taskqueue
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
        [KOCAuthorize]
        public HttpResponseMessage personelattachment(DTORequestAttachmentPersonel request)
        {
            if (request.ids.Count() > 0)
            {
                using (var db = new KOCSAMADLSEntities())
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
                using (var db = new KOCSAMADLSEntities())
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
        [KOCAuthorize]
        public HttpResponseMessage saveCustomerCard(DTOKatZiyareti ct)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                if (db.customer.Any(c => c.customerid == ct.customerid))
                {
                    var item = db.customer.Where(c => c.customerid == ct.customerid).First();

                    item.customername = ct.customername;
                    item.gsm = ct.gsm;
                    //if (ct.netStatus.id != 0)
                    //    item.netstatu = ct.netStatus.id;
                    //if (ct.telStatus.id != 0)
                    //    item.telstatu = ct.telStatus.id;
                    //if (ct.gsmKullanımıStatus.id != 0)
                    //    item.gsmstatu = ct.gsmKullanımıStatus.id;
                    //if (ct.issStatus.id != 0)
                    //    item.iss = ct.issStatus.id;
                    //if (ct.TvKullanımıStatus.id != 0)
                    //    item.tvstatu = ct.TvKullanımıStatus.id;
                    //if (ct.telStatus.id != 0)
                    //    item.telstatu = ct.telStatus.id;
                    //item.description = ct.description;
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

        public HttpResponseMessage getTaskQueueHierarchy(int taskqueueid)
        {
            //Önce bu task üzerinde kullanıcı yetkisi var mı baklıalacak
            using (var db = new KOCSAMADLSEntities())
            {
                var taskqueue = db.taskqueue.Where(tq => tq.taskorderno == taskqueueid);
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");//silinecek
            }
        }
     

    
    }
}
