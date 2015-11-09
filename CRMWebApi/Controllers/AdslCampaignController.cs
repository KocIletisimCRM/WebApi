using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
namespace CRMWebApi.Controllers
{
    public class AdslCampaignController : ApiController
    {
        #region Kampanya Sayfası 
        [Route("getCampaigns")]
        [HttpPost]
        public HttpResponseMessage getCampaigns(DTOFiterGetCampaignRequst request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.campaigns.SqlQuery(querySql).ToList();
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

        [Route("saveCampaigns")]
        [HttpPost]
        public HttpResponseMessage saveCampaigns(DTOcampaigns request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var dcamp = db.campaigns.Where(t => t.id == request.id).FirstOrDefault();

                dcamp.name = request.name;
                dcamp.category = request.category;
                dcamp.subcategory = request.subcategory;
                dcamp.products = request.products;
                dcamp.documents = request.documents;
                dcamp.lastupdated = DateTime.Now;
                dcamp.updatedby = 7;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertCampaigns")]
        [HttpPost]
        public HttpResponseMessage insertCampaigns(DTOcampaigns request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var c = new adsl_campaigns
                {
                    name = request.name,
                    category = request.category,
                    subcategory = request.subcategory,
                    products = request.products,
                    documents = request.documents,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = 7
                };
                db.campaigns.Add(c);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion
    }
}
