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
                    Where(p => p.attachedpersonelid == request.personelid && p.deleted == false && (p.task.taskid == 32 || p.task.taskid == 33 || p.task.taskid == 38 ||
                    p.task.taskid == 39 || p.task.taskid == 40 || p.task.taskid == 59 || p.task.taskid == 60 || p.task.taskid == 62 || p.task.taskid == 63 || p.task.taskid == 64 || p.task.taskid == 65 || p.task.taskid == 79 || p.task.taskid == 72 || p.task.taskid == 80 || p.task.taskid == 75 || p.task.taskid == 76 || p.task.taskid == 82 || p.task.taskid == 86 || p.task.taskid == 85 || p.task.taskid == 55 || p.task.taskid == 54 || p.task.taskid == 102
                    || p.task.taskid == 41 || p.task.taskid == 49 || p.task.taskid == 51) && p.status == null).OrderBy(s => s.attachedcustomer.customername).ToList();
                res.ForEach(r =>
                {
                    var salestaskorderno = db.taskqueue.Where(t => t.task.tasktype == 1 && t.attachedobjectid == r.attachedobjectid)
                           .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();

                    r.customerproduct = db.customerproduct.Include(s => s.campaigns).Where(c => c.taskid == salestaskorderno).ToList();
                    r.description = db.taskqueue.Where(s => s.taskorderno == salestaskorderno).Select(s => s.description).FirstOrDefault();
                });
                return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()), "application/json");
            }
        }

        // Satış kurulum raporu
        public static async Task<List<SKReport>> getSKReport(DTOs.Adsl.DTORequestClasses.DateTimeRange request)
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
                    if (customerInfo.ilKimlikNo != null && WebApiConfig.AdslIls.ContainsKey(customerInfo.ilKimlikNo.Value))
                        res.il = WebApiConfig.AdslIls[customerInfo.ilKimlikNo.Value].ad;
                    if (customerInfo.ilceKimlikNo != null && WebApiConfig.AdslIlces.ContainsKey(customerInfo.ilceKimlikNo.Value))
                        res.ilce = WebApiConfig.AdslIlces[customerInfo.ilceKimlikNo.Value].ad;
                    res.gsm = customerInfo.gsm;

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
                res.campaign = null; // campaign ve customerproduct dictionary olması gerek
                res.kaynak = s_tq.fault;
                res.s_desc = s_tq.description;
                if (s_tq.appointmentdate != null)
                {
                    res.s_year = s_tq.appointmentdate.Value.Year;
                    res.s_month = s_tq.appointmentdate.Value.Month;
                    res.s_day = s_tq.appointmentdate.Value.Day;
                }
                else
                {
                    res.s_year = s_tq.creationdate.Value.Year;
                    res.s_month = s_tq.creationdate.Value.Month;
                    res.s_day = s_tq.creationdate.Value.Day;
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
                        var kPersonelInfo = WebApiConfig.AdslPersonels[kr_tq.attachedpersonelid.Value];
                        res.kr_perid = kPersonelInfo.personelid;
                        res.kr_pername = kPersonelInfo.personelname;
                        res.k_perid = kPersonelInfo.personelid;
                        res.k_pername = kPersonelInfo.personelname;
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
                return res;
            }).ToList();

        }

        public static async Task<List<SLBayiReport>> getBayiSLReport(int BayiId, DTOs.Adsl.DTORequestClasses.DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                var last_tq = WebApiConfig.AdslTaskQueues[r.Last_TON];
                var ktk_tq = r.Ktk_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : last_tq;
                return ktk_tq.status == null || ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).SelectMany(
                p => p.SLs.Where(sl => sl.Value.BayiID.HasValue && sl.Value.BayiID == BayiId) // where tekrar koydum bayiye göre bilgi çekmiyordu tüm bayilerin raporunu alıyordu (Hüseyin KOZ)
                .Select(bsl =>
                {
                    try
                    {
                        var r = new SLBayiReport();
                        if (WebApiConfig.AdslSl.ContainsKey(bsl.Key)) // bayi max time eklendi fazla sart olmasın diye tek ifte toplandı
                        {
                            var bayiSl = WebApiConfig.AdslSl[bsl.Key];
                            r.SLName = bayiSl.SLName;
                            r.BayiSLMaxTime =  bayiSl.BayiMaxTime != null ? bayiSl.BayiMaxTime.Value : 0; 
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
                        r.BayiSLTaskStart = bsl.Value.BStart;
                        r.BayiSLEnd = bsl.Value.BEnd;
                        return r;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                })
            ).ToList();
        }

        public static async Task<List<SLKocReport>> getKocSLReport(DTOs.Adsl.DTORequestClasses.DateTimeRange request)
        {
            await WebApiConfig.updateAdslData().ConfigureAwait(false);
            return WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                var last_tq = WebApiConfig.AdslTaskQueues[r.Last_TON];
                var ktk_tq = r.Ktk_TON.HasValue ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : last_tq;
                return ktk_tq.status == null || ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).SelectMany(
                p => p.SLs
                //.Where(sl => sl.Value.KStart >= request.start && sl.Value.KEnd <= request.end)
                .Select(ksl => {
                    try
                    {
                        var r = new SLKocReport();
                        if (WebApiConfig.AdslSl.ContainsKey(ksl.Key)) // bayi max time ve koc max time eklendi fazla sart olmasın diye tek ifte toplandı
                        {
                            var kocSl = WebApiConfig.AdslSl[ksl.Key];
                            r.SLName = kocSl.SLName;
                            r.BayiSLMaxTime = kocSl.BayiMaxTime != null ? kocSl.BayiMaxTime.Value : 0; // Veritabanından Çek WebApiConfig.AdslSl[bsl.Key].MaxTime gibi
                            r.KocSLMaxTime = kocSl.KocMaxTime != null ? kocSl.KocMaxTime.Value : 0;
                        }
                        else
                            r.SLName = "Tanımlanmamış SL";
                        r.SLName = WebApiConfig.AdslSl.ContainsKey(ksl.Key) ? WebApiConfig.AdslSl[ksl.Key].SLName : "Tanımlanmamış SL";
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
                        r.BayiSLTaskStart = ksl.Value.BStart;
                        r.BayiSLEnd = ksl.Value.BEnd;
                        r.KocSLStart = ksl.Value.KStart.Value;
                        r.KocSLEnd = ksl.Value.KEnd;
                        return r;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                })
            ).ToList();
        }

        [Route("SKR")]
        [HttpPost]
        public async Task<HttpResponseMessage> SKR(DTOs.Adsl.DTORequestClasses.DateTimeRange request)
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
            var request = new DTOs.Adsl.DTORequestClasses.DateTimeRange {
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

        [Route("Taskqueues")]
        [HttpPost]
        public async Task<HttpResponseMessage> Taskqueues(DTOs.Adsl.DTORequestClasses.DateTimeRange request)
        {
            await WebApiConfig.updateAdslData();
            var list = WebApiConfig.AdslTaskQueues.Count;
            return Request.CreateResponse(HttpStatusCode.OK, list, "application/json");
        }

        [Route("Test")]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { count = WebApiConfig.AdslTaskQueues.Count }, "application/json");
        }
    }
}