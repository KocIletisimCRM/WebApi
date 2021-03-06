﻿using CRMWebApi.Models;
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
using CRMWebApi.KOCAuthorization;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http.Headers;

namespace CRMWebApi.Controllers
{

    [RoutePrefix("api/Fiber/Taskqueue")]
    public class FiberTaskqueueController : ApiController
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

                var user = KOCAuthorizeAttribute.getCurrentUser();
                if (!filter.subTables.ContainsKey("taskid")) filter.subTables.Add("taskid", new DTOFilter("task", "taskid"));

                if ((user.userRole & (int)FiberKocUserTypes.Admin) != (int)FiberKocUserTypes.Admin)
                    filter.subTables["taskid"].fieldFilters.Add(new DTOFieldFilter { op = 9, value = $"(attachablepersoneltype = (attachablepersoneltype & {user.userRole}))" });

                if ((user.userRole & (int)FiberKocUserTypes.TeamLeader) != (int)FiberKocUserTypes.TeamLeader)
                    filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "attachedpersonelid", op = 2, value = user.userId });


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
                var tasktypeıds = res.Select(r => r.task.tasktype).Distinct().ToList();
                var tasktypes = db.tasktypes.Where(tt => tasktypeıds.Contains(tt.TaskTypeId)).ToList();
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

                var netstatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.netstatu != null).Select(c => c.attachedcustomer.netstatu).Distinct().ToList();
                var netstatus = db.netStatus.Where(c => netstatusIds.Contains(c.id)).ToList();

                var tvStatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.tvstatu != null).Select(c => c.attachedcustomer.tvstatu).Distinct().ToList();
                var tvstatus = db.TvKullanımıStatus.Where(c => tvStatusIds.Contains(c.id)).ToList();

                var telStatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.telstatu != null).Select(c => c.attachedcustomer.telstatu).Distinct().ToList();
                var telstatus = db.telStatus.Where(c => telStatusIds.Contains(c.id)).ToList();

                var ttvStatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.turkcellTv != null).Select(c => c.attachedcustomer.turkcellTv).Distinct().ToList();
                var ttvstatus = db.TurkcellTVStatus.Where(c => ttvStatusIds.Contains(c.id)).ToList();

                var gsmStatusIds = res.Where(c => c.attachedcustomer != null && c.attachedcustomer.gsmstatu != null).Select(c => c.attachedcustomer.gsmstatu).Distinct().ToList();
                var gsmstatus = db.gsmKullanımıStatus.Where(c => gsmStatusIds.Contains(c.id)).ToList();

                var taskorderIds = res.Select(r => r.taskorderno).ToList();
                var editables = db.v_taskorderIsEditableCRM1Set.Where(r => taskorderIds.Contains(r.taskorderno)).ToList();
                res.ForEach(r =>
                                 {
                                     r.task = tasks.Where(t => t.taskid == r.taskid).FirstOrDefault();
                                     r.task.tasktypes = tasktypes.Where(t => t.TaskTypeId == r.task.tasktype).FirstOrDefault();
                                     r.attachedpersonel = personels.Where(p => p.personelid == r.attachedpersonelid).FirstOrDefault();
                                     r.editable = editables.Where(e => e.taskorderno == r.taskorderno).First().editable == 1;
                                     r.attachedcustomer = customers.Where(c => c.customerid == r.attachedobjectid).FirstOrDefault();
                                     if (r.attachedcustomer == null)
                                     {
                                         r.attachedblock = blocks.Where(b => b.blockid == r.attachedobjectid).FirstOrDefault();
                                     }
                                     if (r.attachedcustomer != null) r.attachedcustomer.issStatus = isss.Where(i => i.id == (r.attachedcustomer.iss ?? 0)).FirstOrDefault();
                                     if (r.attachedcustomer != null)
                                     {
                                         r.attachedcustomer.customer_status = cststatus.Where(c => c.ID == (r.attachedcustomer.customerstatus ?? 0)).FirstOrDefault();
                                         r.attachedcustomer.netStatus = netstatus.Where(c => c.id == (r.attachedcustomer.netstatu ?? 0)).FirstOrDefault();
                                         r.attachedcustomer.telStatus = telstatus.Where(c => c.id == (r.attachedcustomer.telstatu ?? 0)).FirstOrDefault();
                                         r.attachedcustomer.TvKullanımıStatus = tvstatus.Where(c => c.id == (r.attachedcustomer.tvstatu ?? 0)).FirstOrDefault();
                                         r.attachedcustomer.TurkcellTVStatus = ttvstatus.Where(c => c.id == (r.attachedcustomer.turkcellTv ?? 0)).FirstOrDefault();
                                         r.attachedcustomer.gsmKullanımıStatus = gsmstatus.Where(c => c.id == (r.attachedcustomer.gsmstatu ?? 0)).FirstOrDefault();
                                     }

                                     r.taskstatepool = taskstates.Where(tsp => tsp.taskstateid == r.status).FirstOrDefault();
                                     r.Updatedpersonel = personels.Where(u => u.personelid == r.updatedby).FirstOrDefault();
                                     r.asistanPersonel = personels.Where(ap => ap.personelid == r.assistant_personel).FirstOrDefault();
                                     if (request.taskOrderNo != null)
                                     {
                                         var customerid = res.Select(c => c.attachedobjectid).FirstOrDefault();
                                         r.previousTaskQueue = db.taskqueue.Where(t => t.taskorderno == r.previoustaskorderid).FirstOrDefault();
                                         var ptq = r.previousTaskQueue;
                                         // artık reletedtaskorderno satış taskını göteriyor 18.03.2017 (Hüseyin KOZ)
                                         var salestaskorderno = db.taskqueue.Where(t => t.task.tasktype == 1 && t.attachedobjectid == customerid)
                                          .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();
                                         //herhangi bir taskın satış taskını bulup o satış taskı ile ilgili ürünleri taska gömmek için yazıldı.
                                         while (ptq != null)
                                         {
                                             ptq.task = db.task.Where(t => t.taskid == ptq.taskid).FirstOrDefault();
                                             var test = ptq.task.tasktype;
                                             if (test == 1)
                                             {
                                                 salestaskorderno = db.taskqueue.Where(t => t.task.tasktype == 1 && t.attachedobjectid == customerid && t.taskorderno == ptq.taskorderno)
                                           .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();
                                                 break;
                                             }
                                             else
                                             {
                                                 ptq.previousTaskQueue = db.taskqueue.Where(t => t.taskorderno == ptq.previoustaskorderid).FirstOrDefault();
                                                 ptq = ptq.previousTaskQueue;
                                             }
                                         }

                                         //taska bağlı müşteri kampanyası ve bilgileri
                                         r.customerproduct = db.customerproduct.Include(s => s.campaigns).Where(c => c.taskid == salestaskorderno && c.deleted == false).ToList();
                                         //taska bağlı stock hareketleri
                                         r.stockmovement = db.stockmovement.Include(s => s.stockcard).Where(s => s.relatedtaskqueue == r.taskorderno).ToList();
                                         var stockcardids = db.taskstatematches.Where(tsm => tsm.taskid == r.taskid && tsm.stateid == r.status && tsm.stockcards != null).ToList()
                                         .SelectMany(s => s.stockcards.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(ss => Convert.ToInt32(ss))).ToList();
                                         var document =
                                         r.stockcardlist = db.stockcard.Where(s => stockcardids.Contains(s.stockid)).ToList();
                                         //sadece task durumuyla ilişkili stockcardid'ler seçiliyor
                                         if (stockcardids.Count() > 0)
                                         {
                                             r.attachedpersonel.stockstatus = r.attachedpersonel.stockstatus.
                                                Where(ss => stockcardids.Contains(ss.stockcardid)).ToList();
                                             foreach (var ss in r.attachedpersonel.stockstatus)
                                             {
                                                 ss.serials = db.getSerialsOnPersonelFiber(ss.toobject, ss.stockcardid).ToList();
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
            List<DTOtaskqueue> listArray = new List<DTOtaskqueue>(); // Hat satışı taskından sonra gelen tasklardan otomatik kapamalar için
            var user = KOCAuthorizeAttribute.getCurrentUser();
            var customerdocuments = tq.customerdocument != null ? tq.customerdocument.Select(cd => ((Newtonsoft.Json.Linq.JObject)(cd)).ToObject<DTOcustomerdocument>()).ToList() : null;
            var customerproducts = tq.customerproduct != null ? tq.customerproduct.Select(cd => ((Newtonsoft.Json.Linq.JObject)(cd)).ToObject<DTOcustomerproduct>()).ToList() : null;
            var stockmovements = tq.stockmovement != null ? tq.stockmovement.Select(cd => ((Newtonsoft.Json.Linq.JObject)(cd)).ToObject<DTOstockmovement>()).ToList() : null;
            //return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            using (var db = new CRMEntities())
            using (var transaction = db.Database.BeginTransaction())
                try
                {
                    var tsm = db.taskstatematches.Include(t => t.taskstatepool).Where(r => r.taskid == tq.task.taskid && r.stateid == tq.taskstatepool.taskstateid).FirstOrDefault();
                    var dtq = db.taskqueue.Include(tt => tt.task)
                                          .Include(p => p.attachedpersonel)
                                          .Include(tsp => tsp.taskstatepool)
                                          .Include(ap => ap.asistanPersonel)
                                          .Include(pt => pt.previousTaskQueue)
                                          .Where(r => r.taskorderno == tq.taskorderno).First();
                    #region Taskın durumu değişmişse (Aynı Zamanda Taskın durumu açığa alınacaksa) yapılacaklar
                    if ((dtq.status != tq.taskstatepool.taskstateid) && (tq.taskstatepool.taskstateid == 0 || tq.taskstatepool.taskstate != "AÇIK"))
                    {
                        if (tq.taskstatepool.taskstateid == 0)
                        {
                            dtq.status = null;
                            dtq.consummationdate = null;
                        }// taskın durumunu açığa alma
                        else
                        {
                            dtq.status = tq.taskstatepool.taskstateid;
                        }

                        #region Değiştirilen taska bağlı taskların hiyerarşik iptali. Bu kod taskın zorunlu task atamalarından önce çalışmalıdır.
                        var automandatoryTasks = new List<int>();
                        if (tsm != null)
                            automandatoryTasks.AddRange((tsm.automandatorytasks ?? "")
                                .Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)).ToList());
                        Queue<taskqueue> delete = new Queue<taskqueue>();
                        foreach (var item in db.taskqueue.Where(r => (r.previoustaskorderid == tq.taskorderno) && (r.deleted == false)))
                            delete.Enqueue(item);
                        while (delete.Count > 0)
                        {
                            var t = delete.Dequeue();
                            foreach (var item in db.taskqueue.Where(r => (r.previoustaskorderid == t.taskorderno) && (r.deleted == false)))
                                delete.Enqueue(item);
                            t.deleted = true;
                            t.description += "\r\nHiyerarşik Olarak Silindi!";
                            t.lastupdated = DateTime.Now;
                            t.updatedby = user.userId;
                        }
                        db.SaveChanges();//foreach (var item in db.taskqueue.Where(r => !automandatoryTasks.Contains(r.taskid) && r.previoustaskorderid == tq.taskorderno && (r.deleted == false)).ToList())
                        //{
                        //    var stateid = db.taskstatematches.Include(s => s.taskstatepool).Where(r => r.taskid == item.taskid &&
                        //          r.automandatorytasks == null && r.taskstatepool.statetype == 2).First().taskstatepool.taskstateid;
                        //    item.status = stateid;
                        //    item.consummationdate = DateTime.Now;
                        //    item.description = "Hiyerarşik Task iptali";
                        //    db.SaveChanges();
                        //}
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
                        foreach (var item in db.stockmovement.Where(r => r.relatedtaskqueue == tq.taskorderno && r.deleted == false).ToList())
                        {
                            item.deleted = true;
                            item.updatedby = user.userId;
                            item.lastupdated = DateTime.Now;
                        }
                        #endregion

                        var docs = new List<int>();

                        var mailInfo = new List<object>();
                        #region Otomatik zorunlu taklar
                        if (tsm != null)
                        {
                            docs.AddRange((tsm.documents ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)));

                            foreach (var item in automandatoryTasks)
                            {
                                if (db.taskqueue.Where(r => r.deleted == false && r.previoustaskorderid == tq.taskorderno && r.taskid == item && (r.status == null || r.taskstatepool.statetype != 2)).Any())
                                    continue;
                                int? personel_id = (db.task.Where(t => ((t.attachablepersoneltype & dtq.attachedpersonel.category) == t.attachablepersoneltype) && t.taskid == item).Any()) ? (int?)dtq.attachedpersonelid : null;
                                var oot = db.task.FirstOrDefault(t => t.taskid == item);
                                if (oot == null) continue;

                                var oott = db.atama.Where(r => r.formedtasktype == oot.tasktype).ToList(); // atama satırı (oluşan task type tanımlamalarda varsa)
                                if (oott.Count > 0)
                                {
                                    var turAtama = oott.Where(t => t.formedtask == null).ToList(); //bir türdeki bütün oluşacak taskların bir personele atanması
                                    var task = oott.Where(r => r.formedtask == item).ToList();  //tür ve task seçilerek kural oluşturulmuşsa
                                    //Task atama kuralları işlesin.
                                    if (task.Count > 0)
                                    {
                                        var kPersonelandTasks = task.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid && r.closedtask == dtq.task.taskid); //kurallarda hem kapatılan task hemde kapatan personel kuralı varsa
                                        var kTask = task.FirstOrDefault(r => r.closedtask == dtq.task.taskid);  // kapatılan task var kapatan personel olmayan kural varsa
                                        var kPersonel = task.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid);  // kapatan personel var kapatılan task null kuralı varsa
                                        if (kPersonelandTasks != null)
                                            personel_id = kPersonelandTasks.appointedpersonel;
                                        else if (kTask != null)
                                            personel_id = kTask.appointedpersonel;
                                        else if (kPersonel != null)
                                            personel_id = kPersonel.appointedpersonel;
                                        else
                                            personel_id = task[0].appointedpersonel;
                                    }
                                    else if (turAtama.Count > 0)
                                    {
                                        var kPersonelandTasks = turAtama.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid && r.closedtask == dtq.task.taskid); //kurallarda hem kapatılan task hemde kapatan personel kuralı varsa
                                        var kTask = turAtama.FirstOrDefault(r => r.closedtask == dtq.task.taskid);  // kapatılan task var kapatan personel olmayan kural varsa
                                        var kPersonel = turAtama.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid);  // kapatan personel var kapatılan task null kuralı varsa
                                        if (kPersonelandTasks != null)
                                            personel_id = kPersonelandTasks.appointedpersonel;
                                        else if (kTask != null)
                                            personel_id = kTask.appointedpersonel;
                                        else if (kPersonel != null)
                                            personel_id = kPersonel.appointedpersonel;
                                        else
                                            personel_id = turAtama[0].appointedpersonel;
                                    }
                                }

                                var amtq = new taskqueue
                                {
                                    appointmentdate = (dtq.task.tasktype == 2) ? (tq.appointmentdate) : (null),
                                    attachedpersonelid = personel_id,
                                    attachmentdate = personel_id != null ? ((dtq.taskstatepool != null ? ((dtq.taskstatepool.statetype == 3) ? (DateTime?)DateTime.Now.AddDays(1) : (DateTime?)DateTime.Now) : (DateTime?)DateTime.Now)) : (null),
                                    attachedobjectid = dtq.attachedobjectid,
                                    taskid = item,
                                    creationdate = DateTime.Now,
                                    deleted = false,
                                    lastupdated = DateTime.Now,
                                    previoustaskorderid = dtq.taskorderno,
                                    updatedby = user.userId, //User.Identity.PersonelID,
                                    relatedtaskorderid = dtq.relatedtaskorderid.HasValue ? dtq.relatedtaskorderid.Value : dtq.taskorderno
                                };
                                db.taskqueue.Add(amtq);
                                #region Hat Satışları Otomatik Task Kapamalar (16.03.2017)
                                /*
                                 * HAT SATIŞINDAN SONRA OLUŞAN FİBER SATIŞ BİREYSEL TASKINI OTOMATİK KAPAT 16.03.2017 (Hüseyin KOZ)
                                 */
                                var ttp = db.task.First(r => r.taskid == amtq.taskid).tasktype;
                                if (dtq.task.tasktype == 10 && ttp == 1)
                                {
                                    db.SaveChanges();
                                    amtq.relatedtaskorderid = amtq.taskorderno;
                                    amtq.attachedpersonelid = dtq.attachedpersonelid;
                                    amtq.attachmentdate = DateTime.Now;
                                    DTOtaskqueue stq = amtq.toDTO<DTOtaskqueue>();
                                    customerproducts.ForEach(tk =>
                                    {
                                        var k = new JObject();
                                        k.Add("productid", tk.productid);
                                        k.Add("campaignid", tk.campaignid);
                                        k.Add("taskid", amtq.taskorderno);
                                        k.Add("customerid", amtq.attachedobjectid);
                                        stq.customerproduct.Add(k.ToObject<object>());
                                    });
                                    stq.task.tasktypes = new DTOTaskTypes();
                                    stq.task.tasktypes.TaskTypeId = ttp;
                                    stq.customerdocument = new List<object>();
                                    stq.stockmovement = new List<object>();
                                    stq.asistanPersonel = new DTOpersonel();
                                    var state = db.taskstatematches.Where(ts => ts.taskid == amtq.taskid && ts.deleted == false).Select(ts => ts.stateid).ToList();
                                    stq.taskstatepool = new DTOtaskstatepool();
                                    foreach (var sid in state)
                                    {
                                        var st = db.taskstatepool.FirstOrDefault(r => r.taskstateid == sid);
                                        if (st != null && st.statetype == 1)
                                        {
                                            stq.taskstatepool.taskstateid = sid.Value;
                                            break;
                                        }
                                    }
                                    stq.description = "Satış Otomatik Olarak Kapatıldı.";
                                    listArray.Add(stq);
                                }
                                // mobil aktivasyon tamamlama otomatik olarak kapatılsın
                                if (amtq.taskid == 10233)
                                {
                                    db.SaveChanges();
                                    amtq.attachedpersonelid = personel_id.HasValue ? personel_id : 21213; // 21213 (Hüseyin KOZ)
                                    amtq.attachmentdate = DateTime.Now;
                                    DTOtaskqueue stq = amtq.toDTO<DTOtaskqueue>();
                                    stq.customerproduct = new List<object>();
                                    stq.customerdocument = new List<object>();
                                    stq.stockmovement = new List<object>();
                                    stq.task.tasktypes = new DTOTaskTypes();
                                    stq.task.tasktypes.TaskTypeId = ttp;
                                    stq.asistanPersonel = new DTOpersonel();
                                    var state = db.taskstatematches.Where(ts => ts.taskid == amtq.taskid && ts.deleted == false).Select(ts => ts.stateid).ToList();
                                    stq.taskstatepool = new DTOtaskstatepool();
                                    foreach (var sid in state)
                                    {
                                        var st = db.taskstatepool.FirstOrDefault(r => r.taskstateid == sid);
                                        if (st != null && st.statetype == 1)
                                        {
                                            stq.taskstatepool.taskstateid = sid.Value;
                                            break;
                                        }
                                    }
                                    stq.description = "Otomatik Olarak Kapatıldı.";
                                    listArray.Add(stq);
                                }
                                #endregion
                            }
                        }
                        #endregion
                        #region kurulum tamamlanınca ürüne bağlı taskların türetilmesi
                        var automandatoryTask = new List<int>();
                        if (tq.task.tasktypes.TaskTypeId == 3 && tq.taskstatepool.statetype == 1)
                        {
                            // firber kurulumlar tamamlandığında müşterinin kartındaki servis sağlayıcı SUPERONLİNE olsun (KOD : 2000) Hüseyin KOZ 08.03.2017
                            var cust = db.customer.FirstOrDefault(r => r.customerid == dtq.attachedobjectid);
                            if (cust != null)
                                cust.iss = 2000;
                            var ptq = dtq.previousTaskQueue;
                            int saletask = tq.taskorderno;
                            while (ptq != null)
                            {
                                ptq.task = db.task.Where(t => t.taskid == ptq.taskid).FirstOrDefault();
                                if (ptq.task.tasktype == 1)
                                {
                                    saletask = ptq.taskorderno; break;
                                }
                                else
                                {
                                    ptq = db.taskqueue.Where(t => t.taskorderno == ptq.previoustaskorderid).FirstOrDefault();
                                }
                            }
                            var custproducts = db.customerproduct.Where(c => c.customerid == dtq.attachedobjectid && c.taskid == saletask && c.deleted == false).Select(s => s.productid).ToList();

                            var autotasks = db.product_service.Where(p => custproducts.Contains(p.productid) && p.automandatorytasks != null).ToList();
                            var tasks = new List<int>();
                            autotasks.ForEach(t =>
                            {
                                tasks.AddRange((t.automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)));
                            });
                            if (autotasks.Count() > 0)
                            {
                                foreach (var item in tasks)
                                {
                                    //eğer oluşacak task müşteri üzeirnde varsa ve durumu null ise yeniden oluşturmasına izin verme.
                                    //var autotaskcontrol = db.taskqueue.Where(t => t.taskid==item && t.status == null && t.attachedobjectid==dtq.attachedobjectid && t.deleted==false).Count();
                                    if (Convert.ToInt32(item) == 6125) continue;
                                    int? personel_id = (db.task.Where(t => ((t.attachablepersoneltype & dtq.attachedpersonel.category) == t.attachablepersoneltype) && t.taskid == item).Any()) ? (int?)dtq.attachedpersonelid : null;
                                    var oot = db.task.FirstOrDefault(t => t.taskid == item);
                                    if (oot == null) continue;

                                    var oott = db.atama.Where(r => r.formedtasktype == oot.tasktype).ToList(); // atama satırı (oluşan task type tanımlamalarda varsa)
                                    if (oott.Count > 0)
                                    {
                                        var turAtama = oott.Where(t => t.formedtask == null).ToList(); //bir türdeki bütün oluşacak taskların bir personele atanması
                                        var task = oott.Where(r => r.formedtask == item).ToList();  //tür ve task seçilerek kural oluşturulmuşsa
                                                                                                    //Task atama kuralları işlesin.
                                        if (task.Count > 0)
                                        {
                                            var kPersonelandTasks = task.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid && r.closedtask == dtq.task.taskid); //kurallarda hem kapatılan task hemde kapatan personel kuralı varsa
                                            var kTask = task.FirstOrDefault(r => r.closedtask == dtq.task.taskid);  // kapatılan task var kapatan personel olmayan kural varsa
                                            var kPersonel = task.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid);  // kapatan personel var kapatılan task null kuralı varsa
                                            if (kPersonelandTasks != null)
                                                personel_id = kPersonelandTasks.appointedpersonel;
                                            else if (kTask != null)
                                                personel_id = kTask.appointedpersonel;
                                            else if (kPersonel != null)
                                                personel_id = kPersonel.appointedpersonel;
                                            else
                                                personel_id = task[0].appointedpersonel;
                                        }
                                        else if (turAtama.Count > 0)
                                        {
                                            var kPersonelandTasks = turAtama.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid && r.closedtask == dtq.task.taskid); //kurallarda hem kapatılan task hemde kapatan personel kuralı varsa
                                            var kTask = turAtama.FirstOrDefault(r => r.closedtask == dtq.task.taskid);  // kapatılan task var kapatan personel olmayan kural varsa
                                            var kPersonel = turAtama.FirstOrDefault(r => r.offpersonel == dtq.attachedpersonelid);  // kapatan personel var kapatılan task null kuralı varsa
                                            if (kPersonelandTasks != null)
                                                personel_id = kPersonelandTasks.appointedpersonel;
                                            else if (kTask != null)
                                                personel_id = kTask.appointedpersonel;
                                            else if (kPersonel != null)
                                                personel_id = kPersonel.appointedpersonel;
                                            else
                                                personel_id = turAtama[0].appointedpersonel;
                                        }
                                    }
                                    db.taskqueue.Add(new taskqueue
                                    {
                                        appointmentdate = null,
                                        attachmentdate = null,
                                        attachedobjectid = dtq.attachedobjectid,
                                        attachedpersonelid = personel_id,
                                        taskid = Convert.ToInt32(item),
                                        creationdate = DateTime.Now,
                                        deleted = false,
                                        lastupdated = DateTime.Now,
                                        previoustaskorderid = tq.taskorderno,
                                        updatedby = KOCAuthorizeAttribute.getCurrentUser().userId,
                                        relatedtaskorderid = dtq.relatedtaskorderid.HasValue ? dtq.relatedtaskorderid.Value : dtq.taskorderno
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }
                        #endregion
                        #region ürünler kaydediliyor
                        foreach (var p in customerproducts)
                        {
                            db.customerproduct.Add(new customerproduct
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

                            if (tq.task.taskid == 6115 || tq.task.taskid == 6117)
                            {
                                foreach (var item in (db.product_service.Where(r => r.productid == p.productid).First().automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)))
                                {
                                    var personel_id = (db.task.Where(t => t.attachablepersoneltype == dtq.attachedpersonel.category && t.taskid == item).Any());
                                    db.taskqueue.Add(new taskqueue
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
                                        relatedtaskorderid = dtq.relatedtaskorderid.HasValue ? dtq.relatedtaskorderid.Value : dtq.taskorderno
                                    });
                                }
                            }
                        }
                        #endregion
                        #region belgeler kaydediliyor
                        if (customerdocuments.Any(c => c.documenturl != null))
                        {
                            db.customerdocument.AddRange(customerdocuments.Select(cd => new customerdocument
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
                        }
                        #endregion
                        #region stok hareketleri kaydediliyor
                        //foreach (var movement in stockmovements)
                        //{
                        //    if (movement.movementid != 0)
                        //    {
                        //        var sm = db.stockmovement.FirstOrDefault(s => s.movementid == movement.movementid);
                        //        if (sm != null)
                        //        {
                        //            sm.amount = movement.amount;
                        //            sm.confirmationdate = DateTime.Now;
                        //            sm.lastupdated = DateTime.Now;
                        //            sm.movementdate = DateTime.Now;
                        //            sm.serialno = movement.serialno;
                        //            sm.updatedby = user.userId;
                        //        }
                        //        else
                        //        {
                        //            throw new Exception("Stok Hareketi Bulunamadı");
                        //        }
                        //    }
                        //    else
                        //    {
                        //        db.stockmovement.Add(new adsl_stockmovement
                        //        {
                        //            amount = movement.amount,
                        //            confirmationdate = DateTime.Now,
                        //            creationdate = DateTime.Now,
                        //            fromobjecttype = (int)KOCUserTypes.TechnicalStuff,
                        //            fromobject = dtq.attachedpersonelid,
                        //            deleted = false,
                        //            lastupdated = DateTime.Now,
                        //            movementdate = DateTime.Now,
                        //            relatedtaskqueue = dtq.taskorderno,
                        //            serialno = movement.serialno,
                        //            stockcardid = movement.stockcardid,
                        //            toobjecttype = (int)KOCUserTypes.ADSLCustomer,
                        //            toobject = dtq.attachedobjectid,
                        //            updatedby = user.userId
                        //        });
                        //    }
                        //}
                        db.stockmovement.AddRange(stockmovements.Select(sm => new stockmovement
                        {
                            amount = sm.amount ?? 1,
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
                                db.customerproduct.Add(new customerproduct
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
                            db.customerdocument.AddRange(customerdocuments.Select(cd => new customerdocument
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
                                db.stockmovement.Add(new stockmovement
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
                                updatedby = user.userId, // User.Identity.PersonelID,
                                relatedtaskorderid = dtq.relatedtaskorderid.HasValue ? dtq.relatedtaskorderid.Value : dtq.taskorderno
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
                                updatedby = user.userId,// User.Identity.PersonelID,
                                relatedtaskorderid = dtq.relatedtaskorderid.HasValue ? dtq.relatedtaskorderid.Value : dtq.taskorderno
                            });
                        }

                        db.SaveChanges();
                    }

                    #endregion
                    dtq.description = tq.description != null ? tq.description : dtq.description;
                    dtq.appointmentdate = (tq.appointmentdate != null) ? tq.appointmentdate : dtq.appointmentdate;
                    dtq.creationdate = (tq.creationdate != null) ? tq.creationdate : dtq.creationdate;
                    dtq.assistant_personel = (tq.asistanPersonel != null && tq.asistanPersonel.personelid != 0) ? tq.asistanPersonel.personelid : dtq.assistant_personel;
                    dtq.consummationdate = tq.consummationdate != null ? tq.consummationdate : (dtq.consummationdate != null) ? dtq.consummationdate : DateTime.Now;
                    dtq.lastupdated = DateTime.Now;
                    db.SaveChanges();
                    transaction.Commit();
                    foreach (var item in listArray) // fiber'li hat satışından oluşan satış taskalrının otomatik kapatılması için (oto kapatılacak taklar hazırnnıp yine buna atanabilir)
                        saveTaskQueues(item);
                    return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
                }
                catch (Exception e)
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

                var customername = request.Form["customer"].ToString();
                var sitename = request.Form["sitename"].ToString();
                var blockname = request.Form["blockname"].ToString();
                string subPath = "C:\\CRMFİBERWEB\\Files" + sitename + '\\' + blockname + '\\' + customername + "\\";
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

        [Route("closeTaskQueues")]
        [HttpPost]
        public HttpResponseMessage closeTaskQueues(DTORequestCloseTaskqueue request)
        {
            using (var db = new CRMEntities())
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
                    db.customerdocument.Add(new customerdocument
                    {
                        creationdate = DateTime.Now,
                        customerid = tq.attachedobjectid,
                        deleted = false,
                        documentid = item,
                        lastupdated = DateTime.Now,
                        taskqueueid = tq.taskorderno,
                        updatedby = user.userId,// User.Identity.PersonelID,
                        attachedobjecttype = tq.task.attachableobjecttype ?? 0
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

                    db.customerproduct.Add(new customerproduct
                    {
                        campaignid = request.campaignid,
                        creationdate = DateTime.Now,
                        customerid = tq.attachedobjectid,
                        deleted = false,
                        lastupdated = DateTime.Now,
                        productid = p,
                        taskid = tq.taskorderno,
                        updatedby = user.userId,// User.Identity.PersonelID
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
                                updatedby = user.userId, //User.Identity.PersonelID,
                                /*25.10.2014 17:33 OZAL  Önceki kod kısmında alttaki satır kapalıydı ve task oluşurken ilişkilendirme yapılamıyordu*/
                                relatedtaskorderid = tq.relatedtaskorderid.HasValue ? tq.relatedtaskorderid.Value : tq.taskorderno
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
                var user = KOCAuthorizeAttribute.getCurrentUser();
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
                    updatedby = user.userId
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.03.2017
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
                var user = KOCAuthorizeAttribute.getCurrentUser();
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
                    updatedby = user.userId,//User.Identity.PersonelID
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.03.2017
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
            }
        }

        [Route("saveZiyaretTask")]
        [HttpPost]
        public HttpResponseMessage saveZiyaretTask(DTORequestKatZiyareti request)
        {
            using (var db = new CRMEntities())
            {
                var user = KOCAuthorizeAttribute.getCurrentUser();
                foreach (var item in request.tasks)
                {
                    var tq = db.taskqueue.Where(t => t.taskorderno == item).FirstOrDefault();
                    tq.lastupdated = DateTime.Now;
                    tq.consummationdate = DateTime.Now;
                    tq.status = request.status;
                    tq.updatedby = user.userId;
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "İşlem Başarılı", "application/json");
            }
        }

        [Route("saveCTStatusWithKatZiyaret")]
        [HttpPost]
        public HttpResponseMessage saveCTStatusWithKatZiyaret(DTORequestKatZiyareti request)
        {
            using (var db = new CRMEntities())
            {
                var user = KOCAuthorizeAttribute.getCurrentUser();
                foreach (var item in request.tasks)
                {
                    var tq = db.taskqueue.Where(t => t.taskorderno == item).FirstOrDefault();
                    var ct = db.customer.Where(c => c.customerid == tq.attachedobjectid).FirstOrDefault();
                    ct.customerstatus = request.cstatus;
                    ct.lastupdated = DateTime.Now;
                    ct.updatedby = user.userId;
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "İşlem Başarılı", "application/json");
            }
        }

        [Route("personelattachment")]
        [HttpPost]
        public HttpResponseMessage personelattachment(DTORequestAttachmentPersonel request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
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
                        if (request.personelid != null)
                        {
                            var apid = db.personel.Where(p => p.personelid == request.personelid).Select(s => s.personelid).FirstOrDefault();
                            foreach (var item in request.ids)
                            {
                                var tq = db.taskqueue.Where(t => t.taskorderno == item).FirstOrDefault();
                                tq.attachedpersonelid = apid;
                                tq.attachmentdate = DateTime.Now;
                                tq.lastupdated = DateTime.Now;
                                tq.updatedby = user.userId;
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
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                taskqueue katziyareti = null; // çoklu müşterilerde eski müşterinin kat ziyaretini alıp yeni oluşturulan müşteriye sıfır şekilde verilmek için tanımlandı
                var item = new customer
                {
                    blockid = ct.block.blockid,
                    customerid = ct.customerid,
                    customername = ct.customername,
                    gsm = ct.gsm,
                    phone = ct.phone,
                    tckimlikno = ct.tckimlikno,
                    lastupdated = DateTime.Now,
                    creationdate = DateTime.Now,
                    updatedby = user.userId,
                    deleted = false,
                    flat = ct.flat,
                    description = ct.description,
                    customerstatus = ct.customer_status.ID,
                    netstatu = (ct.netStatus.id != 0) ? (int?)ct.netStatus.id : null,
                    telstatu = (ct.telStatus.id != 0) ? (int?)ct.telStatus.id : null,
                    gsmstatu = (ct.gsmKullanımıStatus.id != 0) ? (int?)ct.gsmKullanımıStatus.id : null,
                    iss = (ct.issStatus.id != 0) ? (int?)ct.issStatus.id : null,
                    tvstatu = (ct.TvKullanımıStatus.id != 0) ? (int?)ct.TvKullanımıStatus.id : null,
                    turkcellTv = (ct.TurkcellTVStatus.id != 0) ? (int?)ct.TurkcellTVStatus.id : null,
                    superonlineCustNo = ct.superonlineCustNo,
                };
                if (item.customerid == 0)
                {
                    foreach (var cst in db.customer.Where(c => c.blockid == item.blockid && c.flat == item.flat && c.deleted != true))
                    {
                        var kat = db.taskqueue.Where(r => r.attachedobjectid == cst.customerid && r.taskid == 86).OrderByDescending(r => r.taskorderno).FirstOrDefault();
                        if (kat != null)
                            katziyareti = kat; // en son oluşturulan kat ziyareti yakalanmaya çalışıldı yakaladığına ataması yeterli aslında
                        cst.deleted = null;
                    }
                    db.Entry(item).State = EntityState.Added;
                }
                else db.Entry(item).State = EntityState.Modified;

                db.SaveChanges();
                var custid = item.customerid;
                if (katziyareti != null) //kat ziyareti varsa güncelle (sıfır yap)
                {
                    katziyareti.creationdate = DateTime.Now;
                    katziyareti.attachedobjectid = custid;
                    katziyareti.attachmentdate = DateTime.Now;
                    katziyareti.attachedpersonelid = user.userId;
                    katziyareti.appointmentdate = null;
                    katziyareti.status = null;
                    katziyareti.consummationdate = null;
                    katziyareti.description = null;
                    katziyareti.lastupdated = null;
                    katziyareti.updatedby = user.userId;
                    katziyareti.deleted = false;
                    katziyareti.assistant_personel = null;
                    katziyareti.fault = null;
                    db.SaveChanges();
                }
                if (ct.customer_status.ID != 0)
                {
                    var ziyaretTQ = db.taskqueue.Where(t => t.taskid == 86 && t.attachedobjectid == item.customerid).ToList();
                    if (ziyaretTQ.Count > 0)
                    {
                        foreach (var tq in ziyaretTQ)
                        {
                            tq.status = Convert.ToInt32(ct.customer_status.ID);
                            //tq.consummationdate = DateTime.Now;
                            tq.updatedby = user.userId;
                            tq.lastupdated = DateTime.Now;
                        }
                    }
                }
                if (ct.closedKatZiyareti == true)
                {
                    var res = db.taskqueue.Where(tq => tq.attachedobjectid == ct.customerid && tq.taskid == 86 && tq.status == null).FirstOrDefault();
                    if (res != null)
                    {
                        res.status = ct.customer_status.ID;
                    }
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            }
        }

        [Route("getTQStockMovements")]
        [HttpPost]
        public HttpResponseMessage getStockMovements(DTORequestTaskqueueStockMovements request)
        {
            DTOResponseError errormessage = new DTOResponseError();
            using (var db = new CRMEntities())
            {
                var tq = db.taskqueue.Include(t => t.attachedpersonel).Include(t => t.attachedcustomer).Single(t => t.taskorderno == request.taskorderno);
                var stockmovement = getStockmovements(db, request.taskorderno, request.taskid, request.stateid);
                if (!stockmovement.Any()) return Request.CreateResponse(HttpStatusCode.OK, stockmovement, "application/json");
                var scIds = stockmovement.Select(sm => sm.stockcardid).ToList();
                var stockCards = db.stockcard.Where(sc => scIds.Contains(sc.stockid)).ToList();
                var stockStatus = db.getPersonelStockFiber(tq.attachedpersonelid).Where(ss => scIds.Contains(ss.stockid)).ToList();

                // stok kartı seri numaralı ürün ise task personelinin elindeki ürün seri noları alınıyor.

                stockmovement.ForEach(sm =>
                {
                    sm.stockStatus = stockStatus.FirstOrDefault(ss => ss.stockid == sm.stockcardid);
                    if (sm.stockStatus != null)
                    {
                        if (sm.stockStatus.hasserial != false)
                        {
                            sm.stockStatus.serials = db.getSerialsOnPersonelFiber(tq.attachedpersonelid, sm.stockcardid).ToList();

                        }
                        // seçili seri no listede olmayacağı için listeye ekleniyor
                        if (!string.IsNullOrWhiteSpace(sm.serialno))
                            sm.stockStatus.serials.Insert(0, sm.serialno);
                    }
                    sm.stockcard = stockCards.First(sc => sc.stockid == sm.stockcardid);
                    sm.fromobjecttype = (int)FiberKocUserTypes.TechnicalStuff;
                    sm.frompersonel = tq.attachedpersonel;
                    sm.toobjecttype = (int)FiberKocUserTypes.Customer;
                    sm.tocustomer = tq.attachedcustomer;
                });
                //stockmovement.AsParallel().ForAll((sm) =>
                //{
                //    sm.stockStatus = stockStatus.FirstOrDefault(ss => ss.stockid == sm.stockcardid);
                //    if (sm.stockStatus != null)
                //    {
                //        if (sm.stockStatus.hasserial!=false)
                //        {
                //            sm.stockStatus.serials = db.getSerialsOnPersonelFiber(tq.attachedpersonelid, sm.stockcardid).ToList();

                //        }
                //        // seçili seri no listede olmayacağı için listeye ekleniyor
                //        if (!string.IsNullOrWhiteSpace(sm.serialno))
                //            sm.stockStatus.serials.Insert(0, sm.serialno);
                //    }
                //    sm.stockcard = stockCards.First(sc => sc.stockid == sm.stockcardid);
                //    sm.fromobjecttype = (int)FiberKocUserTypes.TechnicalStuff;
                //    sm.frompersonel = tq.attachedpersonel;
                //    sm.toobjecttype = (int)FiberKocUserTypes.Customer;
                //    sm.tocustomer = tq.attachedcustomer;
                //});
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
            using (var db = new CRMEntities())
            {
                var docs = getDocuments(db, request.taskorderno, request.taskid, request.isSalesTask, request.stateid, request.campaignid, request.customerproducts);
                if (docs.Any())
                {
                    var dIds = docs.Select(csd => csd.documentid).ToList();
                    var documents = db.document.Where(d => dIds.Contains(d.documentid)).ToList();
                    docs.AsParallel().ForAll((csd) =>
                    {
                        csd.fiber_document = documents.First(d => d.documentid == csd.documentid);
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, docs.Select(d => d.toDTO()), "application/json");
            }
        }

        [Route("saveTaskCollective")]
        [HttpPost, HttpGet]
        public HttpResponseMessage saveTaskCollective(List<DTOtaskqueue> request)
        { // toplu task kapatmak için oluşturuldu (Hüseyin) 03.11.2016
            using (var db = new CRMEntities())
                foreach (var item in request)
                {
                    // her task tek tek kontrol et, task için stok veya dokuman entegrasyonu varsa kaydetme
                    var ttype = db.task.First(t => t.taskid == item.task.taskid).tasktype;
                    var stype = db.taskstatepool.FirstOrDefault(t => t.taskstateid == item.taskstatepool.taskstateid);
                    if (db.taskstatematches.Any(t => t.taskid == item.task.taskid && t.stateid == item.taskstatepool.taskstateid && t.deleted == false && !(t.stockcards == null || t.stockcards.Trim() == string.Empty))) { }
                    else if (db.taskstatematches.Any(tsm => tsm.deleted == false && tsm.taskid == item.task.taskid && tsm.stateid == item.taskstatepool.taskstateid && !(tsm.documents == null || tsm.documents.Trim() == string.Empty))) { }
                    else if ((ttype == 1 || ttype == 7 || ttype == 8 || ttype == 9) && item.customerproduct.Count > 0 && db.campaigns.Any(c => c.deleted == false && c.id == (Convert.ToInt32(item.customerproduct[0])) && !(c.documents == null || c.documents.Trim() == string.Empty))) { }
                    else
                    {
                        item.task.tasktypes = new DTOTaskTypes();
                        item.task.tasktypes.TaskTypeId = ttype;
                        item.taskstatepool.statetype = stype != null ? stype.statetype : null;
                        item.customerproduct = new List<object>(); // ürün kaydında yanlışlık olmaması için ürünü temizle
                        item.customerdocument = new List<object>();
                        saveTaskQueues(item);
                    }
                }
            return Request.CreateResponse(HttpStatusCode.OK, "Ok", "application/json");
        }

        [Route("getTaskqueueInfo")]
        [HttpPost, HttpGet]
        public HttpResponseMessage getTaskqueueInfo(taskqueue request)
        { // Tasklarda satış ve önceki task bilgilerini çekmek için oluşturuldu (Hüseyin)
            using (var db = new CRMEntities())
            {
                var task = db.taskqueue.Where(r => r.deleted == false && r.taskorderno == request.taskorderno).FirstOrDefault();
                task.task = db.task.FirstOrDefault(r => r.taskid == task.taskid);
                task.attachedpersonel = db.personel.FirstOrDefault(r => r.personelid == task.attachedpersonelid);
                return Request.CreateResponse(HttpStatusCode.OK, task.toDTO(), "application/json");
            }
        }

        private List<stockmovement> getStockmovements(CRMEntities db, int taskorderno, int taskid, int stateid)
        {
            var tsm = db.taskstatematches.FirstOrDefault(t => t.taskid == taskid && t.stateid == stateid && t.deleted == false && !(t.stockcards == null || t.stockcards.Trim() == string.Empty));
            if (tsm == null) return Enumerable.Empty<stockmovement>().ToList();
            var stockcardIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(tsm.stockcards))
            {
                stockcardIds.AddRange(tsm.stockcards.Split(',').Select(s => Convert.ToInt32(s)));
            }
            var stockmovements = db.stockmovement.Where(s => s.relatedtaskqueue == taskorderno && s.deleted == false).ToList();
            return stockcardIds.Select(s => stockmovements.Where(sm => sm.stockcardid == s).FirstOrDefault() ?? new stockmovement
            {
                stockcardid = s,
                relatedtaskqueue = taskorderno
            }).ToList();
        }

        private List<customerdocument> getDocuments(CRMEntities db, int taskorderno, int taskid, bool isSalesTask, int stateid, int? campaignid = null, List<int> customerproducts = null)
        {
            var customerid = db.taskqueue.Single(tq => tq.taskorderno == taskorderno && tq.deleted == false).attachedobjectid;
            // müsteri evrağı alınıyor
            var customerdocuments = db.customerdocument.Where(cd => cd.customerid == customerid && cd.deleted == false && cd.taskqueueid == taskorderno).ToList();
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
            return documents.Select(d => customerdocuments.Where(cd => cd.documentid == d).FirstOrDefault() ?? new customerdocument
            {
                documentid = d,
                customerid = customerid,
                taskqueueid = taskorderno
            }).ToList();
        }
    }
}
