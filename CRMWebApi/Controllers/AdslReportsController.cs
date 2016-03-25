using CRMWebApi.Models.Adsl;
using CRMWebApi.DTOs.Adsl;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Linq;
using System;

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
        [Route("SKR")]
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> SKR(DTOs.Adsl.DTORequestClasses.DateTimeRange request)
        {
            await WebApiConfig.updateAdslData();
            var TaskTypeText = new string[] { "Diğer", "Satış Taskı", "Randevu Taskı", "Kurulum Taskı", "Randuvusuz Kurulum Taskı", "SOL Kapama Taskı", "Arıza Taskı", "Evrak Alma Taskı", "Teslimat Taskı" };
            var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
            return Request.CreateResponse(HttpStatusCode.OK, WebApiConfig.AdslProccesses.Values.Where(r =>
            {
                if (!r.Ktk_TON.HasValue)
                {
                    DateTime sTime = request.start.AddMonths(-2);
                    var stq = WebApiConfig.AdslTaskQueues[r.S_TON];
                    return stq.creationdate >= sTime && stq.consummationdate <= request.end;
                    //return true;
                }
                var ktk_tq = WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value];
                return ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).Select(r =>
            {
                var s_tq = WebApiConfig.AdslTaskQueues[r.S_TON];
                var kr_tq = (r.Kr_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Kr_TON.Value] : null;
                var k_tq = (r.K_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.K_TON.Value] : null;
                var ktk_tq = (r.Ktk_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                var lasttq = r.Last_TON != 0 ? WebApiConfig.AdslTaskQueues[r.Last_TON] : (ktk_tq ?? k_tq ?? kr_tq ?? s_tq);
                var taskType = TaskTypeText[lasttq.task.tasktypes.TaskTypeId];
                var res = new SLReport();
                //Satış kurulum view sonucuna göre class tanımı yap ve o class türünde bir nesne oluşturup döndür...
                res.custid = s_tq.attachedcustomer.customerid;
                res.custname = s_tq.attachedcustomer.customername;
                res.custphone = s_tq.attachedcustomer.phone;
                res.il = s_tq.attachedcustomer.il.ad; // il dictionary olması gerek
                res.ilce = s_tq.attachedcustomer.ilce.ad; // ilce dictionary olması gerek
                res.gsm = s_tq.attachedcustomer.gsm;
                res.s_perid = s_tq.attachedpersonel.personelid;
                res.s_pername = s_tq.attachedpersonel.personelname;
                res.s_perky = s_tq.attachedpersonel.relatedpersonel.personelname;
                res.s_ton = s_tq.taskorderno;
                res.s_tqname = s_tq.task.taskname;
                res.campaign = null; // campaign ve customerproduct dictionary olması gerek
                res.kaynak = s_tq.fault;
                res.s_desc = s_tq.description;
                res.s_year = s_tq.appointmentdate != null ? s_tq.appointmentdate.Value.Year : s_tq.creationdate.Value.Year;
                res.s_month = s_tq.appointmentdate != null ? s_tq.appointmentdate.Value.Month : s_tq.creationdate.Value.Month;
                res.s_day = s_tq.appointmentdate != null ? s_tq.appointmentdate.Value.Day : s_tq.creationdate.Value.Day;
                res.s_consummationdate = s_tq.consummationdate != null ? (DateTime?)s_tq.consummationdate.Value : null;
                res.s_consummationdateyear = s_tq.consummationdate != null ? (int?)s_tq.consummationdate.Value.Year : null;
                res.s_consummationdatemonth = s_tq.consummationdate != null ? (int?)s_tq.consummationdate.Value.Month : null;
                res.s_consummationdateday = s_tq.consummationdate != null ? (int?)s_tq.consummationdate.Value.Day : null;
                res.s_tqstate = s_tq.taskstatepool != null ? s_tq.taskstatepool.taskstate : null;
                res.s_tqstatetype = s_tq.taskstatepool != null ? StateTypeText[s_tq.taskstatepool.statetype.Value] : "Bekleyen";
                res.kr_ton = kr_tq != null ? (int?)kr_tq.taskorderno : null;
                res.kr_tqname = kr_tq != null ? kr_tq.task.taskname : null;
                res.kr_perid = kr_tq != null ? (int?)kr_tq.attachedpersonel.personelid : null;
                res.kr_pername = kr_tq != null ? kr_tq.attachedpersonel.personelname : null;
                res.kr_consummationdate = kr_tq != null ? kr_tq.consummationdate : null;
                res.kr_consummationdateyear = kr_tq != null ? kr_tq.consummationdate != null ? (int?)kr_tq.consummationdate.Value.Year : null : null;
                res.kr_consummationdatemonth = kr_tq != null ? kr_tq.consummationdate != null ? (int?)kr_tq.consummationdate.Value.Month : null : null;
                res.kr_consummationdateday = kr_tq != null ? kr_tq.consummationdate != null ? (int?)kr_tq.consummationdate.Value.Day : null : null;
                res.kr_tqstate = kr_tq != null ? kr_tq.taskstatepool != null ? kr_tq.taskstatepool.taskstate : null : null;
                res.kr_tqstatetype = kr_tq != null ? kr_tq.taskstatepool != null ? StateTypeText[kr_tq.taskstatepool.statetype.Value] : "Bekleyen" : null;
                res.kr_desc = kr_tq != null ? kr_tq.description : null;
                res.k_ton = k_tq != null ? (int?)k_tq.taskorderno : null;
                res.k_tqname = k_tq != null ? k_tq.task.taskname : null;
                res.k_perid = k_tq != null ? (int?)k_tq.attachedpersonel.personelid : null;
                res.k_pername = k_tq != null ? k_tq.attachedpersonel.personelname : null;
                res.k_consummationdate = k_tq != null ? k_tq.consummationdate : null;
                res.k_consummationdateyear = k_tq != null ? k_tq.consummationdate != null ? (int?)k_tq.consummationdate.Value.Year : null : null;
                res.k_consummationdatemonth = k_tq != null ? k_tq.consummationdate != null ? (int?)k_tq.consummationdate.Value.Month : null : null;
                res.k_consummationdateday = k_tq != null ? k_tq.consummationdate != null ? (int?)k_tq.consummationdate.Value.Day : null : null;
                res.k_tqstate = k_tq != null ? k_tq.taskstatepool != null ? k_tq.taskstatepool.taskstate : null : null;
                res.k_tqstatetype = k_tq != null ? k_tq.taskstatepool != null ? StateTypeText[k_tq.taskstatepool.statetype.Value] : "Bekleyen" : null;
                res.k_desc = k_tq != null ? k_tq.description : null;
                res.ktk_ton = ktk_tq != null ? (int?)ktk_tq.taskorderno : null;
                res.ktk_tqname = ktk_tq != null ? ktk_tq.task.taskname : null;
                res.ktk_perid = ktk_tq != null ? (int?)ktk_tq.attachedpersonel.personelid : null;
                res.ktk_pername = ktk_tq != null ? ktk_tq.attachedpersonel.personelname : null;
                res.ktk_consummationdate = ktk_tq != null ? ktk_tq.consummationdate : null;
                res.ktk_consummationdateyear = ktk_tq != null ? ktk_tq.consummationdate != null ? (int?)ktk_tq.consummationdate.Value.Year : null : null;
                res.ktk_consummationdatemonth = ktk_tq != null ? ktk_tq.consummationdate != null ? (int?)ktk_tq.consummationdate.Value.Month : null : null;
                res.ktk_consummationdateday = ktk_tq != null ? ktk_tq.consummationdate != null ? (int?)ktk_tq.consummationdate.Value.Day : null : null;
                res.ktk_tqstate = ktk_tq != null ? ktk_tq.taskstatepool != null ? ktk_tq.taskstatepool.taskstate : null : null;
                res.ktk_tqstatetype = ktk_tq != null ? ktk_tq.taskstatepool != null ? StateTypeText[ktk_tq.taskstatepool.statetype.Value] : "Bekleyen" : null;
                res.ktk_desc = ktk_tq != null ? ktk_tq.description : null;
                res.lastTaskStatus = lasttq.taskstatepool != null ? StateTypeText[lasttq.taskstatepool.statetype.Value] : "Bekleyen";
                res.lastTaskTypeName = lasttq != null ? TaskTypeText[lasttq.task.tasktypes.TaskTypeId] : null;
                res.lastTaskName = lasttq.task.taskname;
                res.sltime = null;
                res.bayisltime = null;
                res.kocslstartdate = null;
                res.kocslenddate = null;
                res.bayislstartdate = null;
                res.bayislenddate = null;
                res.bayislstartdateadd = null;
                res.bayislenddateadd = null;
                res.fark = null;
                return res;
            }), "application/json");
        }
        [Route("Taskqueues")]
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> Taskqueues(DTOs.Adsl.DTORequestClasses.DateTimeRange request)
        {
            await WebApiConfig.updateAdslData();
            var list = WebApiConfig.AdslTaskQueues.Count;
            return Request.CreateResponse(HttpStatusCode.OK, list, "application/json");
        }
    }
}