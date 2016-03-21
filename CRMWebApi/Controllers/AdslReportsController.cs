using CRMWebApi.Models.Adsl;
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
          
            using (var db= new KOCSAMADLSEntities(false))
            {
               
                var res = db.taskqueue.Include(s=>s.attachedcustomer).Include(c=>c.attachedcustomer.il).Include(c => c.attachedcustomer.ilce).
                    Include(t=>t.task).Include(d=>d.customerdocument).Include(c=>c.customerproduct).
                    Where(p=>p.attachedpersonelid==request.personelid&& p.deleted==false &&(p.task.taskid==32|| p.task.taskid==33|| p.task.taskid==38 ||
                    p.task.taskid == 39|| p.task.taskid == 40 || p.task.taskid== 59 || p.task.taskid == 60 || p.task.taskid==62 || p.task.taskid == 63 || p.task.taskid == 64 || p.task.taskid == 65 || p.task.taskid == 79 || p.task.taskid == 72 || p.task.taskid == 80 || p.task.taskid == 75 || p.task.taskid == 76 || p.task.taskid == 82 || p.task.taskid == 86 || p.task.taskid == 85 || p.task.taskid == 55 || p.task.taskid == 54 || p.task.taskid == 102
                    || p.task.taskid == 41 || p.task.taskid == 49 || p.task.taskid == 51) && p.status==null).OrderBy(s=>s.attachedcustomer.customername).ToList();
                res.ForEach(r=> {
                    var salestaskorderno = db.taskqueue.Where(t => t.task.tasktype == 1 && t.attachedobjectid==r.attachedobjectid)
                           .OrderByDescending(t => t.taskorderno).Select(t => t.taskorderno).FirstOrDefault();

                    r.customerproduct= db.customerproduct.Include(s => s.campaigns).Where(c => c.taskid == salestaskorderno).ToList();
                    r.description = db.taskqueue.Where(s => s.taskorderno == salestaskorderno).Select(s=>s.description).FirstOrDefault();
                });



                return Request.CreateResponse(HttpStatusCode.OK,res.Select(s=>s.toDTO()), "application/json");
            }
        }
        // Satış kurulum raporu
        [Route("SKR")]
        [HttpPost]
        public HttpResponseMessage SKR (DTOs.Adsl.DTORequestClasses.DateTimeRange request)
        {
            return Request.CreateResponse(WebApiConfig.AdslProccesses.Values.Where(r=> {
                if (!r.Ktk_TON.HasValue) return true;
                var ktk_tq = WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value];
                return ktk_tq.consummationdate >= request.start && ktk_tq.consummationdate <= request.end;
            }).Select(r=> {
                var s_tq = WebApiConfig.AdslTaskQueues[r.S_TON];
                var kr_tq = (r.Kr_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Kr_TON.Value] : null;
                var k_tq = (r.K_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.K_TON.Value] : null;
                var ktk_tq = (r.Ktk_TON.HasValue) ? WebApiConfig.AdslTaskQueues[r.Ktk_TON.Value] : null;
                var taskType = s_tq.task.tasktypes.TaskTypeName;
                var TaskName = s_tq.task.taskname;
                var lasttq = (ktk_tq ?? k_tq ?? kr_tq ?? s_tq);
                var lastTaskName = lasttq.task.taskname;
                var StateTypeText = new string[] { "", "Tamamlanan", "İptal Edilen", "Ertelenen" };
                var lastTaskStatus = lasttq.taskstatepool != null ? StateTypeText[lasttq.taskstatepool.statetype.Value] : "Bekleyen";
                return new
                {
                    //Satış kurulum view sonucuna göre class tanımı yap ve o class türünde bir nesne oluşturup döndür...
                };
            }));
        }
    }
}
