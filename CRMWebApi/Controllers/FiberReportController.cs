using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CRMWebApi.Models.Fiber;
using System.Diagnostics;
using System.Data.SqlClient;
using CRMWebApi.DTOs.Fiber;
using System.Data;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/FiberReport")]
    public class FiberReportController : ApiController
    {
        [HttpGet]
        [Route("test/{ton}")]
        public HttpResponseMessage test(int ton)
        {
            return WebApiConfig.AdslProccesses.ContainsKey(ton) ?
                Request.CreateResponse(new { Error = "", Data = WebApiConfig.AdslProccesses[ton] }) :
                Request.CreateResponse(new { Error = "Kayıt Bulunamadı!" });
        }

        [HttpGet]
        [Route("test2")]
        public HttpResponseMessage test2()
        {
            var m = GC.GetTotalMemory(true);
            var tqs = new Dictionary<int, DTOtaskqueue>(500000);
            using (var db = new CRMEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    conn.Open();
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select * from taskqueue where deleted = 0";
                    var p = Stopwatch.StartNew();
                    using (var sqlreader = selectCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        while (sqlreader.Read())
                        {
                            var t = (new taskqueue
                            {
                                taskorderno = (int)sqlreader[0],
                                taskid = Convert.ToInt32(sqlreader[1]),
                                previoustaskorderid = sqlreader.IsDBNull(2)? null : (int?)sqlreader[2],
                                relatedtaskorderid = sqlreader.IsDBNull(3) ? null : (int?)sqlreader[3],
                                creationdate = (DateTime)sqlreader[4],
                                attachedobjectid = sqlreader.IsDBNull(5) ? null : (int?)sqlreader[5],
                                attachmentdate = sqlreader.IsDBNull(6) ? null : (DateTime?)sqlreader[6],
                                attachedpersonelid = sqlreader.IsDBNull(7)? null : (int?)sqlreader[7],
                                appointmentdate = sqlreader.IsDBNull(8) ? null : (DateTime?)sqlreader[8],
                                status = sqlreader.IsDBNull(9) ? null : (int?)sqlreader[9],
                                consummationdate = sqlreader.IsDBNull(10) ? null : (DateTime?)sqlreader[10],
                                description = sqlreader.IsDBNull(11) ? null : (string)sqlreader[11],
                                lastupdated = (DateTime)sqlreader[12],
                                updatedby = (int)sqlreader[13],
                                deleted = (bool)sqlreader[14],
                                assistant_personel = sqlreader.IsDBNull(15) ? null : (int?)sqlreader[15],
                                fault = sqlreader.IsDBNull(16) ? null : (string)sqlreader[16],
                            }).toDTO<DTOtaskqueue>();
                            tqs.Add(t.taskorderno, t); 
                        }
                    }
                    var mem = GC.GetTotalMemory(true) - m;
                    return Request.CreateResponse(new { Time = p.Elapsed, mem = mem / (1024 * 1024) });
                }
            }
        }
    }
}
