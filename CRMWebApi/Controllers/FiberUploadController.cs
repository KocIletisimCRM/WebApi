using CRMWebApi.DTOs.Adsl;
using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Adsl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/Upload")]
 
    public class FiberUploadController : ApiController
    {
        [HttpPost, Route("upload")]
        public HttpResponseMessage saveFiles()
        {
            try
            {
                var request = HttpContext.Current.Request;

                var custid = Convert.ToInt32(request.Form["img_key"].Split('-')[0]);
                var il = request.Form["il"].ToString();
                var ilce = request.Form["ilce"].ToString();
                var tqid = Convert.ToInt32(request.Form["tqid"]);
                var userid = Convert.ToInt32(request.Form["updatedby"]);
                var docid = Convert.ToInt32(request.Form["docid"]);
                string subPath = "E:\\CRMADSLWEB\\Files\\" + il + '\\' + ilce + '\\' + request.Form["img_key"] + "\\";
                System.IO.Directory.CreateDirectory(subPath);
                var filePath = subPath + ((request.Files["file_data"])).FileName;
                using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    (request.Files["file_data"]).InputStream.CopyTo(fs);
                }
                using (var db = new KOCSAMADLSEntities())
                {
                    var cdoc = new adsl_customerdocument
                    {
                        customerid = custid,
                        attachedobjecttype = 3000,
                        taskqueueid = tqid,
                        documentid = docid,
                        documenturl = filePath,
                        receiptdate = DateTime.Now,
                        creationdate = DateTime.Now,
                        lastupdated = DateTime.Now,
                        updatedby = userid,
                        deleted = false
                    };
                    db.customerdocument.Add(cdoc);
                    db.SaveChanges();
                }
                var req = JsonConvert.DeserializeObject<DTOtaskqueue>(request.Form["tq"]);
                return Request.CreateResponse(HttpStatusCode.OK, "uploaded", "application/json"); ;
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "error", "application/json"); ;
            }
        }

        [HttpPost, Route("getUploadedFile")]
        public HttpResponseMessage getUploadedFile()
        {
           //Bu kısım daha sonradan yapılacak.
            return Request.CreateResponse(HttpStatusCode.OK, "uploaded", "application/json"); ;
        }
    }
}
