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
    [RoutePrefix("api/Adsl/Personel")]
    [KOCAuthorize]
    public class AdslPersonelController : ApiController
    {
        #region Personel Tanımlama Sayfası
        [Route("getPersonels")]
        [HttpPost]
        public HttpResponseMessage getPersonels(DTOFilterGetPersonelRequest request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.personel.SqlQuery(querySql).ToList();
                var ilIds = res.Select(s => s.ilKimlikNo).Distinct().ToList();
                var ils = db.il.Where(s => ilIds.Contains(s.kimlikNo)).ToList();

                var ilceIds = res.Select(s => s.ilceKimlikNo).Distinct().ToList();
                var ilces = db.ilce.Where(s => ilceIds.Contains(s.kimlikNo)).ToList();

                var relatedPersonelIds = res.Select(s => s.relatedpersonelid).Distinct().ToList();
                var relatedPersonels = db.personel.Where(s => relatedPersonelIds.Contains(s.personelid)).ToList();

                var kurulumPersonelIds = res.Select(s => s.kurulumpersonelid).Distinct().ToList();
                var kurulumPersonels = db.personel.Where(s => kurulumPersonelIds.Contains(s.personelid)).ToList();

                res.ForEach(s=> {
                    s.ilce = ilces.Where(i => i.kimlikNo == s.ilceKimlikNo).FirstOrDefault();
                    s.il = ils.Where(i=>i.kimlikNo==s.ilKimlikNo).FirstOrDefault();
                    s.relatedpersonel =relatedPersonels.Where(p => p.personelid == s.relatedpersonelid).FirstOrDefault();
                    s.kurulumpersonel = kurulumPersonels.Where(p => p.personelid == s.kurulumpersonelid).FirstOrDefault();
                });
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

        [Route("savePersonel")]
        [HttpPost]
        public HttpResponseMessage savePersonel(DTOpersonel request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var dp = db.personel.Where(t => t.personelid == request.personelid).FirstOrDefault();

                dp.personelname = request.personelname;
                dp.category = (int)request.category;
                dp.roles= dp.category = (int)request.category;
                dp.relatedpersonelid = request.relatedpersonelid;
                dp.kurulumpersonelid = request.kurulumpersonelid;
                dp.ilceKimlikNo = request.ilceKimlikNo;
                dp.ilKimlikNo = request.ilKimlikNo;
                dp.mobile = request.mobile;
                dp.email = request.email;
                dp.password = request.password;
                dp.notes = request.notes;
                dp.lastupdated = DateTime.Now;
                dp.updatedby = KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser().userId;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("insertPersonel")]
        [HttpPost]
        public HttpResponseMessage insertPersonel(DTOpersonel request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                var p = new adsl_personel
                {
                    personelname = request.personelname,
                    category = (int)request.category,
                    mobile = request.mobile,
                    email = request.email,
                    password = request.password,
                    notes = request.notes,
                    roles = (int)request.category,
                    relatedpersonelid = request.relatedpersonelid != 0 ? request.relatedpersonelid : null,
                    kurulumpersonelid = request.kurulumpersonelid,
                    ilceKimlikNo=request.ilceKimlikNo,
                    ilKimlikNo=request.ilKimlikNo,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser().userId
                };
                db.personel.Add(p);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion
    }
}
