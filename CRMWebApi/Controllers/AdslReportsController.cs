using CRMWebApi.Models.Adsl;
using CRMWebApi.DTOs.Adsl;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Linq;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using System.Text;
using CRMWebApi.KOCAuthorization;
using System.Data.SqlClient;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Reports")]
    public class AdslReportsController : ApiController
    {
        [Route("getPersonelWorks")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage getPersonelWorks(DTOs.Adsl.DTOpersonel request)
        {

            using (var db = new KOCSAMADLSEntities(false))
            {

                var res = db.taskqueue.Include(s => s.attachedcustomer).Include(c => c.attachedcustomer.il).Include(c => c.attachedcustomer.ilce).
                    Include(t => t.task).Include(d => d.customerdocument).Include(c => c.customerproduct).
                    Where(p => p.attachedpersonelid == request.personelid && p.deleted == false && (p.task.tasktype == 2 || p.task.tasktype == 3 || p.task.tasktype == 4 ||
                    p.task.tasktype == 7 || p.task.tasktype == 9) && p.status == null).OrderBy(s => s.attachedcustomer.customername).ToList();
                res.ForEach(r =>
                {
                    var salestaskorderno = db.taskqueue.Where(t => (t.task.tasktype == 1 || t.task.tasktype == 8 || t.task.tasktype == 9) && t.attachedobjectid == r.attachedobjectid)
                           .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();

                    r.customerproduct = db.customerproduct.Include(s => s.campaigns).Where(c => c.taskid == salestaskorderno).ToList();
                    r.description = db.taskqueue.Where(s => s.taskorderno == salestaskorderno).Select(s => s.description).FirstOrDefault();
                });
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()), "application/json");
            }
        }

        [Route("BSLOrt")]
        [HttpPost]
        public async Task<HttpResponseMessage> BSLOrt(SLTime bid)
        {
            await WebApiConfig.updateAdslData();
            var d = DateTime.Now;
            var dtr = new DateTimeRange { start = (d - d.TimeOfDay).AddDays(1 - d.Day), end = (d.AddDays(1 - d.Day).AddMonths(1).AddDays(-1)).Date.AddDays(1) };
            var dtr2 = new DateTimeRange { start = dtr.start.AddMonths(-1), end = dtr.end.AddMonths(-1) };
            var slOrt = new double[] { getBayiSLOrt(bid.BayiID.Value, dtr), getBayiSLOrt(bid.BayiID.Value, dtr2) };
            return Request.CreateResponse(HttpStatusCode.OK, slOrt, "application/json");
        }

        [Route("KSLOrt")]
        [HttpPost]
        public async Task<HttpResponseMessage> KSLOrt()
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            var d = DateTime.Now;
            var dtr = new DateTimeRange { start = (d - d.TimeOfDay).AddDays(1 - d.Day), end = (d.AddDays(1 - d.Day).AddMonths(1).AddDays(-1)).Date.AddDays(1) };
            var dtr2 = new DateTimeRange { start = dtr.start.AddMonths(-1), end = dtr.end.AddMonths(-1) };
            var slOrt = new double[] { getKocKurulumSLOrt(dtr), getKocKurulumSLOrt(dtr2) };
            return Request.CreateResponse(HttpStatusCode.OK, slOrt, "application/json");
        }

        //bayi prim ve servis hakediş miktarları
        private static Dictionary<int, SKPayment> getPayment(DateTimeRange request)
        {
            Dictionary<int, SKPayment> total = new Dictionary<int, SKPayment>();
            WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                var ktk_tq = r.Ktk_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                return ktk_tq != null && ktk_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(ktk_tq.status.Value) && WebApiConfig.AdslStatus[ktk_tq.status.Value].statetype.Value == 1 && ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).Select(r =>
             {
                 var s_tq = WebApiConfig.AdslTaskQueues[r.S_TON];
                 var k_tq = r.K_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.K_TON.Value] : null;
                 if (k_tq != null && k_tq.attachedpersonelid.Value == s_tq.attachedpersonelid.Value && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 1)
                 { // satış taskı; satış ise ve satış yapan bayi kurulum yapmışsa bayinin sat-kur hakedişine 1 ekle
                     if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                     total[s_tq.attachedpersonelid.Value].sat_kur++;
                 }
                 else if (WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 9 && k_tq != null)
                 { // satış taskı; CC Satışı ise bayinin kurulum hakedişine 1 ekle
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].kur++;
                 }
                 else if (k_tq != null && k_tq.attachedpersonelid.Value != s_tq.attachedpersonelid.Value && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 1)
                 { // satış taskı; bayi satış taskı ise satış yapan bayinin satış, kurulum yapan bayinin kurulum hakedişine 1 ekle
                     if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                     total[s_tq.attachedpersonelid.Value].sat++;
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].kur++;
                 }
                 else if (k_tq != null && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 8)
                 { // satış taskı; cc teslimat ise bayinin teslimat hakedişine 1 ekle
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].teslimat++;
                 }
                 else if (k_tq != null && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 6)
                 { // satış taskı; arıza ise bayinin arıza hakedişine 1 ekle
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].ariza++;
                 }
                 else if (k_tq != null && k_tq.attachedpersonelid.Value == s_tq.attachedpersonelid.Value && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 12)
                 { // satış taskı; teslimat ise ve satış yapan bayi teslimat yapmışsa bayinin d-sat-tes hakedişine 1 ekle
                     if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                     total[s_tq.attachedpersonelid.Value].d_sat_tes++;
                 }
                 else if (((k_tq != null && k_tq.attachedpersonelid.Value != s_tq.attachedpersonelid.Value) || k_tq == null) && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 12)
                 { // satış taskı; teslimat taskı ise satış yapan bayinin d-sat, kurulum yapan bayinin teslimat hakedişine 1 ekle
                     if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                     total[s_tq.attachedpersonelid.Value].d_sat++;
                     if (k_tq != null)
                     {
                         if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                         total[k_tq.attachedpersonelid.Value].teslimat++;
                     }
                 }
                 else if (WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 14)
                 { // satış taskı; Dsmart kontrol taskı ise satış yapan bayinin d-sat hakedişine 1 ekle
                     if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                     total[s_tq.attachedpersonelid.Value].d_sat++;
                 }
                 return true;
             }).ToList();
            WebApiConfig.AdslTaskQueues.Where(r =>
            {
                var tq = r.Value; // Sam sdm sipariş kapama taskid = 90 (process iptal dahi olsa bayi ekrakları alıp koç personel sam sdm kapatmışsa bayi hakediş alacak)
                return tq.taskid == 90 && tq.status != null && WebApiConfig.AdslStatus.ContainsKey(tq.status.Value) && WebApiConfig.AdslStatus[tq.status.Value].statetype.Value == 1 && tq.consummationdate.HasValue && tq.consummationdate.Value >= request.start && tq.consummationdate.Value <= request.end;
            }).Select(r =>
            {
                var s_tq = r.Value.relatedtaskorderid.HasValue ? WebApiConfig.AdslTaskQueues.ContainsKey(r.Value.relatedtaskorderid.Value) ? WebApiConfig.AdslTaskQueues[r.Value.relatedtaskorderid.Value] : null : null;
                if (s_tq != null && s_tq.taskid == 33 || s_tq.taskid == 64) // başlangıç task tipi ekrak alma olamaz evrak alma tasklarının idleri ile kontrol edildi (Hüseyin KOZ) !!
                { // evrak alınmışsa bayinin evrak alma hakedişine 1 ekle
                    if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                    total[s_tq.attachedpersonelid.Value].evrak++;
                }
                else if (s_tq != null && s_tq.taskid == 122)
                { // çağrı churn bölge içiyse evrak taskının personeline evrak hakedişi ver
                    HashSet<int> tt = new HashSet<int>();
                    var subs = new Queue<int>();
                    if (WebApiConfig.AdslSubTasks.TryGetValue(s_tq.taskorderno, out tt))
                        foreach (var item in tt) subs.Enqueue(item);
                    while (subs.Count > 0)
                    {
                        var p = subs.Dequeue();
                        if (WebApiConfig.AdslSubTasks.TryGetValue(p, out tt))
                            foreach (var item in tt) subs.Enqueue(item);
                        if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTasks.ContainsKey(WebApiConfig.AdslTaskQueues[p].taskid) && WebApiConfig.AdslTasks[WebApiConfig.AdslTaskQueues[p].taskid].tasktype == 7)
                        {
                            total[WebApiConfig.AdslTaskQueues[p].attachedpersonelid.Value].evrak++;
                            break;
                        }
                    }
                }
                return true;
            }).ToList();
            return total;
        }

        // bayi id ve tarih aralığı gönderildiğinde bayinin ortalama sl hesabı
        private static double getBayiSLOrt(int BayiId, DateTimeRange request)
        {
            int sayCust = 0;
            double timeOrt = 0;
            var maxSL = 24; //WebApiConfig.AdslSl.OrderByDescending(k => k.Value.BayiMaxTime).Select(k => k.Value.BayiMaxTime).FirstOrDefault(); // sl tablosunda bayiler için tanımlı maxSL'lerin en büyüğü (çarpan için)
            WebApiConfig.AdslProccesses.Values.SelectMany(
                p => p.SLs.Where(sl => sl.Value.BayiID.HasValue && sl.Value.BayiID == BayiId && sl.Value.BEnd.HasValue && sl.Value.BEnd.Value >= request.start && sl.Value.BEnd.Value <= request.end)
                .Select(bsl =>
                {
                    sayCust++;
                    var r = new SLBayiReport();
                    double factor = 1; // bayi max sl süreleri eşit olmadığından ortalama alabilmek için (Tüm SL'lerin En büyüğü) (bayiMaxSL / buradaki SL) çarpan'ı sayısı kullanıldı (Hüseyin KOZ)
                    if (WebApiConfig.AdslSl.ContainsKey(bsl.Key))
                    {
                        var bayiSl = WebApiConfig.AdslSl[bsl.Key];
                        factor = (bayiSl.BayiMaxTime != null) ? (maxSL / bayiSl.BayiMaxTime.Value) : 1;
                    }
                    r.BayiSLTaskStart = bsl.Value.BStart;
                    r.BayiSLEnd = bsl.Value.BEnd;
                    timeOrt += ((r.BayiSLEnd - r.BayiSLStart).Value.TotalHours) * factor;
                    return true;
                })
            ).ToList();
            return Math.Round(sayCust == 0 ? 0 : timeOrt / sayCust, 2);
        }
        
        // Koç sl total
        private static double getKocSLOrt(DateTimeRange request)
        {
            int sayCust = 0;
            double timeOrt = 0;
            var maxSL = 48; // koç sl 48 üzerinden değerlendirilecek
            WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                var ktk_tq = r.Ktk_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                return ktk_tq != null && ktk_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(ktk_tq.status.Value) && WebApiConfig.AdslStatus[ktk_tq.status.Value].statetype.Value == 1 && ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).SelectMany(
                p => p.SLs.Where(s => s.Value.KEnd.HasValue && s.Value.KStart.HasValue).Select(ksl =>
                {
                    sayCust++;
                    var ston = WebApiConfig.AdslTaskQueues[p.S_TON];
                    var r = new SLKocReport();
                    double factor = 1; // koc max sl süreleri eşit olmadığından ortalama alabilmek için (Tüm SL'lerin En büyüğü) (kocMaxSL / buradaki SL) çarpan'ı sayısı kullanıldı (Hüseyin KOZ)
                    if (WebApiConfig.AdslSl.ContainsKey(ksl.Key))
                    {
                        var kocSl = WebApiConfig.AdslSl[ksl.Key];
                        var slname = kocSl.SLName;
                        factor = kocSl.KocMaxTime != null ? (maxSL / kocSl.KocMaxTime.Value) : 1;
                    }
                    r.KocSLStart = ksl.Value.KStart;
                    r.KocSLEnd = ksl.Value.KEnd;
                    timeOrt += ((r.KocSLEnd - r.KocSLStart).Value.TotalHours) * factor;
                    return true;
                })
            ).ToList();
            return Math.Round(sayCust == 0 ? 0 : timeOrt / sayCust, 2);
        }

        // Koç'un kurulum ve kurumsal kurulum sl'i 48 saat üzerinden
        private static double getKocKurulumSLOrt(DateTimeRange request)
        {
            int sayCust = 0;
            double timeOrt = 0;
            var maxSL = 48; // koç sl 48 üzerinden değerlendirilecek
            WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                var ktk_tq = r.Ktk_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                var s_tq = WebApiConfig.AdslTaskQueues[r.S_TON];
                var stype = WebApiConfig.AdslTasks.ContainsKey(s_tq.taskid) ? WebApiConfig.AdslTasks[s_tq.taskid].tasktype : 0;
                return stype == 9 && ktk_tq != null && ktk_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(ktk_tq.status.Value) && WebApiConfig.AdslStatus[ktk_tq.status.Value].statetype.Value == 1 && ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).SelectMany(
                p => p.SLs.Where(s => s.Value.KEnd.HasValue && s.Value.KStart.HasValue && WebApiConfig.AdslSl.ContainsKey(s.Key) && (WebApiConfig.AdslSl[s.Key].SLID == 1 || WebApiConfig.AdslSl[s.Key].SLID == 4)).Select(ksl =>
                { // SLID 1/4 Kurulum/Kurumsal kurulum sl
                    sayCust++;
                    var r = new SLKocReport();
                    double factor = 1; // koc max sl süreleri eşit olmadığından ortalama alabilmek için (Tüm SL'lerin En büyüğü) (kocMaxSL / buradaki SL) çarpan'ı sayısı kullanıldı (Hüseyin KOZ)
                    var kocSl = WebApiConfig.AdslSl[ksl.Key];
                    var slname = kocSl.SLName;
                    factor = kocSl.KocMaxTime != null ? (maxSL / kocSl.KocMaxTime.Value) : 1;
                    r.KocSLStart = ksl.Value.KStart;
                    r.KocSLEnd = ksl.Value.KEnd;
                    timeOrt += ((r.KocSLEnd - r.KocSLStart).Value.TotalHours) * factor;
                    return true;
                })
            ).ToList();
            return Math.Round(sayCust == 0 ? 0 : timeOrt / sayCust, 2);
        }

        // Hakediş Raporu
        public static async Task<List<SKPaymentReport>> getPaymentReport(DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            var SKPay = getPayment(request);
            return SKPay.Select(r =>
            {
                var bSLOrt = getBayiSLOrt(r.Key, request);
                var res = new SKPaymentReport();
                res.bId = r.Key;
                res.bSLOrt = bSLOrt;
                res.satAdet = r.Value.sat;
                res.stkrAdet = r.Value.sat_kur;
                res.kurAdet = r.Value.kur;
                res.arzAdet = r.Value.ariza;
                res.evrAdet = r.Value.evrak;
                res.tesAdet = r.Value.teslimat;
                res.doSatAdet = r.Value.d_sat;
                res.doSat_tesAdet = r.Value.d_sat_tes;
                if (WebApiConfig.AdslPersonels.ContainsKey(r.Key))
                {
                    var personel = WebApiConfig.AdslPersonels[r.Key];
                    res.bName = personel.personelname;
                    res.il = personel.ilKimlikNo != null ? WebApiConfig.AdslIls.ContainsKey((int)personel.ilKimlikNo) ? WebApiConfig.AdslIls[(int)personel.ilKimlikNo].ad : "İl Yok" : "İl Yok";
                }
                else
                {
                    res.bName = "İsimsiz Personel";
                    res.il = "İl Yok";
                }
                // payment kayıt sıralaması hep küçükten büyüğe olmalı
                res.sat = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 1 && r.Value.sat <= h.Value.upperLimitAmount).Select(h => h.Value.payment).FirstOrDefault() * r.Value.sat) ?? 0;
                res.kur = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 2 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.kur) ?? 0;
                res.sat_kur = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 3 && r.Value.sat_kur <= h.Value.upperLimitAmount && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.sat_kur) ?? 0;
                res.ariza = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 4 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.ariza) ?? 0;
                res.teslimat = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 5 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.teslimat) ?? 0;
                res.evrak = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 6 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.evrak) ?? 0;
                res.doSat = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 7 && r.Value.d_sat <= h.Value.upperLimitAmount).Select(h => h.Value.payment).FirstOrDefault() * r.Value.d_sat) ?? 0;
                res.doSat_tes = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 8 && r.Value.d_sat_tes <= h.Value.upperLimitAmount && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.d_sat_tes) ?? 0;
                return res;
            }).ToList();
        }

        // Bütün bekleyen tasklar
        public static async Task<List<SKStandbyTaskReport>> getSKStandbyTaskReport()
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslTaskQueues.Where(r=> (r.Value.status == null || r.Value.status.Value == 9159 || r.Value.status.Value == 9165) && r.Value.deleted == false).Select(r =>
            {
                var res = new SKStandbyTaskReport();
                res.taskorderno = r.Value.taskorderno;
                res.taskid = r.Value.taskid;
                res.taskname = WebApiConfig.AdslTasks.ContainsKey(r.Value.taskid) ? WebApiConfig.AdslTasks[r.Value.taskid].taskname : "İsimsiz Task";
                res.creationdateyear = r.Value.creationdate.Value.Year;
                res.creationdatemonth = r.Value.creationdate.Value.Month;
                res.creationdateday = r.Value.creationdate.Value.Day;
                res.customerid = r.Value.attachedobjectid.Value;
                if (WebApiConfig.AdslCustomers.ContainsKey(r.Value.attachedobjectid.Value))
                {
                    var cstmr = WebApiConfig.AdslCustomers[r.Value.attachedobjectid.Value];
                    res.customername = cstmr.customername;
                    res.customeradres = cstmr.description;
                    if (cstmr.ilKimlikNo.HasValue && WebApiConfig.AdslIls.ContainsKey(cstmr.ilKimlikNo.Value))
                        res.il = WebApiConfig.AdslIls[cstmr.ilKimlikNo.Value].ad;
                    if (cstmr.ilceKimlikNo.HasValue && WebApiConfig.AdslIlces.ContainsKey(cstmr.ilceKimlikNo.Value))
                        res.ilce = WebApiConfig.AdslIlces[cstmr.ilceKimlikNo.Value].ad;
                }
                else
                    res.customername = "İsimsiz Müşteri";
                if (r.Value.attachedpersonelid != null)
                {
                    var personel = (int?)r.Value.attachedpersonelid.Value;
                    res.personelid = personel;
                    res.personelname = WebApiConfig.AdslPersonels.ContainsKey(personel.Value) ? WebApiConfig.AdslPersonels[personel.Value].personelname : "İsimsiz Personel";
                }
                if (r.Value.attachmentdate != null)
                {
                    var adate = r.Value.attachmentdate.Value;
                    res.attachmentdateyear = adate.Year;
                    res.attachmentdatemonth = adate.Month;
                    res.attachmentdateday = adate.Day;
                }
                res.status = r.Value.status != null ? WebApiConfig.AdslStatus.ContainsKey(r.Value.status.Value) ? WebApiConfig.AdslStatus[r.Value.status.Value].taskstate : null : null;
                res.description = r.Value.description;
                res.fault = r.Value.fault;
                return res;
            }).ToList();
        }

        // Bütün kapatılan tasklar 13.10.2016 Hüseyin KOZ
        public static async Task<List<SKClosedTasksReport>> getSKClosedTasksReport()
        {
            string[] lastStateType = new string[] { "Bekleyen", "Tamamlanan", "İptal Edilen", "Ertelenen" };
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslTaskQueues.Where(r => r.Value.status != null && r.Value.status.Value != 9159 && r.Value.status.Value != 9165 && r.Value.deleted == false).Select(r =>
            {
                var res = new SKClosedTasksReport();
                res.taskorderno = r.Value.taskorderno;
                res.taskid = r.Value.taskid;
                res.taskname = WebApiConfig.AdslTasks.ContainsKey(r.Value.taskid) ? WebApiConfig.AdslTasks[r.Value.taskid].taskname : "İsimsiz Task";
                res.creationdateyear = r.Value.creationdate.Value.Year;
                res.creationdatemonth = r.Value.creationdate.Value.Month;
                res.creationdateday = r.Value.creationdate.Value.Day;
                res.customerid = r.Value.attachedobjectid.Value;
                if (WebApiConfig.AdslCustomers.ContainsKey(r.Value.attachedobjectid.Value))
                {
                    var cstmr = WebApiConfig.AdslCustomers[r.Value.attachedobjectid.Value];
                    res.customername = cstmr.customername;
                    res.customeradres = cstmr.description;
                    res.solcustomerid = cstmr.superonlineCustNo;
                    if (cstmr.ilKimlikNo.HasValue && WebApiConfig.AdslIls.ContainsKey(cstmr.ilKimlikNo.Value))
                        res.il = WebApiConfig.AdslIls[cstmr.ilKimlikNo.Value].ad;
                    if (cstmr.ilceKimlikNo.HasValue && WebApiConfig.AdslIlces.ContainsKey(cstmr.ilceKimlikNo.Value))
                        res.ilce = WebApiConfig.AdslIlces[cstmr.ilceKimlikNo.Value].ad;
                }
                else
                    res.customername = "İsimsiz Müşteri";
                if (r.Value.attachedpersonelid != null)
                {
                    var personel = (int?)r.Value.attachedpersonelid.Value;
                    res.personelid = personel;
                    res.personelname = WebApiConfig.AdslPersonels.ContainsKey(personel.Value) ? WebApiConfig.AdslPersonels[personel.Value].personelname : "İsimsiz Personel";
                }
                if (r.Value.attachmentdate != null)
                {
                    var adate = r.Value.attachmentdate.Value;
                    res.attachmentdateyear = adate.Year;
                    res.attachmentdatemonth = adate.Month;
                    res.attachmentdateday = adate.Day;
                }
                if (r.Value.consummationdate != null)
                {
                    var adate = r.Value.consummationdate.Value;
                    res.consummationdateyear = adate.Year;
                    res.consummationdatemonth = adate.Month;
                    res.consummationdateday = adate.Day;
                }
                if (WebApiConfig.AdslStatus.ContainsKey(r.Value.status.Value)) {
                    var status = WebApiConfig.AdslStatus[r.Value.status.Value];
                    res.status = status.taskstate;
                    res.taskstatus = lastStateType[status.statetype.Value];
                }
                else
                {
                    res.status = "Durum Bulunamadı";
                    res.taskstatus = "Bilinmiyor";
                }
                if (WebApiConfig.AdslProccesses.ContainsKey(r.Value.relatedtaskorderid.Value) && WebApiConfig.AdslProccesses[r.Value.relatedtaskorderid.Value].K_TON.HasValue && WebApiConfig.AdslTaskQueues.ContainsKey(WebApiConfig.AdslProccesses[r.Value.relatedtaskorderid.Value].K_TON.Value))
                {
                    var ktask = WebApiConfig.AdslTaskQueues[WebApiConfig.AdslProccesses[r.Value.relatedtaskorderid.Value].K_TON.Value];
                    res.k_personelid = ktask.attachedpersonelid;
                    res.k_personelname = WebApiConfig.AdslPersonels.ContainsKey(ktask.attachedpersonelid ?? 0) ? WebApiConfig.AdslPersonels[ktask.attachedpersonelid.Value].personelname : null;
                }
                res.description = r.Value.description;
                res.fault = r.Value.fault;
                return res;
            }).ToList();
        }

        // bekleyen tasklar kurulum ve ktk kapanma tarihleri ile birlikte
        public static async Task<List<SKStandbyTasksHours>> getSKStandbyTasksHours ()
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslTaskQueues.Where(r => (r.Value.status == null || r.Value.status.Value == 9159 || r.Value.status.Value == 9165) && r.Value.deleted == false).Select(r =>
            {
                var res = new SKStandbyTasksHours();
                res.taskorderno = r.Value.taskorderno;
                res.taskid = r.Value.taskid;
                res.taskname = WebApiConfig.AdslTasks.ContainsKey(r.Value.taskid) ? WebApiConfig.AdslTasks[r.Value.taskid].taskname : "İsimsiz Task";
                res.creationdateyear = r.Value.creationdate.Value.Year;
                res.creationdatemonth = r.Value.creationdate.Value.Month;
                res.creationdateday = r.Value.creationdate.Value.Day;
                res.customerid = r.Value.attachedobjectid.Value;
                if (WebApiConfig.AdslCustomers.ContainsKey(r.Value.attachedobjectid.Value))
                {
                    var cstmr = WebApiConfig.AdslCustomers[r.Value.attachedobjectid.Value];
                    res.customername = cstmr.customername;
                    res.customeradres = cstmr.description;
                    if (cstmr.ilKimlikNo.HasValue && WebApiConfig.AdslIls.ContainsKey(cstmr.ilKimlikNo.Value))
                        res.il = WebApiConfig.AdslIls[cstmr.ilKimlikNo.Value].ad;
                    if (cstmr.ilceKimlikNo.HasValue && WebApiConfig.AdslIlces.ContainsKey(cstmr.ilceKimlikNo.Value))
                        res.ilce = WebApiConfig.AdslIlces[cstmr.ilceKimlikNo.Value].ad;
                }
                else
                    res.customername = "İsimsiz Müşteri";
                if (r.Value.attachedpersonelid != null)
                {
                    var personel = (int?)r.Value.attachedpersonelid.Value;
                    res.personelid = personel;
                    res.personelname = WebApiConfig.AdslPersonels.ContainsKey(personel.Value) ? WebApiConfig.AdslPersonels[personel.Value].personelname : "İsimsiz Personel";
                }
                if (r.Value.attachmentdate != null)
                {
                    var adate = r.Value.attachmentdate.Value;
                    res.attachmentdateyear = adate.Year;
                    res.attachmentdatemonth = adate.Month;
                    res.attachmentdateday = adate.Day;
                }
                res.status = r.Value.status != null ? WebApiConfig.AdslStatus.ContainsKey(r.Value.status.Value) ? WebApiConfig.AdslStatus[r.Value.status.Value].taskstate : null : null;
                res.description = r.Value.description;
                res.fault = r.Value.fault;
                var k = WebApiConfig.AdslProccesses.Where(pt => pt.Key == WebApiConfig.AdslTaskQueues[r.Value.taskorderno].relatedtaskorderid).FirstOrDefault();
                if (k.Value != null && k.Value.K_TON.HasValue && WebApiConfig.AdslTaskQueues.ContainsKey(k.Value.K_TON.Value) && WebApiConfig.AdslTaskQueues[k.Value.K_TON.Value].consummationdate.HasValue)
                {
                    var ktq = WebApiConfig.AdslTaskQueues[k.Value.K_TON.Value];
                    res.k_personel = ktq.attachedpersonelid.HasValue ? WebApiConfig.AdslPersonels.ContainsKey(ktq.attachedpersonelid.Value) ? WebApiConfig.AdslPersonels[ktq.attachedpersonelid.Value].personelname : "İsimsiz Personel" : null;
                    var kdate = ktq.consummationdate;
                    res.kmonth = kdate.Value.Month;
                    res.kday = kdate.Value.Day;
                    res.ktime = kdate.Value.ToString("HH:mm");
                    res.kyear = kdate.Value.Year;
                }
                if (k.Value != null && k.Value.Ktk_TON.HasValue && WebApiConfig.AdslTaskQueues.ContainsKey(k.Value.Ktk_TON.Value) && WebApiConfig.AdslTaskQueues[k.Value.Ktk_TON.Value].consummationdate.HasValue)
                {
                    var ktkdate = WebApiConfig.AdslTaskQueues[k.Value.Ktk_TON.Value].consummationdate;
                    res.ktkyear = ktkdate.Value.Year;
                    res.ktkmonth = ktkdate.Value.Month;
                    res.ktkday = ktkdate.Value.Day;
                    res.ktktime = ktkdate.Value.ToString("HH:mm");
                }
                return res;
            }).ToList();
        }

        // Satış kurulum raporu
        public static async Task<List<SKReport>> getSKReport(DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
            return WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                var last_tq = WebApiConfig.AdslTaskQueues[r.Last_TON];
                var ktk_tq = r.Ktk_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : last_tq;
                return ktk_tq.status == null || ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).Select(r =>
            {
                var s_tq = WebApiConfig.AdslTaskQueues[r.S_TON];
                var kr_tq = (r.Kr_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Kr_TON.Value] : null;
                var k_tq = (r.K_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.K_TON.Value] : null;
                var ktk_tq = (r.Ktk_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                var lasttq = WebApiConfig.AdslTaskQueues[r.Last_TON];
                var res = new SKReport();
                //Satış kurulum view sonucuna göre class tanımı yap ve o class türünde bir nesne oluşturup döndür...
                if (s_tq.attachedobjectid != null && WebApiConfig.AdslCustomers.ContainsKey(s_tq.attachedobjectid.Value))
                {
                    var customerInfo = WebApiConfig.AdslCustomers[s_tq.attachedobjectid.Value];
                    res.custid = customerInfo.customerid;
                    res.custname = customerInfo.customername;
                    res.custphone = customerInfo.phone;
                    res.customeradres = customerInfo.description;
                    if (customerInfo.ilKimlikNo != null && WebApiConfig.AdslIls.ContainsKey(customerInfo.ilKimlikNo.Value))
                        res.il = WebApiConfig.AdslIls[customerInfo.ilKimlikNo.Value].ad;
                    if (customerInfo.ilceKimlikNo != null && WebApiConfig.AdslIlces.ContainsKey(customerInfo.ilceKimlikNo.Value))
                        res.ilce = WebApiConfig.AdslIlces[customerInfo.ilceKimlikNo.Value].ad;
                    res.gsm = customerInfo.gsm;
                    res.superonlinecustno = customerInfo.superonlineCustNo;
                }
                if (s_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(s_tq.attachedpersonelid.Value))
                {
                    var sPersonelInfo = WebApiConfig.AdslPersonels[s_tq.attachedpersonelid.Value];
                    res.s_perid = sPersonelInfo.personelid;
                    res.s_pername = sPersonelInfo.personelname;
                    if (sPersonelInfo.relatedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(sPersonelInfo.relatedpersonelid.Value))
                        res.s_perky = WebApiConfig.AdslPersonels[sPersonelInfo.relatedpersonelid.Value].personelname;
                }
                res.s_ton = s_tq.taskorderno;
                if (WebApiConfig.AdslTasks.ContainsKey(s_tq.taskid))
                    res.s_tqname = WebApiConfig.AdslTasks[s_tq.taskid].taskname;
                res.campaign = WebApiConfig.AdslCustomerProducts.ContainsKey(r.S_TON) ? WebApiConfig.AdslCampaigns.ContainsKey((int)WebApiConfig.AdslCustomerProducts[r.S_TON].campaignid) ? 
                        WebApiConfig.AdslCampaigns[(int)WebApiConfig.AdslCustomerProducts[r.S_TON].campaignid].name : null : null;
                res.kaynak = s_tq.fault;
                res.s_desc = s_tq.description;
                res.s_createyear = s_tq.creationdate.Value.Year;
                res.s_createmonth = s_tq.creationdate.Value.Month;
                res.s_createday = s_tq.creationdate.Value.Day;
                if (s_tq.appointmentdate != null)
                {
                    res.s_netflowyear = s_tq.appointmentdate.Value.Year;
                    res.s_netflowmonth = s_tq.appointmentdate.Value.Month;
                    res.s_netflowday = s_tq.appointmentdate.Value.Day;
                }
                if (s_tq.consummationdate != null)
                {
                    res.s_consummationdate = s_tq.consummationdate.Value;
                    res.s_consummationdateyear = s_tq.consummationdate.Value.Year;
                    res.s_consummationdatemonth = s_tq.consummationdate.Value.Month;
                    res.s_consummationdateday = s_tq.consummationdate.Value.Day;
                }
                if (s_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(s_tq.status.Value))
                {
                    var statu = WebApiConfig.AdslStatus[s_tq.status.Value];
                    res.s_tqstate = statu.taskstate;
                    res.s_tqstatetype = StateTypeText[statu.statetype.Value];
                }
                else
                    res.s_tqstatetype = "Bekleyen";
                if (kr_tq != null)
                {
                    res.kr_ton = kr_tq.taskorderno;
                    res.kr_creationdateyear = kr_tq.creationdate.Value.Year;
                    res.kr_creationdatemonth = kr_tq.creationdate.Value.Month;
                    res.kr_creationdateday = kr_tq.creationdate.Value.Day;
                    if (WebApiConfig.AdslTasks.ContainsKey(kr_tq.taskid))
                        res.kr_tqname = WebApiConfig.AdslTasks[kr_tq.taskid].taskname;
                    if (kr_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(kr_tq.attachedpersonelid.Value))
                    {
                        var krPersonelInfo = WebApiConfig.AdslPersonels[kr_tq.attachedpersonelid.Value];
                        res.kr_perid = krPersonelInfo.personelid;
                        res.kr_pername = krPersonelInfo.personelname;
                        //res.kr_perky = null;
                    }
                    var kr_previous = kr_tq.previoustaskorderid != null ? (WebApiConfig.AdslTaskQueues.ContainsKey(kr_tq.previoustaskorderid.Value) ? WebApiConfig.AdslTaskQueues[kr_tq.previoustaskorderid.Value] : null) : null;
                    if (kr_previous != null && kr_previous.appointmentdate != null)
                    {
                        res.kr_netflowdateyear = kr_previous.appointmentdate.Value.Year;
                        res.kr_netflowdatemonth = kr_previous.appointmentdate.Value.Month;
                        res.kr_netflowdateday = kr_previous.appointmentdate.Value.Day;
                    }
                    if (kr_tq.consummationdate != null)
                    {
                        res.kr_consummationdate = kr_tq.consummationdate;
                        res.kr_consummationdateyear = kr_tq.consummationdate.Value.Year;
                        res.kr_consummationdatemonth = kr_tq.consummationdate.Value.Month;
                        res.kr_consummationdateday = kr_tq.consummationdate.Value.Day;
                    }
                    if (kr_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(kr_tq.status.Value))
                    {
                        var statu = WebApiConfig.AdslStatus[kr_tq.status.Value];
                        res.kr_tqstate = statu.taskstate;
                        res.kr_tqstatetype = StateTypeText[statu.statetype.Value];
                    }
                    else
                        res.kr_tqstatetype = "Bekleyen";
                    res.kr_desc = kr_tq.description;
                }
                if (k_tq != null)
                {
                    res.k_ton = k_tq.taskorderno;
                    if (WebApiConfig.AdslTasks.ContainsKey(k_tq.taskid))
                        res.k_tqname = WebApiConfig.AdslTasks[k_tq.taskid].taskname;
                    if (k_tq.consummationdate != null)
                    {
                        res.k_consummationdate = k_tq.consummationdate;
                        res.k_consummationdateyear = k_tq.consummationdate.Value.Year;
                        res.k_consummationdatemonth = k_tq.consummationdate.Value.Month;
                        res.k_consummationdateday = k_tq.consummationdate.Value.Day;
                    }
                    if (k_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(k_tq.attachedpersonelid.Value))
                    {
                        var kPersonelInfo = WebApiConfig.AdslPersonels[k_tq.attachedpersonelid.Value];
                        res.k_perid = kPersonelInfo.personelid;
                        res.k_pername = kPersonelInfo.personelname;
                        if (kPersonelInfo.relatedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(kPersonelInfo.relatedpersonelid.Value))
                            res.k_perky = WebApiConfig.AdslPersonels[kPersonelInfo.relatedpersonelid.Value].personelname;
                    }
                    if (k_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(k_tq.status.Value))
                    {
                        var statu = WebApiConfig.AdslStatus[k_tq.status.Value];
                        res.k_tqstate = statu.taskstate;
                        res.k_tqstatetype = StateTypeText[statu.statetype.Value];
                    }
                    else
                        res.k_tqstatetype = "Bekleyen";
                    res.k_desc = k_tq.description;
                }
                if (ktk_tq != null)
                {
                    res.ktk_ton = ktk_tq.taskorderno;
                    if (WebApiConfig.AdslTasks.ContainsKey(ktk_tq.taskid))
                        res.ktk_tqname = WebApiConfig.AdslTasks[ktk_tq.taskid].taskname;
                    if (ktk_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(ktk_tq.attachedpersonelid.Value))
                    {
                        var ktkPersonelInfo = WebApiConfig.AdslPersonels[ktk_tq.attachedpersonelid.Value];
                        res.ktk_perid = ktkPersonelInfo.personelid;
                        res.ktk_pername = ktkPersonelInfo.personelname;
                    }
                    if (ktk_tq.consummationdate != null)
                    {
                        res.ktk_consummationdate = ktk_tq.consummationdate;
                        res.ktk_consummationdateyear = ktk_tq.consummationdate.Value.Year;
                        res.ktk_consummationdatemonth = ktk_tq.consummationdate.Value.Month;
                        res.ktk_consummationdateday = ktk_tq.consummationdate.Value.Day;
                    }
                    if (ktk_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(ktk_tq.status.Value))
                    {
                        var statu = WebApiConfig.AdslStatus[ktk_tq.status.Value];
                        res.ktk_tqstate = statu.taskstate;
                        res.ktk_tqstatetype = StateTypeText[statu.statetype.Value];
                    }
                    else
                        res.ktk_tqstatetype = "Bekleyen";
                    res.ktk_desc = ktk_tq.description;
                }
                if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value))
                {
                    var statu = WebApiConfig.AdslStatus[lasttq.status.Value];
                    res.lastTaskStatus = StateTypeText[statu.statetype.Value];
                    res.lastTaskStatusName = statu.taskstate;
                }
                else
                    res.lastTaskStatus = "Bekleyen";
                var lastTask = WebApiConfig.AdslTasks[lasttq.taskid];
                res.lastTaskTypeName = WebApiConfig.AdslTaskTypes[lastTask.tasktype].TaskTypeName;
                res.lastTaskName = lastTask.taskname;
                res.lastTaskDescription = lasttq.description;
                res.lasttaskcreationdateyear = lasttq.creationdate.Value.Year;
                res.lasttaskcreationdatemonth = lasttq.creationdate.Value.Month;
                res.lasttaskcreationdateday = lasttq.creationdate.Value.Day;
                if (lasttq.consummationdate != null)
                {
                    res.lasttaskconsummationdateyear = lasttq.consummationdate.Value.Year;
                    res.lasttaskconsummationdatemonth = lasttq.consummationdate.Value.Month;
                    res.lasttaskconsummationdateday = lasttq.consummationdate.Value.Day;
                }
                return res;
            }).ToList();
        }

        // Çağrı Satış kurulum raporu
        public static async Task<List<SKReport>> getCagriSKReport(DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            Dictionary<int, int> tasks = new Dictionary<int, int> { { 133, 133 }, { 131, 131 }, { 122, 122 }, { 121, 121 }, { 120, 120 }, { 119, 119 }, { 139, 139 } }; // çağrı satış taskları
            var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
            return WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                var last_tq = WebApiConfig.AdslTaskQueues[r.Last_TON];
                var s_tq = WebApiConfig.AdslTaskQueues[r.S_TON];
                int? cs_tq = tasks.Where(z => z.Key == s_tq.taskid).Select(x => x.Value).FirstOrDefault();
                var ktk_tq = r.Ktk_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : last_tq;
                return cs_tq != null && cs_tq != 0 && (ktk_tq.status == null || ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end);
            }).Select(r =>
            {
                var s_tq = WebApiConfig.AdslTaskQueues[r.S_TON];
                var kr_tq = (r.Kr_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Kr_TON.Value] : null;
                var k_tq = (r.K_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.K_TON.Value] : null;
                var ktk_tq = (r.Ktk_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                var lasttq = WebApiConfig.AdslTaskQueues[r.Last_TON];
                var res = new SKReport();
                //Satış kurulum view sonucuna göre class tanımı yap ve o class türünde bir nesne oluşturup döndür...
                if (s_tq.attachedobjectid != null && WebApiConfig.AdslCustomers.ContainsKey(s_tq.attachedobjectid.Value))
                {
                    var customerInfo = WebApiConfig.AdslCustomers[s_tq.attachedobjectid.Value];
                    res.custid = customerInfo.customerid;
                    res.custname = customerInfo.customername;
                    res.custphone = customerInfo.phone;
                    res.customeradres = customerInfo.description;
                    if (customerInfo.ilKimlikNo != null && WebApiConfig.AdslIls.ContainsKey(customerInfo.ilKimlikNo.Value))
                        res.il = WebApiConfig.AdslIls[customerInfo.ilKimlikNo.Value].ad;
                    if (customerInfo.ilceKimlikNo != null && WebApiConfig.AdslIlces.ContainsKey(customerInfo.ilceKimlikNo.Value))
                        res.ilce = WebApiConfig.AdslIlces[customerInfo.ilceKimlikNo.Value].ad;
                    res.gsm = customerInfo.gsm;
                    res.superonlinecustno = customerInfo.superonlineCustNo;
                }
                if (s_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(s_tq.attachedpersonelid.Value))
                {
                    var sPersonelInfo = WebApiConfig.AdslPersonels[s_tq.attachedpersonelid.Value];
                    res.s_perid = sPersonelInfo.personelid;
                    res.s_pername = sPersonelInfo.personelname;
                    if (sPersonelInfo.relatedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(sPersonelInfo.relatedpersonelid.Value))
                        res.s_perky = WebApiConfig.AdslPersonels[sPersonelInfo.relatedpersonelid.Value].personelname;
                }
                res.s_ton = s_tq.taskorderno;
                if (WebApiConfig.AdslTasks.ContainsKey(s_tq.taskid))
                    res.s_tqname = WebApiConfig.AdslTasks[s_tq.taskid].taskname;
                res.campaign = WebApiConfig.AdslCustomerProducts.ContainsKey(r.S_TON) ? WebApiConfig.AdslCampaigns.ContainsKey((int)WebApiConfig.AdslCustomerProducts[r.S_TON].campaignid) ?
                        WebApiConfig.AdslCampaigns[(int)WebApiConfig.AdslCustomerProducts[r.S_TON].campaignid].name : null : null; // campaign ve customerproduct dictionary olması gerek
                res.kaynak = s_tq.fault;
                res.s_desc = s_tq.description;
                res.s_createyear = s_tq.creationdate.Value.Year;
                res.s_createmonth = s_tq.creationdate.Value.Month;
                res.s_createday = s_tq.creationdate.Value.Day;
                if (s_tq.appointmentdate != null)
                {
                    res.s_netflowyear = s_tq.appointmentdate.Value.Year;
                    res.s_netflowmonth = s_tq.appointmentdate.Value.Month;
                    res.s_netflowday = s_tq.appointmentdate.Value.Day;
                }
                if (s_tq.consummationdate != null)
                {
                    res.s_consummationdate = s_tq.consummationdate.Value;
                    res.s_consummationdateyear = s_tq.consummationdate.Value.Year;
                    res.s_consummationdatemonth = s_tq.consummationdate.Value.Month;
                    res.s_consummationdateday = s_tq.consummationdate.Value.Day;
                }
                if (s_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(s_tq.status.Value))
                {
                    var statu = WebApiConfig.AdslStatus[s_tq.status.Value];
                    res.s_tqstate = statu.taskstate;
                    res.s_tqstatetype = StateTypeText[statu.statetype.Value];
                }
                else
                    res.s_tqstatetype = "Bekleyen";
                if (kr_tq != null)
                {
                    res.kr_ton = kr_tq.taskorderno;
                    res.kr_creationdateyear = kr_tq.creationdate.Value.Year;
                    res.kr_creationdatemonth = kr_tq.creationdate.Value.Month;
                    res.kr_creationdateday = kr_tq.creationdate.Value.Day;
                    if (WebApiConfig.AdslTasks.ContainsKey(kr_tq.taskid))
                        res.kr_tqname = WebApiConfig.AdslTasks[kr_tq.taskid].taskname;
                    if (kr_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(kr_tq.attachedpersonelid.Value))
                    {
                        var krPersonelInfo = WebApiConfig.AdslPersonels[kr_tq.attachedpersonelid.Value];
                        res.kr_perid = krPersonelInfo.personelid;
                        res.kr_pername = krPersonelInfo.personelname;
                        //res.kr_perky = null;
                    }
                    var kr_previous = kr_tq.previoustaskorderid != null ? (WebApiConfig.AdslTaskQueues.ContainsKey(kr_tq.previoustaskorderid.Value) ? WebApiConfig.AdslTaskQueues[kr_tq.previoustaskorderid.Value] : null) : null;
                    if (kr_previous != null && kr_previous.appointmentdate != null)
                    {
                        res.kr_netflowdateyear = kr_previous.appointmentdate.Value.Year;
                        res.kr_netflowdatemonth = kr_previous.appointmentdate.Value.Month;
                        res.kr_netflowdateday = kr_previous.appointmentdate.Value.Day;
                    }
                    if (kr_tq.consummationdate != null)
                    {
                        res.kr_consummationdate = kr_tq.consummationdate;
                        res.kr_consummationdateyear = kr_tq.consummationdate.Value.Year;
                        res.kr_consummationdatemonth = kr_tq.consummationdate.Value.Month;
                        res.kr_consummationdateday = kr_tq.consummationdate.Value.Day;
                    }
                    if (kr_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(kr_tq.status.Value))
                    {
                        var statu = WebApiConfig.AdslStatus[kr_tq.status.Value];
                        res.kr_tqstate = statu.taskstate;
                        res.kr_tqstatetype = StateTypeText[statu.statetype.Value];
                    }
                    else
                        res.kr_tqstatetype = "Bekleyen";
                    res.kr_desc = kr_tq.description;
                }
                if (k_tq != null)
                {
                    res.k_ton = k_tq.taskorderno;
                    if (WebApiConfig.AdslTasks.ContainsKey(k_tq.taskid))
                        res.k_tqname = WebApiConfig.AdslTasks[k_tq.taskid].taskname;
                    if (k_tq.consummationdate != null)
                    {
                        res.k_consummationdate = k_tq.consummationdate;
                        res.k_consummationdateyear = k_tq.consummationdate.Value.Year;
                        res.k_consummationdatemonth = k_tq.consummationdate.Value.Month;
                        res.k_consummationdateday = k_tq.consummationdate.Value.Day;
                    }
                    if (k_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(k_tq.attachedpersonelid.Value))
                    {
                        var kPersonelInfo = WebApiConfig.AdslPersonels[k_tq.attachedpersonelid.Value];
                        res.k_perid = kPersonelInfo.personelid;
                        res.k_pername = kPersonelInfo.personelname;
                        //res.k_perky = null;
                    }
                    if (k_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(k_tq.status.Value))
                    {
                        var statu = WebApiConfig.AdslStatus[k_tq.status.Value];
                        res.k_tqstate = statu.taskstate;
                        res.k_tqstatetype = StateTypeText[statu.statetype.Value];
                    }
                    else
                        res.k_tqstatetype = "Bekleyen";
                    res.k_desc = k_tq.description;
                }
                if (ktk_tq != null)
                {
                    res.ktk_ton = ktk_tq.taskorderno;
                    if (WebApiConfig.AdslTasks.ContainsKey(ktk_tq.taskid))
                        res.ktk_tqname = WebApiConfig.AdslTasks[ktk_tq.taskid].taskname;
                    if (ktk_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(ktk_tq.attachedpersonelid.Value))
                    {
                        var ktkPersonelInfo = WebApiConfig.AdslPersonels[ktk_tq.attachedpersonelid.Value];
                        res.ktk_perid = ktkPersonelInfo.personelid;
                        res.ktk_pername = ktkPersonelInfo.personelname;
                    }
                    if (ktk_tq.consummationdate != null)
                    {
                        res.ktk_consummationdate = ktk_tq.consummationdate;
                        res.ktk_consummationdateyear = ktk_tq.consummationdate.Value.Year;
                        res.ktk_consummationdatemonth = ktk_tq.consummationdate.Value.Month;
                        res.ktk_consummationdateday = ktk_tq.consummationdate.Value.Day;
                    }
                    if (ktk_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(ktk_tq.status.Value))
                    {
                        var statu = WebApiConfig.AdslStatus[ktk_tq.status.Value];
                        res.ktk_tqstate = statu.taskstate;
                        res.ktk_tqstatetype = StateTypeText[statu.statetype.Value];
                    }
                    else
                        res.ktk_tqstatetype = "Bekleyen";
                    res.ktk_desc = ktk_tq.description;
                }
                if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value))
                {
                    var statu = WebApiConfig.AdslStatus[lasttq.status.Value];
                    res.lastTaskStatus = StateTypeText[statu.statetype.Value];
                    res.lastTaskStatusName = statu.taskstate;
                }
                else
                    res.lastTaskStatus = "Bekleyen";
                var lastTask = WebApiConfig.AdslTasks[lasttq.taskid];
                res.lastTaskTypeName = WebApiConfig.AdslTaskTypes[lastTask.tasktype].TaskTypeName;
                res.lastTaskName = lastTask.taskname;
                res.lastTaskDescription = lasttq.description;
                res.lasttaskcreationdateyear = lasttq.creationdate.Value.Year;
                res.lasttaskcreationdatemonth = lasttq.creationdate.Value.Month;
                res.lasttaskcreationdateday = lasttq.creationdate.Value.Day;
                if (lasttq.consummationdate != null)
                {
                    res.lasttaskconsummationdateyear = lasttq.consummationdate.Value.Year;
                    res.lasttaskconsummationdatemonth = lasttq.consummationdate.Value.Month;
                    res.lasttaskconsummationdateday = lasttq.consummationdate.Value.Day;
                }
                return res;
            }).ToList();
        }

        // Bayi SL Report
        public static async Task<List<SLBayiReport>> getBayiSLReport(int BayiId, DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            var maxSL = 24; // 24 saat üzerinden değerlendir (çarpan için)
            return WebApiConfig.AdslProccesses.Values.SelectMany(
                p => p.SLs.Where(sl =>
                {
                    var lasttq = WebApiConfig.AdslTaskQueues[p.Last_TON];
                    var date = DateTime.Now; // geçmiş aylarda bekleyen işlem gözükmemesi için oluşturuldu
                    return sl.Value.BayiID.HasValue && sl.Value.BayiID == BayiId && ((!sl.Value.BEnd.HasValue && ((lasttq.consummationdate == null && request.start <= date && request.end >= date) || (lasttq.consummationdate != null && lasttq.consummationdate >= request.start && lasttq.consummationdate <= request.end))) || (sl.Value.BEnd.HasValue && sl.Value.BEnd.Value >= request.start && sl.Value.BEnd.Value <= request.end));
                }).Select(bsl =>
                {
                    var r = new SLBayiReport();
                    var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
                    var lasttq = WebApiConfig.AdslTaskQueues[p.Last_TON];
                    if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value))
                    {
                        var statu = WebApiConfig.AdslStatus[lasttq.status.Value];
                        r.status = StateTypeText[statu.statetype.Value]; // proccess durumunu gösterir
                    }
                    else
                        r.status = "Bekleyen";
                    if (bsl.Value.BEnd.HasValue) // slstatus; anlık sl durumu. bayi sl end yoksa ya iptaldir yada bekleyen. bayi sl dosyasında sllerini hesaplayabilsin diye oluşturuldu
                        r.slstatus = "Tamamlanan";
                    else if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value) && WebApiConfig.AdslStatus[lasttq.status.Value].statetype.Value == 2)
                        r.slstatus = "İptal Edilen";
                    else if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value) && WebApiConfig.AdslStatus[lasttq.status.Value].statetype.Value == 3)
                        r.slstatus = "Ertelenen";
                    else
                        r.slstatus = "Bekleyen";
                    r.BayiSLTaskStart = bsl.Value.BStart;
                    r.BayiSLEnd = bsl.Value.BEnd;
                    if (WebApiConfig.AdslSl.ContainsKey(bsl.Key)) // bayi max time eklendi fazla sart olmasın diye tek ifte toplandı
                    {
                        var bayiSl = WebApiConfig.AdslSl[bsl.Key];
                        r.SLName = bayiSl.SLName;
                        r.BayiSLMaxTime = bayiSl.BayiMaxTime != null ? bayiSl.BayiMaxTime.Value : 0;
                        r.BayiSLEtkisi = r.BayiSLStart == null ? null : (double?)Math.Round((((r.BayiSLEnd - r.BayiSLStart).Value.TotalHours)*(bayiSl.BayiMaxTime != null ? (maxSL / bayiSl.BayiMaxTime.Value) : 1)),2);
                    }
                    else
                        r.SLName = "Tanımlanmamış SL";
                    if (bsl.Value.BayiID.HasValue && WebApiConfig.AdslPersonels.ContainsKey(bsl.Value.BayiID.Value)) 
                    { // Bayi bilgileri yazılmıyordu boş geliyordu o yüzden ekledim gerek yoksa bilgiler slkocreport classına alınabilir ?
                        var Bayi = WebApiConfig.AdslPersonels[bsl.Value.BayiID.Value];
                        r.BayiId = Bayi.personelid;
                        r.BayiName = Bayi.personelname;
                        r.Il = WebApiConfig.AdslIls.ContainsKey(Bayi.ilKimlikNo ?? 0) ? WebApiConfig.AdslIls[Bayi.ilKimlikNo.Value].ad : null;
                        r.Ilce = WebApiConfig.AdslIlces.ContainsKey(Bayi.ilceKimlikNo ?? 0) ? WebApiConfig.AdslIlces[Bayi.ilceKimlikNo.Value].ad : null;
                    }
                    r.CustomerId = bsl.Value.CustomerId;
                    if (WebApiConfig.AdslCustomers.ContainsKey(bsl.Value.CustomerId))
                    {
                        var cust = WebApiConfig.AdslCustomers[bsl.Value.CustomerId];
                        r.CustomerName = cust.customername;
                        r.superonlineNo = cust.superonlineCustNo;
                    }
                    else
                        r.CustomerName = "Tanımlanmamış Müşteri";
                    return r;
                })
            ).ToList();
        }

        // KOCSL Report
        public static async Task<List<SLKocReport>> getKocSLReport(DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            var maxSL = 24.0; // (çarpan için)
            var maxKSL = 48.0; // (çarpan için)
            return WebApiConfig.AdslProccesses.Values.SelectMany(
                p => p.SLs.Where(sl => {
                    var lasttq = WebApiConfig.AdslTaskQueues[p.Last_TON];
                    var date = DateTime.Now; // geçmiş aylarda bekleyen işlem gözükmemesi için oluşturuldu
                    return (!sl.Value.KEnd.HasValue && ((lasttq.consummationdate == null && request.start <= date && request.end >= date) || (lasttq.consummationdate != null && lasttq.consummationdate >= request.start && lasttq.consummationdate <= request.end))) || (sl.Value.KEnd.HasValue && sl.Value.KEnd.Value >= request.start && sl.Value.KEnd.Value <= request.end) || (sl.Value.BEnd.HasValue && sl.Value.BEnd.Value >= request.start && sl.Value.BEnd.Value <= request.end);
                }).Select(ksl =>
                {
                    try
                    {
                        var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
                        var lasttq = WebApiConfig.AdslTaskQueues[p.Last_TON];
                        var stq = WebApiConfig.AdslTaskQueues[p.S_TON];
                        var r = new SLKocReport();
                        r.staskname = WebApiConfig.AdslTasks.ContainsKey(stq.taskid) ? WebApiConfig.AdslTasks[stq.taskid].taskname : "İsimsiz Task";
                        if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value))
                        {
                            var statu = WebApiConfig.AdslStatus[lasttq.status.Value];
                            r.status = StateTypeText[statu.statetype.Value];
                        }
                        else
                            r.status = "Bekleyen";
                        if (ksl.Value.BEnd.HasValue) // slstatus anlık sl durumu bayi sl end yoksa ya iptaldir yada bekleyen ayırt etmek için oluşturuldu
                            r.slstatus = "Tamamlanan";
                        else if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value) && WebApiConfig.AdslStatus[lasttq.status.Value].statetype.Value == 2)
                            r.slstatus = "İptal Edilen";
                        else if (lasttq.status != null && WebApiConfig.AdslStatus.ContainsKey(lasttq.status.Value) && WebApiConfig.AdslStatus[lasttq.status.Value].statetype.Value == 3)
                            r.slstatus = "Ertelenen";
                        else
                            r.slstatus = "Bekleyen";
                        if ((ksl.Value.BEnd.HasValue && ksl.Value.BEnd.Value >= request.start && ksl.Value.BEnd.Value <= request.end) || (!ksl.Value.BEnd.HasValue && ksl.Value.BStart.HasValue && ksl.Value.BStart.Value <= request.end))
                        { // koç sl yasin bey'in isteği üzerine anlık bayi sl işlemine göre çekilecek bundan dolayı aynı sl'in 2 farklı ayda görünmesini engellemek için şart koyuldu
                            r.BayiSLTaskStart = ksl.Value.BStart;
                            r.BayiSLEnd = ksl.Value.BEnd;
                        }
                        if ((ksl.Value.KEnd.HasValue && ksl.Value.KEnd.Value >= request.start && ksl.Value.KEnd.Value <= request.end) || (!ksl.Value.KEnd.HasValue && ksl.Value.KStart.HasValue && ksl.Value.KStart.Value <= request.end))
                        { // koç sl yasin bey'in isteği üzerine anlık bayi sl işlemine göre çekilecek bundan dolayı aynı sl'in 2 farklı ayda görünmesini engellemek için şart koyuldu
                            r.KocSLStart = ksl.Value.KStart;
                            r.KocSLEnd = ksl.Value.KEnd;
                        }
                        if (WebApiConfig.AdslSl.ContainsKey(ksl.Key)) // bayi max time ve koc max time eklendi fazla sart olmasın diye tek ifte toplandı
                        {
                            var kocSl = WebApiConfig.AdslSl[ksl.Key];
                            r.SLName = kocSl.SLName;
                            r.BayiSLMaxTime = kocSl.BayiMaxTime != null ? (int?)kocSl.BayiMaxTime.Value : null;
                            r.KocSLMaxTime = kocSl.KocMaxTime != null ? (int?)kocSl.KocMaxTime.Value : null;
                            r.BayiSLEtkisi = r.BayiSLStart == null ? null : (double?)Math.Round((((r.BayiSLEnd - r.BayiSLStart).Value.TotalHours) * (kocSl.BayiMaxTime != null ? (maxSL / (double)kocSl.BayiMaxTime.Value) : 1)), 2);
                            r.KocSLEtkisi = r.KocSLStart == null ? null : (double?)Math.Round((((r.KocSLEnd - r.KocSLStart).Value.TotalHours) * (kocSl.KocMaxTime != null ? (maxKSL / (double)kocSl.KocMaxTime.Value) : 1)), 2);
                        }
                        else
                            r.SLName = "Tanımlanmamış SL";
                        if (ksl.Value.BayiID.HasValue && WebApiConfig.AdslPersonels.ContainsKey(ksl.Value.BayiID.Value))
                        {
                            var Bayi = WebApiConfig.AdslPersonels[ksl.Value.BayiID.Value];
                            r.BayiId = Bayi.personelid;
                            r.BayiName = Bayi.personelname;
                            r.Il = WebApiConfig.AdslIls.ContainsKey(Bayi.ilKimlikNo ?? 0) ? WebApiConfig.AdslIls[Bayi.ilKimlikNo.Value].ad : null;
                            r.Ilce = WebApiConfig.AdslIlces.ContainsKey(Bayi.ilceKimlikNo ?? 0) ? WebApiConfig.AdslIlces[Bayi.ilceKimlikNo.Value].ad : null;
                        }
                        r.CustomerId = ksl.Value.CustomerId;
                        if (WebApiConfig.AdslCustomers.ContainsKey(ksl.Value.CustomerId))
                        {
                            var cust = WebApiConfig.AdslCustomers[ksl.Value.CustomerId];
                            r.CustomerName = cust.customername;
                            r.superonlineNo = cust.superonlineCustNo;
                        }
                        else
                            r.CustomerName = "Tanımlanmamış Müşteri";
                        return r;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                })
            ).ToList();
        }

        // Personel bilgileri raporu (Anlık personel bilgilerine ulaşabilmek için)
        public static async Task<List<PersonelsReport>> getPersonelsReport()
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslPersonels.Select(r =>
            {
                var res = new PersonelsReport();
                res.personelid = r.Value.personelid;
                res.personelname = r.Value.personelname;
                res.email = r.Value.email;
                res.mobile = r.Value.mobile;
                res.il = r.Value.ilKimlikNo != null ? WebApiConfig.AdslIls.ContainsKey(r.Value.ilKimlikNo.Value) ? WebApiConfig.AdslIls[r.Value.ilKimlikNo.Value].ad : null : null;
                res.ilce = r.Value.ilceKimlikNo != null ? WebApiConfig.AdslIlces.ContainsKey(r.Value.ilceKimlikNo.Value) ? WebApiConfig.AdslIlces[r.Value.ilceKimlikNo.Value].ad : null : null;
                res.kanalyoneticisi = r.Value.relatedpersonelid != null ? WebApiConfig.AdslPersonels.ContainsKey(r.Value.relatedpersonelid.Value) ? WebApiConfig.AdslPersonels[r.Value.relatedpersonelid.Value].personelname : null : null;
                res.kurulumbayisi = r.Value.kurulumpersonelid != null ? WebApiConfig.AdslPersonels.ContainsKey(r.Value.kurulumpersonelid.Value) ? WebApiConfig.AdslPersonels[r.Value.kurulumpersonelid.Value].personelname : null : null;
                res.description = r.Value.notes;
                foreach (var item in WebApiConfig.AdslObjectTypes)
                {
                    if ((item.Value.typeid & r.Value.roles) == item.Value.typeid)
                    {
                        if (res.gorev == null)
                            res.gorev += item.Value.typname;
                        else
                            res.gorev += ", " + item.Value.typname;
                    }
                }
                return res;
            }).ToList();
        }

        // Evrak başarı oranı raporu (kontrol edilecek doğru mu çalışıyor ?)
        public static async Task<List<ISSSuccessRate>> getISSSuccessRateReport()
        {
            /* taskid = 47 => Emptor Sisteme Giriş Churn
             * taskid = 90 => Sam Sdm Sipariş Kapama
             * taskid = 31 => Satış Churn
             * taskid = 63 => Satış Churn Vdsl
             * taskid = 33 => Satış CC Churn
             * taskid = 64 => Satış CC Churn Vdsl
             */
            var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslTaskQueues.Where(r =>
                {
                    int taskid = r.Value.taskid;
                    var previous = (r.Value.previoustaskorderid != null && WebApiConfig.AdslTaskQueues.ContainsKey(r.Value.previoustaskorderid.Value)) ? WebApiConfig.AdslTaskQueues[r.Value.previoustaskorderid.Value] : null;
                    int? statetype = r.Value.status != null ? WebApiConfig.AdslStatus.ContainsKey(r.Value.status.Value) ? (int?)WebApiConfig.AdslStatus[r.Value.status.Value].statetype.Value : null : null;
                    return r.Value.deleted == false && ((taskid == 47 && previous != null && previous.taskid != 113) || 
                                   (taskid == 90 && previous != null && previous.taskid != 114) ||
                                    ((taskid == 31 || taskid == 63 || taskid == 33 || taskid == 64) && (statetype == null || statetype.Value != 1)));
                }).Select(r =>
                {
                    var res = new ISSSuccessRate();
                    res.customerid = r.Value.attachedobjectid.Value;
                    res.customername = WebApiConfig.AdslCustomers.ContainsKey(r.Value.attachedobjectid.Value) ? WebApiConfig.AdslCustomers[r.Value.attachedobjectid.Value].customername : "İsimsiz Müşteri";
                    if (r.Value.taskid == 90)
                    {
                        var stask = r.Value.previoustaskorderid != null ? WebApiConfig.AdslTaskQueues.ContainsKey(r.Value.previoustaskorderid.Value) ? WebApiConfig.AdslTaskQueues[r.Value.previoustaskorderid.Value] : null : null;
                        if (stask != null)
                        {
                            res.staskoerderno = stask.taskorderno;
                            res.stask = WebApiConfig.AdslTasks.ContainsKey(stask.taskid) ? WebApiConfig.AdslTasks[stask.taskid].taskname : "Task Adı Yok";
                            res.screatedateyear = stask.creationdate.Value.Year;
                            res.screatedatemonth = stask.creationdate.Value.Month;
                            res.screatedateday = stask.creationdate.Value.Day;
                            if (stask.status != null && WebApiConfig.AdslStatus.ContainsKey(stask.status.Value))
                            {
                                var state = WebApiConfig.AdslStatus[stask.status.Value];
                                res.staskstatus = state.taskstate;
                                res.staskstatetype = StateTypeText[state.statetype.Value];
                            }
                            if (stask.appointmentdate != null)
                            {
                                res.snetflowdateyear = stask.appointmentdate.Value.Year;
                                res.snetflowdatemonth = stask.appointmentdate.Value.Month;
                                res.snetflowdateday = stask.appointmentdate.Value.Day;
                            }
                            if (stask.consummationdate != null)
                            {
                                res.sconsummationdateday = stask.consummationdate.Value.Day;
                                res.sconsummationdatemonth = stask.consummationdate.Value.Month;
                                res.sconsummationdateyear = stask.consummationdate.Value.Year;
                            }
                        }
                        res.endtaskorderno = r.Value.taskorderno;
                        res.endtask = WebApiConfig.AdslTasks.ContainsKey(r.Value.taskid) ? WebApiConfig.AdslTasks[r.Value.taskid].taskname : "Task Adı Yok";
                        if (r.Value.status != null && WebApiConfig.AdslStatus.ContainsKey(r.Value.status.Value))
                        {
                            var state = WebApiConfig.AdslStatus[r.Value.status.Value];
                            res.endtaskstatus = state.taskstate;
                            res.endtaskstatetype = StateTypeText[state.statetype.Value];
                        }
                        res.endtaskcreatedateday = r.Value.creationdate.Value.Day;
                        res.endtaskcreatedatemonth = r.Value.creationdate.Value.Month;
                        res.endtaskcreatedateyear = r.Value.creationdate.Value.Year;
                        if (r.Value.consummationdate != null)
                        {
                            res.endconsummationdateday = r.Value.consummationdate.Value.Day;
                            res.endconsummationdatemonth = r.Value.consummationdate.Value.Month;
                            res.endconsummationdateyear = r.Value.consummationdate.Value.Year;
                        }
                    }
                    else
                    {
                        res.staskoerderno = r.Value.taskorderno;
                        res.stask = WebApiConfig.AdslTasks.ContainsKey(r.Value.taskid) ? WebApiConfig.AdslTasks[r.Value.taskid].taskname : null;
                        res.screatedateday = r.Value.creationdate.Value.Day;
                        res.screatedatemonth = r.Value.creationdate.Value.Month;
                        res.screatedateyear = r.Value.creationdate.Value.Year;
                        if (r.Value.appointmentdate != null)
                        {
                            res.snetflowdateyear = r.Value.appointmentdate.Value.Year;
                            res.snetflowdatemonth = r.Value.appointmentdate.Value.Month;
                            res.snetflowdateday = r.Value.appointmentdate.Value.Day;
                        }
                        if (r.Value.consummationdate != null)
                        {
                            res.sconsummationdateday = r.Value.consummationdate.Value.Day;
                            res.sconsummationdatemonth = r.Value.consummationdate.Value.Month;
                            res.sconsummationdateyear = r.Value.consummationdate.Value.Year;
                        }
                        if (r.Value.status != null && WebApiConfig.AdslStatus.ContainsKey(r.Value.status.Value))
                        {
                            var state = WebApiConfig.AdslStatus[r.Value.status.Value];
                            res.staskstatus = state.taskstate;
                            res.staskstatetype = StateTypeText[state.statetype.Value];
                        }
                    }
                    return res;
                }).ToList();
        }

        // Aylık başarı raporu detey için kullanılabilir.
        public static async Task<List<SKRate>> sil(DateTimeRange request)
        {
            int k_total = 0, e_total = 0;
            List<KocAdslProccess> kurulum = new List<KocAdslProccess>();
            List<KocAdslProccess> evrak = new List<KocAdslProccess>();
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            WebApiConfig.AdslProccesses.Values.Select(r =>
            {
                if (r.Kr_TON.HasValue && WebApiConfig.AdslTaskQueues.ContainsKey(r.Kr_TON.Value) && (WebApiConfig.AdslTaskQueues[r.Kr_TON.Value].taskid == 38 || WebApiConfig.AdslTaskQueues[r.Kr_TON.Value].taskid == 60)) {
                    var krtq = WebApiConfig.AdslTaskQueues[r.Kr_TON.Value];
                    var ktk = (r.Ktk_TON.HasValue && WebApiConfig.AdslTaskQueues.ContainsKey(r.Ktk_TON.Value)) ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                    var aptq = (krtq != null && WebApiConfig.AdslTaskQueues.ContainsKey(krtq.previoustaskorderid.Value)) ? WebApiConfig.AdslTaskQueues[krtq.previoustaskorderid.Value] : null;
                    var netflow = (aptq != null && aptq.appointmentdate.HasValue) ? aptq.appointmentdate.Value : (krtq != null ? (DateTime?)krtq.creationdate.Value : null);
                    if (ktk != null && netflow != null && netflow >= request.start && ktk.consummationdate.HasValue && ktk.consummationdate.Value < request.end.AddDays(7) && ktk.status.HasValue && WebApiConfig.AdslStatus.ContainsKey(ktk.status.Value) && WebApiConfig.AdslStatus[ktk.status.Value].statetype == 1)
                    {
                        k_total++;
                        kurulum.Add(r);
                    }
                    else if (netflow != null && netflow >= request.start && DateTime.Now < request.end.AddDays(7))
                        k_total++;
                }
                var stq = WebApiConfig.AdslTaskQueues.ContainsKey(r.S_TON) ? WebApiConfig.AdslTaskQueues[r.S_TON] : null;
                if (stq != null && (stq.taskid == 33 || stq.taskid == 64 || stq.taskid == 114 || stq.taskid == 31 || stq.taskid == 63 || stq.taskid == 113 || stq.taskid == 122) && ((stq.appointmentdate.HasValue && stq.appointmentdate.Value >= request.start) || stq.creationdate.Value >= request.start))
                {
                    if (stq.taskid == 33 || stq.taskid == 64 || stq.taskid == 114)
                        e_total++;
                    HashSet<int> tt = new HashSet<int>();
                    var subs = new Queue<int>();
                    if (WebApiConfig.AdslSubTasks.TryGetValue(stq.taskorderno, out tt))
                        foreach (var item in tt) subs.Enqueue(item);
                    while (subs.Count > 0)
                    {
                        var p = subs.Dequeue();
                        if (WebApiConfig.AdslSubTasks.TryGetValue(p, out tt))
                            foreach (var item in tt) subs.Enqueue(item);
                        if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTaskQueues[p].taskid == 47 && WebApiConfig.AdslTaskQueues[p].status.HasValue && WebApiConfig.AdslStatus.ContainsKey(WebApiConfig.AdslTaskQueues[p].status.Value) && WebApiConfig.AdslStatus[WebApiConfig.AdslTaskQueues[p].status.Value].statetype.Value == 1)
                            e_total++;
                        if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTaskQueues[p].taskid == 90)
                        {
                            var tq = WebApiConfig.AdslTaskQueues[p];
                            if (tq.status.HasValue && WebApiConfig.AdslStatus.ContainsKey(tq.status.Value) && WebApiConfig.AdslStatus[tq.status.Value].statetype.Value == 1 && tq.consummationdate.HasValue && tq.consummationdate.Value < request.end.AddDays(7))
                                evrak.Add(r);
                            break;
                        }
                    }
                }
                return true;
            }).ToList();
            List<DateTime> allDates = new List<DateTime>();
            Dictionary<string, SKRate> rate = new Dictionary<string, SKRate>();
            for (DateTime date = request.start.AddDays(1).AddMinutes(-1); date < request.end; date = date.AddDays(1))
                allDates.Add(date);
            foreach (var day in allDates)
            {
                if (!rate.ContainsKey($"{day.Year}-{day.Month}-{day.Day}"))
                    rate[$"{day.Year}-{day.Month}-{day.Day}"] = new SKRate();
                double ksuccess = 0, esuccess = 0;
                foreach (var item in kurulum)
                {
                    var netf = (WebApiConfig.AdslTaskQueues.ContainsKey(WebApiConfig.AdslTaskQueues[item.Kr_TON.Value].previoustaskorderid.Value) && WebApiConfig.AdslTaskQueues[WebApiConfig.AdslTaskQueues[item.Kr_TON.Value].previoustaskorderid.Value].appointmentdate.HasValue) ? WebApiConfig.AdslTaskQueues[WebApiConfig.AdslTaskQueues[item.Kr_TON.Value].previoustaskorderid.Value].appointmentdate.Value : WebApiConfig.AdslTaskQueues[item.Kr_TON.Value].creationdate.Value;
                    if (netf > day)
                        ksuccess++;
                }
            }
            return new List<SKRate>();
        }

        // Başarı oranları ve sl süreleri raporu
        public static async Task<List<SKRate>> getRates(DateTimeRange request)
        { // istenilen ay içerisinde kurulum randevusuna düşen (kr netflow tarihi o ay olan) ve bunların başarılı gerçekleşen miktarları
            var skdate = new DateTimeRange { start = request.start.AddMonths(-4), end = request.end.AddMonths(2) };
            List<SKReport> SKReport = await getSKReport(skdate).ConfigureAwait(false);
            List<DateTime> allDates = new List<DateTime>();
            for (DateTime date = request.start.AddDays(1).AddMinutes(-1); date < request.end; date = date.AddDays(1))
                allDates.Add(date);
            return allDates.Where(r => r <= DateTime.Now.AddDays(1)).Select(r =>
            {
                SKRate res = new SKRate();
                var dtr = new DateTimeRange { start = (r - r.TimeOfDay).AddDays(1 - r.Day), end = (r.AddDays(1 - r.Day).AddMonths(1).AddDays(-1)).Date.AddDays(1).AddMinutes(-1) };
                var lastDate = r.AddDays(1 - r.Day).AddMonths(1).AddDays(-1).Date;

                res.k_SLOrt = getKocKurulumSLOrt(new DateTimeRange { start = dtr.start, end = r});
                res.date = r;
                res.year = r.Year;
                res.month = r.Month;
                res.day = r.Day;

                int saySL = 0;
                double timeSL = 0;
                int totalDoc = 0;
                double completedDoc = 0;
                int totalProccess = 0;
                double completedProcess = 0;

                SKReport.Where(t =>
                {
                    var kr_tq = t.kr_ton != null ? WebApiConfig.AdslTaskQueues[t.kr_ton.Value] : null;
                    var appointment = kr_tq != null ? t.kr_netflowdateday != null ? (DateTime?)new DateTime(t.kr_netflowdateyear.Value, t.kr_netflowdatemonth.Value, t.kr_netflowdateday.Value) : null : null;
                    return kr_tq != null && appointment != null && (kr_tq.taskid == 38 || kr_tq.taskid == 60) && (r >= lastDate || (r < lastDate && kr_tq.creationdate <= r)) &&
                                appointment >= dtr.start && appointment <= dtr.end;
                }).Select(k =>
                {
                    totalProccess++;
                    var ktk_tq = k.ktk_ton != null ? WebApiConfig.AdslTaskQueues[k.ktk_ton.Value] : null;
                    if (ktk_tq != null && ktk_tq.consummationdate != null && ((r >= lastDate && ktk_tq.consummationdate < request.start.AddMonths(1).AddDays(7)) || (r < lastDate && ktk_tq.consummationdate <= r)) && ktk_tq.status != null && WebApiConfig.AdslStatus.ContainsKey(ktk_tq.status.Value) && WebApiConfig.AdslStatus[ktk_tq.status.Value].statetype == 1)
                        completedProcess++; // ktk olumlu kapatılmışsa tamamlanan miktar bir artar.
                    return res;
                }).ToList();

                // Churn evrak alma miktarları ve Evrak alma sl
                SKReport.Where(t => 
                { // 31, 33, 63, 64 taskidleri evrak alma gerektiren tasklar
                    var s_tq = WebApiConfig.AdslTaskQueues[t.s_ton];
                    return (r.Date >= lastDate || s_tq.creationdate <= r) && s_tq.appointmentdate != null && s_tq.appointmentdate.Value >= dtr.start && s_tq.appointmentdate.Value <= dtr.end && 
                                (s_tq.taskid == 31 || s_tq.taskid == 63 || s_tq.taskid == 33 || s_tq.taskid == 64 || s_tq.taskid == 114 || s_tq.taskid == 122);
                }).Select(k =>
                {  // Satış taskı evrak alınması gereken task ise
                    var s_tq = WebApiConfig.AdslTaskQueues[k.s_ton];
                    if (s_tq.taskid == 33 || s_tq.taskid == 64 || s_tq.taskid == 114)
                        totalDoc++;
                    HashSet<int> tt = new HashSet<int>();
                    var subs = new Queue<int>();
                    if (WebApiConfig.AdslSubTasks.TryGetValue(s_tq.taskorderno, out tt))
                        foreach (var item in tt) subs.Enqueue(item);
                    while (subs.Count > 0)
                    {
                        var p = subs.Dequeue();
                        if (WebApiConfig.AdslSubTasks.TryGetValue(p, out tt))
                            foreach (var item in tt) subs.Enqueue(item);
                        if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTaskQueues[p].taskid == 47 && WebApiConfig.AdslTaskQueues[p].status.HasValue && WebApiConfig.AdslStatus.ContainsKey(WebApiConfig.AdslTaskQueues[p].status.Value) && WebApiConfig.AdslStatus[WebApiConfig.AdslTaskQueues[p].status.Value].statetype.Value == 1)
                            totalDoc++;
                        if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTaskQueues[p].taskid == 90)
                        {
                            var tq = WebApiConfig.AdslTaskQueues[p];
                            if (tq.status.HasValue && WebApiConfig.AdslStatus.ContainsKey(tq.status.Value) && WebApiConfig.AdslStatus[tq.status.Value].statetype.Value == 1 && tq.consummationdate.HasValue && tq.consummationdate.Value < request.end.AddDays(7))
                            {
                                saySL++;
                                timeSL += (tq.consummationdate - s_tq.appointmentdate).Value.TotalHours;
                            }
                            break;
                        }
                    }
                    return res;
                }).ToList();

                res.k_completed = (int?)completedProcess;
                res.k_total = totalProccess;
                res.e_SuccesRate = completedDoc != 0 ? (double?)Math.Round(100 * (completedDoc / totalDoc), 2) : null;
                res.k_SuccesRate = completedProcess != 0 ? (double?)Math.Round(100 * (completedProcess / totalProccess), 2) : null;
                res.e_SLOrt = saySL != 0 ? (double?)Math.Round(timeSL / saySL, 2) : null;

                return res;
            }).ToList();
        }

        // Bayi bilgileri üzerindeki modem miktarları ve üzerlerindeki iş miktarları
        public static async Task<List<InfoBayiReport>> getInfoBayi()
        {
            var bekleyenTaskq = await getSKStandbyTaskReport();
            return WebApiConfig.AdslPersonels.Where(r => {
                var mail = r.Value.email != null ? r.Value.email.Contains("@") ? r.Value.email.Split('@')[1] : null : null;
                return mail != "kociletisim.com.tr" && r.Value.deleted == false;
            }).Select(r => {
                InfoBayiReport res = new InfoBayiReport();
                res.personelid = r.Value.personelid;
                res.personelname = r.Value.personelname;
                res.email = r.Value.email;
                res.il = r.Value.ilKimlikNo.HasValue ? WebApiConfig.AdslIls.ContainsKey(r.Value.ilKimlikNo.Value) ? WebApiConfig.AdslIls[r.Value.ilKimlikNo.Value].ad : "İl Girilmemiş" : "İl Girilmemiş";
                res.ilce = r.Value.ilceKimlikNo.HasValue ? WebApiConfig.AdslIlces.ContainsKey(r.Value.ilceKimlikNo.Value) ? WebApiConfig.AdslIlces[r.Value.ilceKimlikNo.Value].ad : "İlce Girilmemiş" : "İlce Girilmemiş";
                res.worksay = bekleyenTaskq.Where(t => t.personelid == r.Value.personelid && WebApiConfig.AdslTasks.ContainsKey(t.taskid) && WebApiConfig.AdslTaskTypes.ContainsKey(WebApiConfig.AdslTasks[t.taskid].tasktype) && WebApiConfig.AdslTaskTypes[WebApiConfig.AdslTasks[t.taskid].tasktype].TaskTypeId != 0).ToList().Count;
                using (var db = new KOCSAMADLSEntities(false))
                    res.modemsay = db.getSerialsOnPersonelAdsl(r.Value.personelid, 1117).ToList().Count;
                return res;
            }).ToList();
        }

        public static async Task<List<StockMovementBackSeri>> getSMBS ()
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            using (var db = new KOCSAMADLSEntities())
            return WebApiConfig.AdslTaskQueues.Where(r => r.Value.status.HasValue && r.Value.deleted == false && WebApiConfig.AdslStatus.ContainsKey(r.Value.status.Value) && WebApiConfig.AdslStatus[r.Value.status.Value].statetype == 2 /*İptal Durumu*/ && (r.Value.taskid == 42 || r.Value.taskid == 97 || r.Value.taskid == 49) && db.getSerialsOnCustomerAdsl(r.Value.attachedobjectid, 1117/*Modem*/).FirstOrDefault() != null).Select(r =>
            {
                var res = new StockMovementBackSeri();
                res.customerid = r.Value.attachedobjectid.Value;
                if (WebApiConfig.AdslCustomers.ContainsKey(r.Value.attachedobjectid.Value))
                {
                    var cust = WebApiConfig.AdslCustomers[r.Value.attachedobjectid.Value];
                    res.customername = cust.customername;
                    res.superonlineno = cust.superonlineCustNo;
                }
                res.taskNo = r.Value.taskorderno;
                res.task = WebApiConfig.AdslTasks.ContainsKey(r.Value.taskid) ? WebApiConfig.AdslTasks[r.Value.taskid].taskname : "İsimsiz Task";
                res.seri = db.getSerialsOnCustomerAdsl(r.Value.attachedobjectid, 1117/*Modem*/).FirstOrDefault();
                var person = db.stockmovement.Where(per => per.serialno == res.seri && per.toobject == res.customerid && per.deleted == false).OrderByDescending(ss => ss.movementid).FirstOrDefault();
                res.personelid = person.fromobject.Value;
                res.personelname = WebApiConfig.AdslPersonels.ContainsKey(res.personelid) ? WebApiConfig.AdslPersonels[res.personelid].personelname : "İsimsiz Personel";
                return res;
            }).ToList();
        }

        // Muhasebe programı Cari Hareketler raporu
        public static async Task<List<CRMWebApi.DTOs.Cari.CariHareketReport>> getCariHareketler ()
        {
            Dictionary<int, CRMWebApi.DTOs.Cari.CariHareketReport> info = new Dictionary<int, DTOs.Cari.CariHareketReport>();
            await WebApiConfig.updateCariData().ConfigureAwait(false);
            WebApiConfig.CariHareketler.Select(r => {
                int periodId = Convert.ToInt32(r.Value.AlacakliFirma.ToString() + r.Value.Donem.Year.ToString() + r.Value.Donem.Month.ToString());
                if (!info.ContainsKey(periodId)) info[periodId] = new DTOs.Cari.CariHareketReport();
                info[periodId].personelid = r.Value.AlacakliFirma;
                info[periodId].personelname = WebApiConfig.CariPersonels.ContainsKey(r.Value.AlacakliFirma) ? WebApiConfig.CariPersonels[r.Value.AlacakliFirma].personelname : "İsimsiz Bayi";
                info[periodId].periodId = periodId;
                info[periodId].alacak += r.Value.Tutar;
                if (r.Value.Odendi) info[periodId].odenen += r.Value.Tutar;
                info[periodId].period = r.Value.Donem.AddDays(1 - r.Value.Donem.Day).AddMonths(1).AddDays(-1).ToShortDateString();
                return true;
            }).ToList();
            return info.Select(r => {
                DTOs.Cari.CariHareketReport res = new DTOs.Cari.CariHareketReport();
                res.periodId = r.Value.periodId;
                res.personelid = r.Value.personelid;
                res.personelname = r.Value.personelname;
                res.period = r.Value.period;
                res.alacak = r.Value.alacak;
                res.odenen = r.Value.odenen;
                return res;
            }).OrderBy(r => r.personelid).ToList();
        }

        // SAM SDM ve CC TİM Taskları için evrak alım durumu
        public static async Task<List<EvrakBasari>> getEvrakBasari()
        {
            var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslProccesses.Where(t => {
                var stq = WebApiConfig.AdslTaskQueues.ContainsKey(t.Value.S_TON) ? WebApiConfig.AdslTaskQueues[t.Value.S_TON] : null;
                return stq != null && (stq.taskid == 33 || stq.taskid == 64 || stq.taskid == 31 || stq.taskid == 63 || stq.taskid == 113 || stq.taskid == 122 || stq.taskid == 114);
            }).Select(r => {
                EvrakBasari res = new EvrakBasari();
                var stq = WebApiConfig.AdslTaskQueues[r.Value.S_TON];
                adsl_taskqueue samtq = null;
                res.custid = stq.attachedobjectid.Value;
                res.stask_desc = stq.description;
                if (WebApiConfig.AdslCustomers.ContainsKey(stq.attachedobjectid.Value))
                {
                    var cust = WebApiConfig.AdslCustomers[stq.attachedobjectid.Value];
                    res.custname = cust.customername;
                    res.custsolid = cust.superonlineCustNo;
                    res.il = cust.ilKimlikNo.HasValue ? WebApiConfig.AdslIls.ContainsKey(cust.ilKimlikNo.Value) ? WebApiConfig.AdslIls[cust.ilKimlikNo.Value].ad : "İsimsiz İl" : "Boş";
                    res.ilce = cust.ilceKimlikNo.HasValue ? WebApiConfig.AdslIlces.ContainsKey(cust.ilceKimlikNo.Value) ? WebApiConfig.AdslIlces[cust.ilceKimlikNo.Value].ad : "İsimsiz İlce" : "Boş";
                }
                else
                    res.custname = "İsimsiz Müşteri";
                res.s_personel = stq.attachedpersonelid.HasValue ? WebApiConfig.AdslPersonels.ContainsKey(stq.attachedpersonelid.Value) ? WebApiConfig.AdslPersonels[stq.attachedpersonelid.Value].personelname : "İsimsiz Personel" : "Atanmamış";
                res.s_ton = r.Value.S_TON;
                res.s_tname = WebApiConfig.AdslTasks.ContainsKey(stq.taskid) ? WebApiConfig.AdslTasks[stq.taskid].taskname : "İsimsiz Task";
                res.kaynak = stq.fault;
                res.s_create_day = stq.creationdate.Value.Day;
                res.s_create_month = stq.creationdate.Value.Month;
                res.s_create_year = stq.creationdate.Value.Year;
                if (stq.taskid == 33 || stq.taskid == 64 || stq.taskid == 114)
                    res.slstart = stq.appointmentdate.HasValue ? stq.appointmentdate : stq.creationdate;
                if (stq.appointmentdate.HasValue)
                {
                    res.s_netflow_day = stq.appointmentdate.Value.Day;
                    res.s_netflow_month = stq.appointmentdate.Value.Month;
                    res.s_netflow_year = stq.appointmentdate.Value.Year;
                }
                if (stq.consummationdate.HasValue)
                {
                    res.s_close_day = stq.consummationdate.Value.Day;
                    res.s_close_month = stq.consummationdate.Value.Month;
                    res.s_close_year = stq.consummationdate.Value.Year;
                }
                if (stq.status.HasValue)
                {
                    if (WebApiConfig.AdslStatus.ContainsKey(stq.status.Value))
                    {
                        res.s_status = WebApiConfig.AdslStatus[stq.status.Value].taskstate;
                        if (WebApiConfig.AdslStatus[stq.status.Value].statetype == 2)
                        {
                            res.s_statustype = "İptal Edilen";
                            res.process_status = "İptal Edilen";
                        }
                        else
                        {
                            res.s_statustype = StateTypeText[WebApiConfig.AdslStatus[stq.status.Value].statetype.Value];
                            HashSet<int> tt = new HashSet<int>();
                            var subs = new Queue<int>();
                            if (WebApiConfig.AdslSubTasks.TryGetValue(stq.taskorderno, out tt))
                                foreach (var item in tt) subs.Enqueue(item);
                            else
                                res.process_status = StateTypeText[WebApiConfig.AdslStatus[stq.status.Value].statetype.Value];
                            while (subs.Count > 0)
                            {
                                var p = subs.Dequeue();
                                if (WebApiConfig.AdslSubTasks.TryGetValue(p, out tt))
                                    foreach (var item in tt) subs.Enqueue(item);
                                if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTaskQueues[p].taskid == 90)
                                {
                                    samtq = WebApiConfig.AdslTaskQueues[p];
                                    break;
                                }
                                else if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTaskQueues[p].status == null)
                                    res.process_status = "Bekleyen";
                                else if (WebApiConfig.AdslTaskQueues.ContainsKey(p) && WebApiConfig.AdslTaskQueues[p].status != null && WebApiConfig.AdslStatus.ContainsKey(WebApiConfig.AdslTaskQueues[p].status.Value))
                                    res.process_status = StateTypeText[WebApiConfig.AdslStatus[WebApiConfig.AdslTaskQueues[p].status.Value].statetype.Value];
                            }
                        }
                    }
                    else
                    {
                        res.s_status = "Tanımsız Durum";
                        res.process_status = "Tanımsız Durum İhlali";
                    }
                }
                else
                {
                    res.s_statustype = "Bekleyen";
                    res.process_status = "Bekleyen";
                }
                if (samtq != null)
                {
                    res.kapamatask_ton = samtq.taskorderno;
                    res.kapamatask_name = WebApiConfig.AdslTasks.ContainsKey(samtq.taskid) ? WebApiConfig.AdslTasks[samtq.taskid].taskname : "İsimsiz Task";
                    res.kapamatask_personel = samtq.attachedpersonelid.HasValue ? WebApiConfig.AdslPersonels.ContainsKey(samtq.attachedpersonelid.Value) ? WebApiConfig.AdslPersonels[samtq.attachedpersonelid.Value].personelname : "İsimsiz Personel" : "Atanmamış";
                    res.kapamatask_create_day = samtq.creationdate.Value.Day;
                    res.kapamatask_create_month = samtq.creationdate.Value.Month;
                    res.kapamatask_create_year = samtq.creationdate.Value.Year;
                    res.kapamatask_desc = samtq.description;
                    if (stq.taskid == 31 || stq.taskid == 63 || stq.taskid == 113 || stq.taskid == 122)
                        res.slstart = samtq.creationdate;
                    if (samtq.consummationdate.HasValue)
                    {
                        res.kapamatask_close_day = samtq.consummationdate.Value.Day;
                        res.kapamatask_close_month = samtq.consummationdate.Value.Month;
                        res.kapamatask_close_year = samtq.consummationdate.Value.Year;
                        res.slfinish = samtq.consummationdate;
                        //res.sl = (TimeSpan?)(res.slstart.Value - res.slfinish.Value);
                    }
                    if (samtq.status.HasValue)
                    {
                        if (WebApiConfig.AdslStatus.ContainsKey(samtq.status.Value))
                        {
                            res.kapamatask_statustype = StateTypeText[WebApiConfig.AdslStatus[samtq.status.Value].statetype.Value];
                            res.process_status = StateTypeText[WebApiConfig.AdslStatus[samtq.status.Value].statetype.Value];
                        }
                        else
                        {
                            res.kapamatask_status = "Tanımsız Durum";
                            res.process_status = "Tanımsız Durum İhlali";
                        }
                    }
                    else
                    {
                        res.kapamatask_statustype = "Bekleyen";
                        res.process_status = "Bekleyen";
                    }
                }
                return res;
            }).ToList();
        }

        [Route("SKR")]
        [HttpPost]
        public async Task<HttpResponseMessage> SKR(DateTimeRange request)
        {
            var stw = Stopwatch.StartNew();
            var report = await getSKReport(request).ConfigureAwait(false);
            stw.Stop();
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                data = report,
                count = report.Count,
                timming = stw.Elapsed
            }, "application/json");
        }

        [Route("SKRGet")]
        [HttpGet]
        public async Task<HttpResponseMessage> SKR([FromUri] string start, [FromUri] string end )
        {
            var request = new DateTimeRange {
                start = DateTime.ParseExact(start, "d", System.Globalization.CultureInfo.InvariantCulture),
                end = DateTime.ParseExact(end,  "d", System.Globalization.CultureInfo.InvariantCulture),
            };
            var report = await getSKReport(request).ConfigureAwait(false);
            var dataString = new List<string>();
            dataString.Add(SKReport.GetHeadhers());
            dataString.AddRange(report.Select(r => r.ToString()));
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StringContent(string.Join("\r\n", dataString));
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "SKReport.csv" };
                return result;
        }

        [Route("SLBayiGet")]
        [HttpGet]
        public async Task<HttpResponseMessage> SLBayiGet([FromUri] int BayiId)
        { // bayi bulunduğumuz ay sl detay raporunu indirmesi için
            var d = DateTime.Now;
            var request = new DateTimeRange { start = (d - d.TimeOfDay).AddDays(1 - d.Day), end = (d.AddDays(1 - d.Day).AddMonths(1).AddDays(-1)).Date.AddDays(1) };
            var report = await getBayiSLReport(BayiId, request).ConfigureAwait(false);
            var dataString = new List<string>();
            dataString.Add(SLBayiReport.GetHeadhers());
            dataString.AddRange(report.Select(r => r.ToString()));
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(string.Join("\r\n", dataString), Encoding.GetEncoding("ISO-8859-9"));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "SLReport.csv" };
            return result;
        }

        [Route("GSLBayiGet")]
        [HttpGet]
        public async Task<HttpResponseMessage> GSLBayiGet([FromUri] int BayiId)
        { // bayi geçen ay sl detay raporunu indirmesi için
            var d = DateTime.Now;
            var request = new DateTimeRange { start = ((d - d.TimeOfDay).AddDays(1 - d.Day)).AddMonths(-1), end = ((d.AddDays(1 - d.Day).AddMonths(1).AddDays(-1)).Date.AddDays(1)).AddMonths(-1) };
            var report = await getBayiSLReport(BayiId, request).ConfigureAwait(false);
            var dataString = new List<string>();
            dataString.Add(SLBayiReport.GetHeadhers());
            dataString.AddRange(report.Select(r => r.ToString()));
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(string.Join("\r\n", dataString), Encoding.GetEncoding("ISO-8859-9"));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "GSLReport.csv" };
            return result;
        }
    }
}