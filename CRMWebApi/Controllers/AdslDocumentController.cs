using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Document")]
    public class AdslDocumentController : ApiController
    {
        #region Document sayfası için
        [Route("getDocuments")]
        [HttpPost]
        public HttpResponseMessage getDocuments(DTOGetDocumentFilter request)
        {
            using (var db = new KOCSAMADLSEntities())
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
            using (var db = new KOCSAMADLSEntities())
            {
                var ddoc = db.document.Where(d => d.documentid == request.documentid).FirstOrDefault();
                ddoc.documentname = request.documentname;
                ddoc.documentdescription = request.documentdescription;
                ddoc.lastupdated = DateTime.Now;
                ddoc.updatedby = KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser().userId;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, DTOResponseError.NoError(), "application/json");
            }
        }

        [Route("insertDocument")]
        [HttpPost]
        public HttpResponseMessage insertDocument(DTOdocument request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var d = new adsl_document
                {
                    documentname = request.documentname,
                    documentdescription = request.documentdescription,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser().userId
                };
                db.document.Add(d);
                db.SaveChanges();
                DTOResponseError errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("getDocumentIds")]
        [HttpPost]
        public HttpResponseMessage getDocumentIds(DTOGetDocumentIdsRequest request)
        {
            using (var db=new KOCSAMADLSEntities())
            {
                //taska ve durumuna bağlı müşteri dökümanları
                var documentids = db.taskstatematches.Where(tsm => tsm.taskid == request.taskid && tsm.stateid == request.taskstate && tsm.documents != null && tsm.deleted==false).ToList()
                .SelectMany(s => s.documents.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(ss => Convert.ToInt32(ss))).ToList();
                if (request.campaignid != null)
                {
                    var campaignDocs = db.campaigns.Where(c => c.id == request.campaignid && c.documents != null && c.deleted == false).ToList()
                      .SelectMany(s => s.documents.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(ss => Convert.ToInt32(ss))).ToList();
                    documentids.AddRange(campaignDocs);

                }
                if (request.productIds!=null)
                {
                var productDocs = db.product_service.Where(p => request.productIds.Contains(p.productid) && p.documents != null && p.deleted == false).ToList()
                    .SelectMany(s => s.documents.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(ss => Convert.ToInt32(ss))).ToList();
                documentids.AddRange(productDocs);
                }
                var res = db.document.Where(d => documentids.Distinct().Contains(d.documentid) && d.deleted == false).ToList();

                return Request.CreateResponse(HttpStatusCode.OK,res.Select(s=>s.toDTO()).ToList(),"application/json");
            }

        }
        #endregion
    }
}
