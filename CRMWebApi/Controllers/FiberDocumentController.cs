using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CRMWebApi.DTOs.Fiber;
using CRMWebApi.Models.Fiber;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using CRMWebApi.KOCAuthorization;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/Document")]
    public class FiberDocumentController : ApiController
    {
        #region Document sayfası için
        [Route("getDocuments")]
        [HttpPost]
        public HttpResponseMessage getDocuments(DTOGetDocumentFilter request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.document.SqlQuery(querySql).ToList();

                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo)
                    , "application/json");
            }
        }

        [Route("saveDocument")]
        [HttpPost]
        public HttpResponseMessage saveDocument(DTOdocument request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var ddoc = db.document.Where(d => d.documentid == request.documentid).FirstOrDefault();
                ddoc.documentname = request.documentname;
                ddoc.documentdescription = request.documentdescription;
                ddoc.lastupdated = DateTime.Now;
                ddoc.updatedby = user.userId;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, DTOResponseError.NoError(), "application/json");
            }
        }

        [Route("insertDocument")]
        [HttpPost]
        public HttpResponseMessage insertDocument(DTOdocument request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var d = new document
                {
                    documentname = request.documentname,
                    documentdescription = request.documentdescription,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = user.userId
                };
                db.document.Add(d);
                db.SaveChanges();
                DTOResponseError errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion

    }
}
