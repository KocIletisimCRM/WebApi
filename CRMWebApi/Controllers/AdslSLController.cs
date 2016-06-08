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
    [RoutePrefix("api/Adsl/SL")]
    [KOCAuthorize]
    public class AdslSLController : ApiController
    {
        [Route("getSL")]
        [HttpPost]
        public HttpResponseMessage getSL(DTOFilterGetSLRequest request)
        { // SL hesaplama sayfasının doldurulması (Hüseyin KOZ)
            using (var db = new KOCSAMADLSEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.SL.SqlQuery(querySql).ToList();

                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Select(r => r.toDTO()).ToList(), paginginfo, querySql)
                    , "application/json"); 
            }
        }
        [Route("insertSL")]
        [HttpPost]
        public HttpResponseMessage insertSL(DTOSL request)
        { // SL hesaplamaları için gerekli olan bilgilerin kaydı (Hüseyin KOZ)
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                    var p = new SL
                    {
                        SLName = request.SLName,
                        KocSTask = request.KocSTask,
                        KocETask = request.KocETask,
                        KocMaxTime = request.KocMaxTime,
                        BayiSTask = request.BayiSTask,
                        BayiETask = request.BayiETask,
                        BayiMaxTime = request.BayiMaxTime,
                        lastupdated = DateTime.Now,
                        updatedby = user.userId,
                        deleted = false,
                    };
                    db.SL.Add(p);
                    db.SaveChanges();
                    tran.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "Hata Oluştu" };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
        }
        [Route("updateSL")]
        [HttpPost]
        public HttpResponseMessage updateSL(DTOSL request)
        { // SL hesaplamaları için gerekli olan bilgilerin güncellenmesi (Hüseyin KOZ)
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var upa = db.SL.Where(r => r.SLID == request.SLID).FirstOrDefault();
                    upa.SLName = request.SLName;
                    upa.KocSTask = request.KocSTask;
                    upa.KocETask = request.KocETask;
                    upa.KocMaxTime = request.KocMaxTime;
                    upa.BayiSTask = request.BayiSTask;
                    upa.BayiETask = request.BayiETask;
                    upa.BayiMaxTime = request.BayiMaxTime;
                    upa.lastupdated = DateTime.Now;
                    upa.updatedby = user.userId;
                    db.SaveChanges();
                    tran.Commit();
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "SL Düzenleme Tamamlandı." };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "SL Düzenleme Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                }
        }
        [Route("deleteSL")]
        [HttpPost]
        public HttpResponseMessage deleteSL(int SLID)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var upa = db.SL.Where(r => r.SLID == SLID).FirstOrDefault();
                    //upa.deleted = true; //delete işlemi sayfadan yapılacaksa aktif edilip işlem burdan sağlanacak (Hüseyin KOZ)
                    upa.lastupdated = DateTime.Now;
                    upa.updatedby = user.userId;
                    db.SaveChanges();
                    tran.Commit();
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "işlem Gerçekleştirildi." };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "işlem Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                }
        }

    }
}
