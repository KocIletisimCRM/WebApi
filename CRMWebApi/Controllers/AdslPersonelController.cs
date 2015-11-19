using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.Models.Adsl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Personel")]
    public class AdslPersonelController : ApiController
    {
        #region Personel Tanımlama Sayfası

        [Route("getPersonels")]
        [HttpPost]
        //[KOCAuthorize]
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
                dp.roles= dp.category = (int)request.category; ;
                dp.mobile = request.mobile;
                dp.email = request.email;
                dp.password = request.password;
                dp.notes = request.notes;
                dp.lastupdated = DateTime.Now;
                dp.updatedby = 7;
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
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = 7
                };
                db.personel.Add(p);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
        #endregion
    }
}
