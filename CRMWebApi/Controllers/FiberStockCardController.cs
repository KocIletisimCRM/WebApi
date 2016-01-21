using CRMWebApi.DTOs.Fiber;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using CRMWebApi.Models.Fiber;
using CRMWebApi.KOCAuthorization;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/Stockcard")]
    public class FiberStockCardController : ApiController
    {
        #region Depo Kart Tanımlamaları Sayfası

        [Route("getStockCards")]
        [HttpPost]
        public HttpResponseMessage getStockCads(DTOFilterGetStockCardRequest request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.stockcard.SqlQuery(querySql).ToList();
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

        [Route("saveStockCard")]
        [HttpPost]
        public HttpResponseMessage saveStockCard(DTOstockcard request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var ds = db.stockcard.Where(t => t.stockid == request.stockid).FirstOrDefault();

                ds.productname = request.productname;
                ds.category = request.category;
                ds.hasserial = request.hasserial;
                ds.unit = request.unit;
                ds.description = request.description;
                ds.lastupdated = DateTime.Now;
                ds.updatedby = user.userId;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }


        [Route("insertStockCard")]
        [HttpPost]
        public HttpResponseMessage insertStockCard(DTOstockcard request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var s = new stockcard
                {
                    productname = request.productname,
                    category = request.category,
                    hasserial = request.hasserial,
                    unit = request.unit,
                    description = request.description,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = user.userId
                };
                db.stockcard.Add(s);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion

    }
}
