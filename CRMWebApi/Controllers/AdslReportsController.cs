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
using System.IO;
using System.Net.Http.Headers;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using System.Text;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Reports")]
    public class AdslReportsController : ApiController
    {
        [Route("getPersonelWorks")]
        [HttpPost]
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
            var slOrt = new double[] { getKocSLOrt(dtr), getKocSLOrt(dtr2) };
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
                 var k_tq = WebApiConfig.AdslTaskQueues[r.K_TON.Value];
                 if (k_tq.attachedpersonelid.Value == s_tq.attachedpersonelid.Value && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 1)
                 { // satış taskı; satış ise ve satış yapan bayi kurulum yapmışsa bayinin sat-kur hakedişine 1 ekle
                     if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                     total[s_tq.attachedpersonelid.Value].sat_kur++;
                 }
                 else if (WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 9)
                 { // satış taskı; CC Satışı ise bayinin kurulum hakedişine 1 ekle
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].kur++;
                 }
                 else if (k_tq.attachedpersonelid.Value != s_tq.attachedpersonelid.Value && WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 1)
                 { // satış taskı; bayi satış taskı ise satış yapan bayinin satış, kurulum yapan bayinin kurulum hakedişine 1 ekle
                     if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                     total[s_tq.attachedpersonelid.Value].sat++;
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].kur++;
                 }
                 else if (WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 8)
                 { // satış taskı; teslimat ise bayinin teslimat hakedişine 1 ekle
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].teslimat++;
                 }
                 else if (WebApiConfig.AdslTasks[s_tq.taskid].tasktype == 6)
                 { // satış taskı; arıza ise bayinin arıza hakedişine 1 ekle
                     if (!total.ContainsKey(k_tq.attachedpersonelid.Value)) total[k_tq.attachedpersonelid.Value] = new SKPayment();
                     total[k_tq.attachedpersonelid.Value].ariza++;
                 }
                 return true;
             }).ToList();
            WebApiConfig.AdslTaskQueues.Where(r =>
            {
                var tq = r.Value; // Sam sdm sipariş kapama taskid = 90 (process iptal dahi olsa bayi ekrakları alıp koç personel sam sdm kapatmışsa bayi hakediş alacak)
                return tq.taskid == 90 && tq.status != null && WebApiConfig.AdslStatus.ContainsKey(tq.status.Value) && WebApiConfig.AdslStatus[tq.status.Value].statetype.Value == 1 && tq.consummationdate.HasValue && tq.consummationdate.Value >= request.start && tq.consummationdate.Value <= request.end;
            }).Select(r =>
            {
                var s_tq = r.Value.previoustaskorderid.HasValue ? WebApiConfig.AdslTaskQueues.ContainsKey(r.Value.previoustaskorderid.Value) ? WebApiConfig.AdslTaskQueues[r.Value.previoustaskorderid.Value] : null : null;
                if (s_tq != null && s_tq.taskid == 33 || s_tq.taskid == 64) // başlangıç task tipi ekrak alma olamaz evrak alma tasklarının idleri ile kontrol edildi (Hüseyin KOZ) !!
                { // evrak alınmışsa bayinin evrak alma hakedişine 1 ekle
                    if (!total.ContainsKey(s_tq.attachedpersonelid.Value)) total[s_tq.attachedpersonelid.Value] = new SKPayment();
                    total[s_tq.attachedpersonelid.Value].evrak++;
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
        
        private static double getKocSLOrt(DateTimeRange request)
        {
            int sayCust = 0;
            double timeOrt = 0;
            var maxSL = WebApiConfig.AdslSl.OrderByDescending(k => k.Value.KocMaxTime).Select(k => k.Value.KocMaxTime).First(); // sl tablosunda bayiler için tanımlı maxSL'lerin en büyüğü (çarpan için)
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
                        factor = (maxSL != null && kocSl.KocMaxTime != null) ? (maxSL.Value / kocSl.KocMaxTime.Value) : 1;
                    }
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
                res.bName = WebApiConfig.AdslPersonels.ContainsKey(r.Key) ? WebApiConfig.AdslPersonels[r.Key].personelname : "İsimsiz Personel";
                res.sat = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 1 && r.Value.sat <= h.Value.upperLimitAmount).Select(h => h.Value.payment).FirstOrDefault() * r.Value.sat) ?? 0;
                res.kur = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 2 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.kur) ?? 0;
                res.sat_kur = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 3 && r.Value.sat_kur <= h.Value.upperLimitAmount && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.sat_kur) ?? 0;
                res.ariza = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 4 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.ariza) ?? 0;
                res.teslimat = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 5 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.teslimat) ?? 0;
                res.evrak = (WebApiConfig.AdslPaymentSystem.Where(h => h.Value.paymentType == 6 && bSLOrt <= h.Value.upperLimitSL).Select(h => h.Value.payment).FirstOrDefault() * r.Value.evrak) ?? 0;
                return res;
            }).ToList();
        }

        // Bütün bekleyen tasklar
        public static async Task<List<SKStandbyTaskReport>> getSKStandbyTaskReport()
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslTaskQueues.Where(r=> r.Value.status == null && r.Value.deleted == false).Select(r =>
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
                    res.customername = WebApiConfig.AdslCustomers[r.Value.attachedobjectid.Value].customername;
                    res.customeradres = WebApiConfig.AdslCustomers[r.Value.attachedobjectid.Value].description;
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
                res.description = r.Value.description;
                res.fault = r.Value.fault;
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
                    if (WebApiConfig.AdslTasks.ContainsKey(kr_tq.taskid))
                        res.kr_tqname = WebApiConfig.AdslTasks[kr_tq.taskid].taskname;
                    if (kr_tq.attachedpersonelid != null && WebApiConfig.AdslPersonels.ContainsKey(kr_tq.attachedpersonelid.Value))
                    {
                        var krPersonelInfo = WebApiConfig.AdslPersonels[kr_tq.attachedpersonelid.Value];
                        res.kr_perid = krPersonelInfo.personelid;
                        res.kr_pername = krPersonelInfo.personelname;
                        //res.kr_perky = null;
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
                }
                else
                    res.lastTaskStatus = "Bekleyen";
                var lastTask = WebApiConfig.AdslTasks[lasttq.taskid];
                res.lastTaskTypeName = WebApiConfig.AdslTaskTypes[lastTask.tasktype].TaskTypeName;
                res.lastTaskName = lastTask.taskname;
                res.lasttaskcretiondateyear = lasttq.creationdate.Value.Year;
                res.lasttaskcretiondatemonth = lasttq.creationdate.Value.Month;
                res.lasttaskcretiondateday = lasttq.creationdate.Value.Day;
                return res;
            }).ToList();
        }

        public static async Task<List<SLBayiReport>> getBayiSLReport(int BayiId, DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            var maxSL = WebApiConfig.AdslSl.OrderByDescending(k => k.Value.BayiMaxTime).Select(k => k.Value.BayiMaxTime).FirstOrDefault(); // sl tablosunda bayiler için tanımlı maxSL'lerin en büyüğü (çarpan için)
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
                    else
                        r.slstatus = "Bekleyen";
                    r.BayiSLTaskStart = bsl.Value.BStart;
                    r.BayiSLEnd = bsl.Value.BEnd;
                    if (WebApiConfig.AdslSl.ContainsKey(bsl.Key)) // bayi max time eklendi fazla sart olmasın diye tek ifte toplandı
                    {
                        var bayiSl = WebApiConfig.AdslSl[bsl.Key];
                        r.SLName = bayiSl.SLName;
                        r.BayiSLMaxTime = bayiSl.BayiMaxTime != null ? bayiSl.BayiMaxTime.Value : 0;
                        r.BayiSLEtkisi = r.BayiSLStart == null ? null : (double?)Math.Round((((r.BayiSLEnd - r.BayiSLStart).Value.TotalHours)*((maxSL != null && bayiSl.BayiMaxTime != null) ? (maxSL.Value / bayiSl.BayiMaxTime.Value) : 1)),2);
                    }
                    else
                        r.SLName = "Tanımlanmamış SL";
                    if (bsl.Value.BayiID.HasValue && WebApiConfig.AdslPersonels.ContainsKey(bsl.Value.BayiID.Value)) // Bayi bilgileri yazılmıyordu boş geliyordu o yüzden ekledim gerek yoksa bilgiler slkocreport classına alınabilir ?
                    {
                        var Bayi = WebApiConfig.AdslPersonels[bsl.Value.BayiID.Value];
                        r.BayiId = Bayi.personelid;
                        r.BayiName = Bayi.personelname;
                        r.Il = WebApiConfig.AdslIls.ContainsKey(Bayi.ilKimlikNo ?? 0) ? WebApiConfig.AdslIls[Bayi.ilKimlikNo.Value].ad : null;
                        r.Ilce = WebApiConfig.AdslIlces.ContainsKey(Bayi.ilceKimlikNo ?? 0) ? WebApiConfig.AdslIlces[Bayi.ilceKimlikNo.Value].ad : null;
                    }
                    r.CustomerId = bsl.Value.CustomerId;
                    r.CustomerName = WebApiConfig.AdslCustomers.ContainsKey(bsl.Value.CustomerId) ?
                        WebApiConfig.AdslCustomers[bsl.Value.CustomerId].customername : "Tanımlanmamış Müşteri";
                    return r;
                })
            ).ToList();
        }

        public static async Task<List<SLKocReport>> getKocSLReport(DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            var maxSL = WebApiConfig.AdslSl.OrderByDescending(k => k.Value.BayiMaxTime).Select(k => k.Value.BayiMaxTime).FirstOrDefault(); // sl tablosunda bayiler için tanımlı maxSL'lerin en büyüğü (çarpan için)
            var maxKSL = WebApiConfig.AdslSl.OrderByDescending(k => k.Value.KocMaxTime).Select(k => k.Value.KocMaxTime).FirstOrDefault(); // sl tablosunda koç için tanımlı maxSL'lerin en büyüğü (çarpan için)
            return WebApiConfig.AdslProccesses.Values.SelectMany(
                p => p.SLs.Where(sl => {
                    var lasttq = WebApiConfig.AdslTaskQueues[p.Last_TON];
                    var date = DateTime.Now; // geçmiş aylarda bekleyen işlem gözükmemesi için oluşturuldu
                    return (!sl.Value.KEnd.HasValue && ((lasttq.consummationdate == null && request.start <= date && request.end >= date) || (lasttq.consummationdate != null && lasttq.consummationdate >= request.start && lasttq.consummationdate <= request.end))) || (sl.Value.KEnd.HasValue && sl.Value.KEnd.Value >= request.start && sl.Value.KEnd.Value <= request.end) || (sl.Value.BEnd.HasValue && sl.Value.BEnd.Value >= request.start && sl.Value.BEnd.Value <= request.end);
                }).Select(ksl =>
                {
                    var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
                    var lasttq = WebApiConfig.AdslTaskQueues[p.Last_TON];
                    var r = new SLKocReport();
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
                    else
                        r.slstatus = "Bekleyen";
                    if ((ksl.Value.BEnd.HasValue && ksl.Value.BEnd.Value >= request.start && ksl.Value.BEnd.Value <= request.end ) || (!ksl.Value.BEnd.HasValue && ksl.Value.BStart.HasValue && ksl.Value.BStart.Value <= request.end))
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
                        r.BayiSLMaxTime = kocSl.BayiMaxTime != null ? kocSl.BayiMaxTime.Value : 0;
                        r.KocSLMaxTime = kocSl.KocMaxTime != null ? kocSl.KocMaxTime.Value : 0;
                        r.BayiSLEtkisi = r.BayiSLStart == null ? null : (double?)Math.Round((((r.BayiSLEnd - r.BayiSLStart).Value.TotalHours) * ((maxSL != null && kocSl.BayiMaxTime != null) ? (maxSL.Value / kocSl.BayiMaxTime.Value) : 1)), 2);
                        r.KocSLEtkisi = r.KocSLStart == null ? null : (double?)Math.Round((((r.KocSLEnd - r.KocSLStart).Value.TotalHours) * ((maxSL != null && kocSl.KocMaxTime != null) ? (maxKSL.Value / kocSl.KocMaxTime.Value) : 1)), 2);
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
                    r.CustomerName = WebApiConfig.AdslCustomers.ContainsKey(ksl.Value.CustomerId) ?
                        WebApiConfig.AdslCustomers[ksl.Value.CustomerId].customername : "Tanımlanmamış Müşteri";
                    return r;
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
                res.il = r.Value.ilKimlikNo != null ? WebApiConfig.AdslIls.ContainsKey(r.Value.ilKimlikNo.Value) ? WebApiConfig.AdslIls[r.Value.ilKimlikNo.Value].ad : null : null;
                res.ilce = r.Value.ilceKimlikNo != null ? WebApiConfig.AdslIlces.ContainsKey(r.Value.ilceKimlikNo.Value) ? WebApiConfig.AdslIlces[r.Value.ilceKimlikNo.Value].ad : null : null;
                res.kanalyoneticisi = r.Value.relatedpersonelid != null ? WebApiConfig.AdslPersonels.ContainsKey(r.Value.relatedpersonelid.Value) ? WebApiConfig.AdslPersonels[r.Value.relatedpersonelid.Value].personelname : null : null;
                res.kurulumbayisi = r.Value.kurulumpersonelid != null ? WebApiConfig.AdslPersonels.ContainsKey(r.Value.kurulumpersonelid.Value) ? WebApiConfig.AdslPersonels[r.Value.kurulumpersonelid.Value].personelname : null : null;
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