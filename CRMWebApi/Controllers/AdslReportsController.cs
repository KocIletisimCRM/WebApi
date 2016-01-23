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
               
                var res = db.taskqueue.Include(s=>s.attachedcustomer).Include(c=>c.attachedcustomer.il).Include(c => c.attachedcustomer.il).
                    Include(t=>t.task).Include(d=>d.customerdocument).Include(c=>c.customerproduct).
                    Where(p=>p.attachedpersonelid==request.personelid&& p.deleted==false &&(p.task.taskid==32|| p.task.taskid==33|| p.task.taskid==38 ||
                    p.task.taskid == 39|| p.task.taskid == 40 || p.task.taskid== 59 || p.task.taskid == 60 || p.task.taskid==62 || p.task.taskid == 63 || p.task.taskid == 64
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
    }
}
