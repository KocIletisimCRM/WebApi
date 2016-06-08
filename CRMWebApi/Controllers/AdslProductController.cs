using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Adsl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Product")]
    [KOCAuthorize]
    public class AdslProductController : ApiController
    {
        #region Depo Kart Tanımlamaları Sayfası

        [Route("getProducts")]
        [HttpPost]
        public HttpResponseMessage getProducts(DTOGetProductFilter request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.product_service.SqlQuery(querySql).ToList();
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };

                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(r => r.toDTO()).ToList(), paginginfo, querySql)
                    , "application/json");
            }
        }

        [Route("saveProduct")]
        [HttpPost]
        public HttpResponseMessage saveProduct(DTOProduct_service request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var dpro = db.product_service.Where(t => t.productid == request.productid).FirstOrDefault();

                dpro.productname = request.productname;
                dpro.category = request.category;
                dpro.maxduration = request.maxduration;
                dpro.automandatorytasks = request.automandatorytasks;
                dpro.documents = request.documents;
                dpro.lastupdated = DateTime.Now;
                dpro.updatedby = KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser().userId;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertProduct")]
        [HttpPost]
        public HttpResponseMessage insertProduct(DTOProduct_service request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var p = new adsl_product_service
                {
                    productname = request.productname,
                    category = request.category,
                    automandatorytasks = request.automandatorytasks,
                    maxduration = request.maxduration,
                    documents=request.documents,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser().userId,

                };
                db.product_service.Add(p);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion
    }
}
