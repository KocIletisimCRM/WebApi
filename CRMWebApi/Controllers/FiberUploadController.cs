using CRMWebApi.Models.Adsl;
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
        public HttpResponseMessage deleteFile()
        {
            var request = HttpContext.Current.Request;
            string subPath = "C:\\deneme\\" + request.Form["img_key"] + "\\";
            var custid = Convert.ToInt32(request.Form["img_key"].Split('-')[0]);
            var tqid = Convert.ToInt32(request.Form["tqid"]);
            var docid = Convert.ToInt32(request.Form["docid"]);
            System.IO.Directory.CreateDirectory(subPath);
            var filePath = subPath + (request.Files["kartik-input-702[]"]).FileName;
            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                (request.Files["kartik-input-702[]"]).InputStream.CopyTo(fs);
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
                    updatedby = 7,// KOCAuthorizeAttribute.getCurrentUser().userId,
                    deleted = false
                };
                db.customerdocument.Add(cdoc);
                db.SaveChanges();
            }
            return Request.CreateResponse(HttpStatusCode.OK, "uploaded", "application/json"); ;
        }

        [HttpPost, Route("getUploadedFile")]
        public HttpResponseMessage getUploadedFile()
        {
           //Bu kısım daha sonradan yapılacak.
            return Request.CreateResponse(HttpStatusCode.OK, "uploaded", "application/json"); ;
        }
    }
}
