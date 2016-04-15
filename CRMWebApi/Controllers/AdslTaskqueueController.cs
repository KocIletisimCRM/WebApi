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
using Newtonsoft.Json;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/TaskQueues")]

    [KOCAuthorize]
    public class AdslTaskqueueController : ApiController
    {
        [Route("getTaskQueues")]
        [HttpPost]
        public HttpResponseMessage getTaskQueues(DTOGetTaskQueueRequest request)
        {
            using (var db = new KOCSAMADLSEntities(false))
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var user = KOCAuthorizeAttribute.getCurrentUser();
                // var user = new KOCAuthorizedUser { userId = 21204, userRole = 67108896 };
                if (!filter.subTables.ContainsKey("taskid")) filter.subTables.Add("taskid", new DTOFilter("task", "taskid"));

                if ((user.userRole & (int)FiberKocUserTypes.Admin) != (int)FiberKocUserTypes.Admin)
                    filter.subTables["taskid"].fieldFilters.Add(new DTOFieldFilter { op = 9, value = $"(attachablepersoneltype = (attachablepersoneltype & {user.userRole}))" });

                if ((user.userRole & (int)FiberKocUserTypes.TeamLeader) != (int)FiberKocUserTypes.TeamLeader)
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

                var tasktypeıds = res.Select(r => r.task.tasktype).Distinct().ToList();
                var tasktypes = db.tasktypes.Where(tt => tasktypeıds.Contains(tt.TaskTypeId)).ToList();

                var personelIds = res.Select(r => r.attachedpersonelid)
                    .Union(res.Select(r => r.assistant_personel))
                    .Union(res.Select(r => r.updatedby)).Distinct().ToList();

                var personels = db.personel.Where(p => personelIds.Contains(p.personelid)).ToList();

                var customerIds = res.Select(c => c.attachedobjectid).Distinct().ToList();
                var customers = db.customer.Where(c => customerIds.Contains(c.customerid)).ToList();

                var ilIds = res.Select(s => s.attachedcustomer.ilKimlikNo).Distinct().ToList();
                var iller = db.il.Where(i => ilIds.Contains(i.kimlikNo)).ToList();

                var ilceIds = res.Select(s => s.attachedcustomer.ilceKimlikNo).Distinct().ToList();
                var ilceler = db.ilce.Where(i => ilceIds.Contains(i.kimlikNo)).ToList();

                var taskstateIds = res.Select(tsp => tsp.status).Distinct().ToList();
                var taskstates = db.taskstatepool.Where(tsp => taskstateIds.Contains(tsp.taskstateid)).ToList();

                var taskorderIds = res.Select(r => r.taskorderno).ToList();
                var editables = db.v_taskorderIsEditable.Where(r => taskorderIds.Contains(r.taskorderno)).ToList();


                res.ForEach(r =>
                {
                    r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                    r.task.tasktypes = tasktypes.Where(t => t.TaskTypeId == r.task.tasktype).FirstOrDefault();
                    r.attachedpersonel = personels.Where(p => p.personelid == r.attachedpersonelid).FirstOrDefault();
                    r.attachedcustomer = customers.Where(c => c.customerid == r.attachedobjectid).FirstOrDefault();
                    r.attachedcustomer.il = iller.Where(i => i.kimlikNo == r.attachedcustomer.ilKimlikNo).FirstOrDefault();
                    r.attachedcustomer.ilce = ilceler.Where(i => i.kimlikNo == r.attachedcustomer.ilceKimlikNo).FirstOrDefault();
                    r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                    //r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                    r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();
                    r.editable = editables.Where(e => e.taskorderno == r.taskorderno).First().editable == 1;
                    if (request.taskOrderNo != null)
                    {
                        var customerid = res.Select(c => c.attachedobjectid).FirstOrDefault();
                        var salestaskorderno = db.taskqueue.Where(t => (t.task.tasktype == 1 || t.task.tasktype == 8 || t.task.tasktype == 9) && t.attachedobjectid == customerid)
                            .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();

                        // taska bağlı müşteri kampanyası ve bilgileri
                        r.customerproduct = db.customerproduct.Include(s => s.campaigns).Where(c => c.taskid == salestaskorderno && c.deleted == false).ToList();
                        r.customerdocument = getDocuments(db, r.taskorderno, r.taskid, (r.task.tasktype == 1 || r.task.tasktype == 8 || r.task.tasktype == 9), r.status ?? 0, r.customerproduct.Any() ? r.customerproduct.First().campaignid : null, r.customerproduct.Select(cp => cp.productid ?? 0).ToList());
                        if (r.customerdocument.Any())
                        {
                            var dIds = r.customerdocument.Select(csd => csd.documentid).ToList();
                            var documents = db.document.Where(d => dIds.Contains(d.documentid)).ToList();
                            r.customerdocument.AsParallel().ForAll((csd) =>
                            {
                                csd.adsl_document = documents.First(d => d.documentid == csd.documentid);
                            });
                        }
                        // taska bağlı stok hareketleri yükleniyor
                        if (r.status != null)
                        {
                            r.stockmovement = getStockmovements(db, r.taskorderno, r.taskid, r.status ?? 0);
                            var scIds = r.stockmovement.Select(sm => sm.stockcardid).ToList();
                            var stockCards = db.stockcard.Where(sc => scIds.Contains(sc.stockid)).ToList();
                            var stockStatus = db.getPersonelStockAdsl(r.attachedpersonelid).Where(ss => scIds.Contains(ss.stockid)).ToList();
                            // stok kartı seri numaralı ürün ise task personelinin elindeki ürün seri noları alınıyor.
                            r.stockmovement.AsParallel().ForAll((sm) =>
                            {
                                sm.stockStatus = stockStatus.FirstOrDefault(ss => ss.stockid == sm.stockcardid);
                                if (sm.stockStatus != null)
                                {
                                    sm.stockStatus.serials = db.getSerialsOnPersonelAdsl(r.attachedpersonelid, sm.stockcardid).ToList();
                                    // seçili seri no listede olmayacağı için listeye ekleniyor
                                    if (!string.IsNullOrWhiteSpace(sm.serialno))
                                        sm.stockStatus.serials.Insert(0, sm.serialno);
                                }
                                sm.stockcard = stockCards.First(sc => sc.stockid == sm.stockcardid);
                                sm.fromobjecttype = (int)KOCUserTypes.TechnicalStuff;
                                sm.frompersonel = r.attachedpersonel;
                                sm.toobjecttype = (int)KOCUserTypes.ADSLCustomer;
                                sm.tocustomer = r.attachedcustomer;
                            });
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
            var user = KOCAuthorizeAttribute.getCurrentUser();
            var customerdocuments =tq.customerdocument!=null? tq.customerdocument.Select(cd => ((Newtonsoft.Json.Linq.JObject)(cd)).ToObject<DTOcustomerdocument>()).ToList():null;
            var customerproducts =tq.customerproduct!=null? tq.customerproduct.Select(cd => ((Newtonsoft.Json.Linq.JObject)(cd)).ToObject<DTOcustomerproduct>()).ToList():null;
            var stockmovements =tq.stockmovement!=null ?tq.stockmovement.Select(cd => ((Newtonsoft.Json.Linq.JObject)(cd)).ToObject<DTOstockmovement>()).ToList():null;
            //return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            using (var db = new KOCSAMADLSEntities())
            using (var transaction = db.Database.BeginTransaction())
                try
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
                    if ((dtq.status != tq.taskstatepool.taskstateid) && (tq.taskstatepool.taskstateid == 0 || tq.taskstatepool.taskstate != "AÇIK"))
                    {
                        if (tq.taskstatepool.taskstateid == 0)
                        {
                            dtq.consummationdate = null;
                            dtq.status = null;
                        }// taskın durumunu açığa alma
                        else
                        {
                            dtq.consummationdate = DateTime.Now;
                            dtq.status = tq.taskstatepool.taskstateid;
                        }

                        #region Değiştirilen taska bağlı taskların hiyerarşik iptali. Bu kod taskın zorunlu task atamalarından önce çalışmalıdır.
                        var automandatoryTasks = new List<int>();
                        if (tsm != null)
                            automandatoryTasks.AddRange((tsm.automandatorytasks ?? "")
                                .Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)).ToList());
                        Queue<adsl_taskqueue> delete = new Queue<adsl_taskqueue>();
                        foreach (var item in db.taskqueue.Where(r => (r.relatedtaskorderid == tq.taskorderno || r.previoustaskorderid == tq.taskorderno) && (r.deleted == false)))
                            delete.Enqueue(item);
                        while (delete.Count > 0)
                        {
                            var t = delete.Dequeue();
                            foreach (var item in db.taskqueue.Where(r => (r.relatedtaskorderid == t.taskorderno || r.previoustaskorderid == t.taskorderno) && (r.deleted == false)))
                                delete.Enqueue(item);
                            t.deleted = true;
                            t.description += "\r\nHiyerarşik Olarak Silindi!";
                            t.lastupdated = DateTime.Now;
                            t.updatedby = user.userId;
                        }
                        db.SaveChanges();
                        #endregion

                        #region Browser tarafından attachedobjectid gönderilemediği için taskqueue tablosundan attachedobjectid ye erişme işlemi
                        var tqAttachedobjectid = dtq.attachedobjectid;
                        #endregion

                        #region zaten var olan müşteri ürünlerini silme(ilgili taskla ilişkili olanları)...
                        db.Database.ExecuteSqlCommand("update customerproduct set deleted=1, lastupdated=GetDate(), updatedby={0} where deleted=0 and taskid={1} and customerid={2}", new object[] { user.userId, tq.taskorderno, tqAttachedobjectid });
                        #endregion
                        #region zaten var olan müşteri dökümanlarını silme(ilgili taskla ilişkili olanları)...
                        db.Database.ExecuteSqlCommand("update customerdocument set deleted=1, lastupdated=GetDate(), updatedby={0} where deleted=0 and taskqueueid={1} and customerid={2}", new object[] { user.userId, tq.taskorderno, tqAttachedobjectid });
                        #endregion

                        #region stok hareketlerini silme
                        foreach (var item in db.stockmovement.Where(r => r.relatedtaskqueue == tq.taskorderno && r.deleted == false))
                        {
                            item.deleted = true;
                            item.updatedby = user.userId;
                            item.lastupdated = DateTime.Now;
                        }
                        #endregion

                        var docs = new List<int>();
                        var mailInfo = new List<object>();
                        #region Otomatik zorunlu tasklar
                        if (tsm != null)
                        {
                            docs.AddRange((tsm.documents ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)));

                            foreach (var item in automandatoryTasks)
                            {
                                if (db.taskqueue.Where(r => r.deleted==false && (r.relatedtaskorderid == tq.taskorderno || r.previoustaskorderid == tq.taskorderno) && r.taskid == item && (r.status == null || r.taskstatepool.statetype != 2)).Any())
                                    continue;  
                                int? personel_id = (db.task.Where(t => ((t.attachablepersoneltype & dtq.attachedpersonel.category) == t.attachablepersoneltype) && t.taskid == item).Any()) ? (int?)dtq.attachedpersonelid : null;
                                //Otomatik Kurulum Bayisi Ataması (Oluşan task kurulum taskı ise)
                                var oot = db.task.FirstOrDefault(t => t.taskid == item);
                                if (oot == null) continue;
                                if (oot.tasktype == 2)
                                {
                                    var ptq = dtq;
                                    int? saletask = null;
                                    while (ptq != null)
                                    {
                                        ptq.task = db.task.Where(t => t.taskid == ptq.taskid).FirstOrDefault();
                                        if (ptq.task.tasktype == 1)
                                        {
                                            saletask = ptq.taskorderno;
                                            break;
                                        }
                                        else
                                        {
                                            ptq = db.taskqueue.Where(t => t.taskorderno == ptq.previoustaskorderid).FirstOrDefault();
                                        }
                                    }
                                    if (saletask != null)
                                    {
                                        var satbayi = db.taskqueue.First(r=>r.taskorderno==saletask).attachedpersonelid;
                                        personel_id = db.personel.First(p => p.personelid == satbayi).kurulumpersonelid;//Kurulum bayisi idsi
                                    }
                                    //Satış taskını bul. Taskı yapanın kurulum bayisini al. Kurulum taskını bu bayiyie ata
                                }
                                if (oot.tasktype == 3)
                                {
                                    var ptq = dtq;
                                    int? krtask = null;
                                    while (ptq != null)
                                    {
                                        ptq.task = db.task.Where(t => t.taskid == ptq.taskid).FirstOrDefault();
                                        if (ptq.task.tasktype == 2)
                                        {
                                            krtask = ptq.taskorderno;
                                            break;
                                        }
                                        else
                                        {
                                            ptq = db.taskqueue.Where(t => t.taskorderno == ptq.previoustaskorderid).FirstOrDefault();
                                        }
                                    }
                                    if (krtask != null)
                                    {
                                        var kbayi = db.taskqueue.First(r => r.taskorderno == krtask).attachedpersonelid;
                                        personel_id = db.personel.First(p => p.personelid == kbayi).kurulumpersonelid;//Kurulum bayisi idsi
                                    }
                                }
                                if (item == 45)  // Evrak Onayı Saha Taskı oluşuyorsa satış yapan bayinin kanal yöneticisine ata
                                {
                                    var ptq = dtq;
                                    int? saletask = null;
                                    while (ptq != null)
                                    {
                                        ptq.task = db.task.Where(t => t.taskid == ptq.taskid).FirstOrDefault();
                                        if (ptq.task.tasktype == 1)
                                        {
                                            saletask = ptq.taskorderno;
                                            break;
                                        }
                                        else
                                        {
                                            ptq = db.taskqueue.Where(t => t.taskorderno == ptq.previoustaskorderid).FirstOrDefault();
                                        }
                                    }
                                    if (saletask != null)
                                    {
                                        var satbayi = db.taskqueue.First(r => r.taskorderno == saletask).attachedpersonelid;
                                        personel_id = db.personel.First(p => p.personelid == satbayi).relatedpersonelid;  //Bayi Kanal Yöneticisi
                                    }
                                    //Satış taskını bul. Satış yapanın kanal yöneticisini al. Evrak alma onayı saha taskını bu personele ata
                                }
                                //Diğer otomatik personel atamaları ()
                                var oott = db.atama.Where(r => r.formedtasktype == oot.tasktype).ToList(); // atama satırı (oluşan task type tanımlamalarda varsa)
                                if (oott.Count > 0)
                                {
                                    var turAtama = oott.FirstOrDefault(t=> t.formedtask == null); //bir türdeki bütün oluşacak taskların bir personele atanması
                                    var task = oott.FirstOrDefault(r => r.formedtask == item);  //tür ve task seçilerek kural oluşturulmuşsa
                                    //Task atama kuralları işlesin.
                                    if (task != null)
                                        personel_id = task.appointedpersonel;
                                    else if (turAtama != null)
                                        personel_id = turAtama.appointedpersonel;
                                }
                                var amtq = new adsl_taskqueue
                                {
                                    appointmentdate = (dtq.task.tasktype == 2) ? (tq.appointmentdate) : (null),
                                    attachedpersonelid = personel_id,
                                    attachmentdate = personel_id != null? ((dtq.taskstatepool != null ? ((dtq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : (DateTime?)DateTime.Now)) : (null),
                                    attachedobjectid = dtq.attachedobjectid,
                                    taskid = item,
                                    creationdate = DateTime.Now,
                                    deleted = false,
                                    lastupdated = DateTime.Now,
                                    previoustaskorderid = dtq.taskorderno,
                                    updatedby = user.userId, //User.Identity.PersonelID,
                                    relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? dtq.taskorderno : dtq.relatedtaskorderid
                                };
                                if((automandatoryTasks.Contains(38) || automandatoryTasks.Contains(60)) && dtq.attachedpersonelid!=1016)
                                {
                                    mailInfo.Add(dtq.attachedobjectid);
                                    mailInfo.Add(dtq.attachedpersonelid);
                                    sendemail(mailInfo);
                                }

                                db.taskqueue.Add(amtq);
                            }
                        }

                        #endregion
                        #region kurulum tamamlanınca ürüne bağlı taskların türetilmesi
                        if ((tq.task.taskid==41 && tq.taskstatepool.taskstateid==9117) || (tq.task.taskid == 49 && tq.taskstatepool.taskstateid == 9129))
                        {
                            var custproducts = db.customerproduct.Where(c => c.customerid == dtq.attachedobjectid && c.deleted == false).Select(s => s.productid).ToList();
                            var autotasks = db.product_service.Where(p => custproducts.Contains(p.productid) && p.automandatorytasks != null).Select(s=>s.automandatorytasks).ToList();
                            if (autotasks.Count>0)
                            {
                                foreach (var item in (autotasks.First() ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                                {
                                    var oot = db.task.FirstOrDefault(t => t.taskid == item);
                                    if (oot == null) continue;
                                    int? personel_id = null;
                                    var oott = db.atama.Where(r => r.formedtasktype == oot.tasktype).ToList(); // atama satırı (oluşan task type tanımlamalarda varsa)
                                    if (oott.Count > 0)
                                    {  
                                        //Task atama kuralları işlesin.
                                        var turAtama = oott.FirstOrDefault(t => t.formedtask == null); //bir türdeki bütün oluşacak taskların bir personele atanması
                                        var task = oott.FirstOrDefault(r => r.formedtask == item);  //tür ve task seçilerek kural oluşturulmuşsa
                                        if (task != null)
                                            personel_id = task.appointedpersonel;
                                        else if (turAtama != null)
                                            personel_id = turAtama.appointedpersonel;
                                    }
                                    db.taskqueue.Add(new adsl_taskqueue
                                    {
                                        attachedpersonelid = personel_id,
                                        appointmentdate = null,
                                        attachmentdate = null,
                                        attachedobjectid = dtq.attachedobjectid,
                                        taskid = item,
                                        creationdate = DateTime.Now,
                                        deleted = false,
                                        lastupdated = DateTime.Now,
                                        previoustaskorderid = tq.taskorderno,
                                        updatedby = KOCAuthorizeAttribute.getCurrentUser().userId,
                                        relatedtaskorderid = tq.relatedtaskorderid
                                    });
                                }
                            }                            
                        }
                        #endregion
                        #region ürünler kaydediliyor
                        foreach (var p in customerproducts)
                        {
                                db.customerproduct.Add(new adsl_customerproduct
                            {
                                campaignid = p.campaignid,
                                creationdate = DateTime.Now,
                                customerid = dtq.attachedobjectid,
                                deleted = false,
                                lastupdated = DateTime.Now,
                                productid = p.productid,
                                taskid = dtq.taskorderno,
                                updatedby = user.userId
                            });
                           

                            if (tq.task.taskid == 88 && tq.taskstatepool.taskstateid != 9116)
                            {
                                foreach (var item in (db.product_service.Where(r => r.productid == p.productid).First().automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                                {
                                    var personel_id = (db.task.Where(t => t.attachablepersoneltype == dtq.attachedpersonel.category && t.taskid == item).Any());
                                    db.taskqueue.Add(new adsl_taskqueue
                                    {
                                        appointmentdate = ((item == 4 || item == 55 || item == 72 || item == 68) && (item == 5 || item == 18 || item == 73 || item == 69 || item == 55)) ? (tq.appointmentdate) : (null),
                                        attachmentdate = (item == 73) ? (DateTime?)DateTime.Now : (personel_id ? ((tq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : tq.attachmentdate),
                                        attachedobjectid = dtq.attachedobjectid,
                                        taskid = item,
                                        creationdate = DateTime.Now,
                                        deleted = false,
                                        lastupdated = DateTime.Now,
                                        previoustaskorderid = tq.taskorderno,
                                        updatedby = user.userId,
                                        relatedtaskorderid = tsm.taskstatepool.statetype == 1 ? tq.taskorderno : tq.relatedtaskorderid
                                    });
                                }
                            }
                            db.SaveChanges();
                        }
                        #endregion
                        #region belgeler kaydediliyor
                        if (customerdocuments.Any(c => c.documenturl != null))
                        {
                            db.customerdocument.AddRange(customerdocuments.Select(cd => new adsl_customerdocument
                            {
                                attachedobjecttype = (int)KOCUserTypes.ADSLCustomer,
                                creationdate = DateTime.Now,
                                customerid = dtq.attachedobjectid,
                                deleted = false,
                                //deliverydate = ?
                                documentid = cd.documentid,
                                documenturl = cd.documenturl,
                                lastupdated = DateTime.Now,
                                //receiptdate = ?
                                taskqueueid = dtq.taskorderno,
                                updatedby = user.userId
                            }));
                            db.SaveChanges();
                        }
                        #endregion
                        #region stok hareketleri kaydediliyor
                        db.stockmovement.AddRange(stockmovements.Select(sm => new adsl_stockmovement
                        {
                            amount = sm.amount,
                            confirmationdate = DateTime.Now,
                            creationdate = DateTime.Now,
                            fromobjecttype = (int)KOCUserTypes.TechnicalStuff,
                            fromobject = dtq.attachedpersonelid,
                            deleted = false,
                            lastupdated = DateTime.Now,
                            movementdate = DateTime.Now,
                            relatedtaskqueue = dtq.taskorderno,
                            serialno = sm.serialno,
                            stockcardid = sm.stockcardid,
                            toobjecttype = (int)KOCUserTypes.ADSLCustomer,
                            toobject = dtq.attachedobjectid,
                            updatedby = user.userId
                        }));
                        db.SaveChanges();
                        #endregion
                    }
                    #endregion
                    else
                    {
                        #region ürünler kaydediliyor
                        if (customerproducts.Any(cp => cp.id != 0))
                        {
                            foreach (var p in customerproducts)
                            {
                                    db.customerproduct.Add( new adsl_customerproduct
                                {
                                    campaignid = p.campaignid,
                                    creationdate = DateTime.Now,
                                    customerid = dtq.attachedobjectid,
                                    deleted = false,
                                    lastupdated = DateTime.Now,
                                    productid = p.productid,
                                    taskid = dtq.taskorderno,
                                    updatedby = user.userId
                                });
                            }
                        }
                        #endregion
                        #region belgeler kaydediliyor
                        if (customerdocuments.Any(cd => cd.id != 0))
                            db.customerdocument.AddRange(customerdocuments.Select(cd => new adsl_customerdocument
                            {
                                attachedobjecttype = (int)KOCUserTypes.ADSLCustomer,
                                creationdate = DateTime.Now,
                                customerid = dtq.attachedobjectid,
                                deleted = false,
                                //deliverydate = ?
                                documentid = cd.documentid,
                                documenturl = cd.documenturl,
                                lastupdated = DateTime.Now,
                                //receiptdate = ?
                                taskqueueid = dtq.taskorderno,
                                updatedby = user.userId
                            }));
                        #endregion
                        #region stok hareketleri kaydediliyor
                        foreach (var movement in stockmovements)
                        {
                            if (movement.movementid != 0)
                            {
                                var sm = db.stockmovement.FirstOrDefault(s => s.movementid == movement.movementid);
                                if (sm != null)
                                {
                                    sm.amount = movement.amount;
                                    sm.confirmationdate = DateTime.Now;
                                    sm.lastupdated = DateTime.Now;
                                    sm.movementdate = DateTime.Now;
                                    sm.serialno = movement.serialno;
                                    sm.updatedby = user.userId;
                                }
                                else
                                {
                                    throw new Exception("Stok Hareketi Bulunamadı");
                                }
                            }
                            else
                            {
                                db.stockmovement.Add(new adsl_stockmovement
                                {
                                    amount = movement.amount,
                                    confirmationdate = DateTime.Now,
                                    creationdate = DateTime.Now,
                                    fromobjecttype = (int)KOCUserTypes.TechnicalStuff,
                                    fromobject = dtq.attachedpersonelid,
                                    deleted = false,
                                    lastupdated = DateTime.Now,
                                    movementdate = DateTime.Now,
                                    relatedtaskqueue = dtq.taskorderno,
                                    serialno = movement.serialno,
                                    stockcardid = movement.stockcardid,
                                    toobjecttype = (int)KOCUserTypes.ADSLCustomer,
                                    toobject = dtq.attachedobjectid,
                                    updatedby = user.userId
                                });
                            }
                        }
                        //if (!stockmovements.Any(sm => sm.movementid != 0))
                        //    db.stockmovement.AddRange(stockmovements.Select(sm => new adsl_stockmovement
                        //    {
                        //        amount = sm.amount,
                        //        confirmationdate = DateTime.Now,
                        //        creationdate = DateTime.Now,
                        //        fromobjecttype = (int)KOCUserTypes.TechnicalStuff,
                        //        fromobject = dtq.attachedpersonelid,
                        //        deleted = false,
                        //        lastupdated = DateTime.Now,
                        //        movementdate = DateTime.Now,
                        //        relatedtaskqueue = dtq.taskorderno,
                        //        serialno = sm.serialno,
                        //        stockcardid = sm.stockcardid,
                        //        toobjecttype = (int)KOCUserTypes.ADSLCustomer,
                        //        toobject = dtq.attachedobjectid,
                        //        updatedby=user.userId
                        //    }));
                        #endregion
                    }

                    dtq.description = tq.description != null ? tq.description : dtq.description;
                    dtq.appointmentdate = (tq.appointmentdate != null) ? tq.appointmentdate : dtq.appointmentdate;
                    dtq.creationdate = (tq.creationdate != null) ? tq.creationdate : dtq.creationdate;
                    dtq.assistant_personel = (tq.asistanPersonel.personelid != 0) ? tq.asistanPersonel.personelid : dtq.assistant_personel;
                    dtq.lastupdated = DateTime.Now;
                    db.SaveChanges();
                    transaction.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return Request.CreateResponse(HttpStatusCode.OK, "error", "application/json");
                }
        }

        [HttpPost, Route("upload")]
        public HttpResponseMessage saveFiles()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var docids = JsonConvert.DeserializeObject<List<int>>(request.Form["documentids"]);
                var req = JsonConvert.DeserializeObject<DTOtaskqueue>(request.Form["tq"]);
               // var user = KOCAuthorizeAttribute.getCurrentUser();

                var custid = Convert.ToInt32(request.Form["customer"].Split('-')[0]);
                var il = request.Form["il"].ToString();
                var ilce = request.Form["ilce"].ToString();
                string subPath = "C:\\CRMADSLWEB\\Files\\" + il + '\\' + ilce + '\\' + request.Form["customer"] + "\\";
                System.IO.Directory.CreateDirectory(subPath);
                //var filePath = subPath + ((request.Files["file_data"])).FileName;
                for (int i = 0; i < request.Files.Count; i++)
                {
                    // var filePath = subPath + (i+1).ToString() + "." + request.Files[i].FileName.Split('.')[1];
                    var filePath = subPath + (request.Files[i]).FileName;
                    using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        (request.Files[i]).InputStream.CopyTo(fs);
                    }
                    var doc = req.customerdocument[i] as JObject;
                    doc["documenturl"] = filePath;
                }
                return saveTaskQueues(req);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "error", "application/json"); ;
            }
        }

        [HttpPost, Route("download")]
        public HttpResponseMessage Post()
        {
            var path = @"C:\\CRMADSLWEB\\Files\\TRABZON\\ORTAHİSAR\\5562-ŞEREF BEKTAŞOĞLU\\deneme.jpeg";
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition
                = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "deneme.jpeg";
            result.Content.Headers.ContentLength = stream.Length;
            return result;

        }

        //[HttpPost, Route("download")]
        //public HttpResponseMessage GetFile()
        //{
        //    HttpResponseMessage result = null;
        //    var localFilePath = @"C:\\CRMADSLWEB\\Files\\TRABZON\\ORTAHİSAR\\5562-ŞEREF BEKTAŞOĞLU\\deneme.jpeg";

        //    if (!File.Exists(localFilePath))
        //    {
        //        result = Request.CreateResponse(HttpStatusCode.Gone);
        //    }
        //    else
        //    {
        //        // Serve the file to the client
        //        result = Request.CreateResponse(HttpStatusCode.OK);
        //        result.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
        //        result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
        //        result.Content.Headers.ContentDisposition.FileName = "deneme.jpeg";
        //    }

        //    return result;
        //}
        [Route("closeTaskQueues")]
        [HttpPost]
        public HttpResponseMessage closeTaskQueues(DTORequestCloseTaskqueue request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var user = KOCAuthorizeAttribute.getCurrentUser();
                var tq = db.taskqueue.Include(t => t.attachedpersonel).Where(r => r.taskorderno == request.taskorderno).First();
                var cdocs = tq.customerproduct.Where(r => r.deleted == false);
                var olddocs = (cdocs.Any()) ? (cdocs.First().campaigns.documents ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)).ToList() : new List<int>();
                var newdocs = (db.campaigns.Where(r => r.id == request.campaignid).First().documents ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r));
                #region Eski kampanya belgeleri siliniyor
                foreach (var item in olddocs.Where(r => !newdocs.Contains(r)))
                {
                    foreach (var cdc in db.customerdocument.Where(r => r.documentid == item && r.taskqueueid == tq.taskorderno && r.deleted == false))
                    {
                        cdc.updatedby = user.userId;// User.Identity.PersonelID;
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
                        updatedby = user.userId,// User.Identity.PersonelID,
                        attachedobjecttype = 3000
                    });
                }
                #endregion
                var tsm = db.taskstatematches.Where(r => r.taskid == tq.taskid && r.stateid == tq.status).FirstOrDefault();
                #region Eski ürünler siliniyor
                db.Database.ExecuteSqlCommand("update customerproduct set deleted=1, lastupdated=GetDate(), updatedby={0} where taskid={1}", new object[] { user.userId, tq.taskorderno });
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
        public HttpResponseMessage saveSalesTask(DTOcustomer request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            {
                var oldCust = db.customer.Where(c => c.tc == request.tc).ToList();
                if (oldCust.Count == 0)
                {
                    var customer = new customer
                    {
                        customername = request.customername.ToUpper(),
                        tc = request.tc,
                        gsm = request.gsm,
                        phone = request.phone,
                        ilKimlikNo = request.ilKimlikNo,
                        ilceKimlikNo = request.ilceKimlikNo,
                        bucakKimlikNo = request.bucakKimlikNo,
                        mahalleKimlikNo = request.mahalleKimlikNo,
                        yolKimlikNo = request.yolKimlikNo,
                        binaKimlikNo = request.binaKimlikNo,
                        daire = request.daire,
                        updatedby = user.userId,
                        description = request.description,
                        lastupdated = DateTime.Now,
                        creationdate = DateTime.Now,
                        deleted = false,
                        email = request.email,
                        superonlineCustNo = request.superonlineCustNo,
                    };
                    db.customer.Add(customer);
                    db.SaveChanges();

                    var cust = db.customer.Where(c => c.tc == request.tc && c.customername == request.customername).FirstOrDefault();

                    var taskqueue = new adsl_taskqueue
                    {
                        appointmentdate = request.appointmentdate,
                        attachedobjectid = cust.customerid,
                        attachedpersonelid = request.salespersonel ?? user.userId,
                        attachmentdate = DateTime.Now,
                        creationdate = DateTime.Now,
                        deleted = false,
                        description = request.taskdescription,
                        lastupdated = DateTime.Now,
                        status = null,
                        taskid = request.taskid,
                        updatedby = user.userId,
                        fault=request.fault
                    };

                    db.taskqueue.Add(taskqueue);
                    db.SaveChanges();

                    if (request.productids!=null)
                    {
                        foreach (var item in request.productids)
                        {
                            var customerproducst = new adsl_customerproduct
                            {
                                taskid = taskqueue.taskorderno,
                                customerid = customer.customerid,
                                productid = item,
                                campaignid = request.campaignid,
                                creationdate = DateTime.Now,
                                lastupdated = DateTime.Now,
                                updatedby = user.userId,
                                deleted = false
                            };
                            db.customerproduct.Add(customerproducst);
                            db.SaveChanges();
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Girilen TC Numarası Başkasına Aittir", "application/json");
            }
        }

        [Route("saveFaultTask")]
        [HttpPost]
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
                    updatedby = KOCAuthorizeAttribute.getCurrentUser().userId
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
            }
        }

        [Route("confirmCustomer")]
        [HttpPost]
        public HttpResponseMessage confirmCustomer(DTOcustomer request)
        {
            using (var db=new KOCSAMADLSEntities(false))
            {
                var res = db.customer.Include(i=>i.il).Include(m=>m.ilce).Where(c => c.tc == request.tc && c.deleted == false).FirstOrDefault();              
                if (res !=null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,res.toDTO(), "application/json");
                }
                else
                {
                    DTOResponseError error = new DTOResponseError();
                    error.errorCode = -1;
                    return Request.CreateResponse(HttpStatusCode.OK,error.errorCode, "application/json");
                }
            }

        }

        [Route("insertTaskqueue")]
        [HttpPost]
        public HttpResponseMessage insertTaskqueue(DTOtaskqueue request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities(false))
            {
                adsl_taskqueue taskqueue = new adsl_taskqueue
                {
                    taskid = request.task.taskid,
                    attachedobjectid = request.attachedcustomer.customerid,
                    attachedpersonelid=user.userId,
                    creationdate = DateTime.Now,
                    attachmentdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    updatedby = user.userId,
                    deleted = false
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
                        if (request.personelid != null)
                        {
                            var apid = db.personel.Where(p => p.personelid == request.personelid).Select(s => s.personelid).FirstOrDefault();
                            foreach (var item in request.ids)
                            {
                                var tq = db.taskqueue.Where(t => t.taskorderno == item).FirstOrDefault();
                                tq.attachedpersonelid = apid;
                                tq.attachmentdate = DateTime.Now;
                                tq.lastupdated = DateTime.Now;
                                tq.updatedby = KOCAuthorizeAttribute.getCurrentUser().userId;
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

        public HttpResponseMessage getTaskQueueHierarchy(int taskqueueid)
        {
            //Önce bu task üzerinde kullanıcı yetkisi var mı baklıalacak
            using (var db = new KOCSAMADLSEntities())
            {
                var taskqueue = db.taskqueue.Where(tq => tq.taskorderno == taskqueueid);
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");//silinecek
            }
        }

        [Route("getTQStockMovements")]
        [HttpPost]
        public HttpResponseMessage getStockMovements(DTORequestTaskqueueStockMovements request)
        {
            DTOResponseError errormessage = new DTOResponseError();
            using (var db = new KOCSAMADLSEntities())
            {
                var tq = db.taskqueue.Include(t => t.attachedpersonel).Include(t => t.attachedcustomer).Single(t => t.taskorderno == request.taskorderno);
                var stockmovement = getStockmovements(db, request.taskorderno, request.taskid, request.stateid);
                if (!stockmovement.Any()) return Request.CreateResponse(HttpStatusCode.OK, stockmovement, "application/json");
                var scIds = stockmovement.Select(sm => sm.stockcardid).ToList();
                var stockCards = db.stockcard.Where(sc => scIds.Contains(sc.stockid)).ToList();
                var stockStatus = db.getPersonelStockAdsl(tq.attachedpersonelid).Where(ss => scIds.Contains(ss.stockid)).ToList();

                // stok kartı seri numaralı ürün ise task personelinin elindeki ürün seri noları alınıyor.
                stockmovement.AsParallel().ForAll((sm) =>
                {
                    sm.stockStatus = stockStatus.FirstOrDefault(ss => ss.stockid == sm.stockcardid);
                    if (sm.stockStatus != null)
                    {
                        sm.stockStatus.serials = db.getSerialsOnPersonelAdsl(tq.attachedpersonelid, sm.stockcardid).ToList();
                        // seçili seri no listede olmayacağı için listeye ekleniyor
                        if (!string.IsNullOrWhiteSpace(sm.serialno))
                            sm.stockStatus.serials.Insert(0, sm.serialno);
                    }
                    sm.stockcard = stockCards.First(sc => sc.stockid == sm.stockcardid);
                    sm.fromobjecttype = (int)KOCUserTypes.TechnicalStuff;
                    sm.frompersonel = tq.attachedpersonel;
                    sm.toobjecttype = (int)KOCUserTypes.ADSLCustomer;
                    sm.tocustomer = tq.attachedcustomer;
                });
                return Request.CreateResponse(HttpStatusCode.OK, stockmovement.Select(sm => sm.toDTO()), "application/json");
                //}
                //else
                //{
                //    errormessage.errorMessage = "Elinizde Kurulumu Kapatabilmek İçin Gerekli Donanımlar Yok";
                //    return Request.CreateResponse(HttpStatusCode.OK,errormessage, "application/json");
                //}
            }
        }

        [Route("getTQDocuments")]
        [HttpPost]
        public HttpResponseMessage getDocuments(DTORequestTaskqueueDocuments request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var docs = getDocuments(db, request.taskorderno, request.taskid, request.isSalesTask, request.stateid, request.campaignid, request.customerproducts);
                if (docs.Any())
                {
                    var dIds = docs.Select(csd => csd.documentid).ToList();
                    var documents = db.document.Where(d => dIds.Contains(d.documentid)).ToList();
                    docs.AsParallel().ForAll((csd) =>
                    {
                        csd.adsl_document = documents.First(d => d.documentid == csd.documentid);
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, docs.Select(d => d.toDTO()), "application/json");
            }
        }

        private List<adsl_stockmovement> getStockmovements(KOCSAMADLSEntities db, int taskorderno, int taskid, int stateid)
        {
            var tsm = db.taskstatematches.FirstOrDefault(t => t.taskid == taskid && t.stateid == stateid && t.deleted == false && !(t.stockcards == null || t.stockcards.Trim() == string.Empty));
            if (tsm == null) return Enumerable.Empty<adsl_stockmovement>().ToList();
            var stockcardIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(tsm.stockcards))
            {
                stockcardIds.AddRange(tsm.stockcards.Split(',').Select(s => Convert.ToInt32(s)));
            }
            var stockmovements = db.stockmovement.Where(s => s.relatedtaskqueue == taskorderno && s.deleted == false).ToList();
            return stockcardIds.Select(s => stockmovements.Where(sm => sm.stockcardid == s).FirstOrDefault() ?? new adsl_stockmovement
            {
                stockcardid = s,
                relatedtaskqueue = taskorderno
            }).ToList();
        }

        private List<adsl_customerdocument> getDocuments(KOCSAMADLSEntities db, int taskorderno, int taskid, bool isSalesTask, int stateid, int? campaignid = null, List<int> customerproducts = null)
        {
            var customerid = db.taskqueue.Single(tq => tq.taskorderno == taskorderno && tq.deleted == false).attachedobjectid;
            // müsteri evrağı alınıyor
            var customerdocuments = db.customerdocument.Where(cd => cd.customerid == customerid && cd.deleted == false).ToList();
            // gerekli evrak bilgileri alınıyor
            var documents = new List<int>();
            if (db.taskstatematches.Any(tsm => tsm.deleted == false && tsm.taskid == taskid && tsm.stateid == stateid && !(tsm.documents == null || tsm.documents.Trim() == string.Empty)))
                documents.AddRange(db.taskstatematches.First(tsm => tsm.deleted == false && tsm.taskid == taskid && tsm.stateid == stateid && !(tsm.documents == null || tsm.documents.Trim() == string.Empty)).documents.Split(',').Select(si => Convert.ToInt32(si)));
            if (isSalesTask)
            {
                if (db.campaigns.Any(c => c.deleted == false && c.id == (campaignid ?? 0) && !(c.documents == null || c.documents.Trim() == string.Empty)))
                    documents.AddRange(db.campaigns.First(c => c.deleted == false && c.id == (campaignid ?? 0) && !(c.documents == null || c.documents.Trim() == string.Empty)).documents.Split(',').Select(si => Convert.ToInt32(si)));
                if (customerproducts != null && db.product_service.Any(ps => ps.deleted == false && customerproducts.Contains(ps.productid) && !(ps.documents == null || ps.documents.Trim() == string.Empty)))
                    documents.AddRange(db.product_service.First(ps => ps.deleted == false && customerproducts.Contains(ps.productid) && !(ps.documents == null || ps.documents.Trim() == string.Empty)).documents.Split(',').Select(si => Convert.ToInt32(si)));
            }
            return documents.Select(d => customerdocuments.Where(cd => cd.documentid == d).FirstOrDefault() ?? new adsl_customerdocument
            {
                documentid = d,
                customerid = customerid,
                taskqueueid = taskorderno
            }).ToList();
        }

        public void sendemail(List<object> info)
        {

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("adslbayidestek@kociletisim.com.tr"); // Mail'in kimden olduğu adresi buraya yazılır.
            mail.Subject = "KOÇ İLETİŞİM RANDEVU TASKI"; // mail'in konusu
            int customer =Convert.ToInt32(info[0].ToString());
            int bayiid = Convert.ToInt32(info[1]);

            using (var db = new KOCSAMADLSEntities(false))
            {
                var bayi = db.personel.Where(p => p.personelid == bayiid).FirstOrDefault();
                var customerinfo = db.customer.Where(c => c.customerid == customer).FirstOrDefault();
                for (int i = 0; i < bayi.email.ToString().Split(';').Count(); i++)
                {
                    mail.To.Add(bayi.email.ToString().Split(';')[i]); // kime gönderileceği adresin yazıldığı textbox'tan alınır.

                }
                mail.Body=  string.Format(@" Merhaba Sayın  {0}, {1}  {2}
                       Merkezimizden {3} adlı müşterimizin Kurulum Randevu Taskı size atanmıştır.Müşterimizin İletişim Numarası {4} 'dır.{5} İyi Çalışmalar Dileriz",
                       bayi.personelname.ToUpper().ToString(), 
                       Environment.NewLine,
                       Environment.NewLine,
                       customerinfo.customername,
                       customerinfo.gsm,
                       Environment.NewLine);
             // mail'in ana kısmı, içeriği.. 
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587); // gmail üzerinden gönderileceğinden smtp.gmail.com ve onun 587 nolu portu kullanılır.

            smtp.Credentials = new NetworkCredential("yazilimkoc@gmail.com", "612231Tb"); //hangi e-posta üzerinden gönderileceği. E posta, şifre'si yazılır.

            smtp.EnableSsl = true;

            try
            {
                smtp.Send(mail); // mail gönderilir.
            }
            catch (Exception)
            {
                throw ;
            }
            }
        }
    }
}