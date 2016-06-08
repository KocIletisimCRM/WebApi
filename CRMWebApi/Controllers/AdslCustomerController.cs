using CRMWebApi.DTOs.Adsl;
using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Adsl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Customer")]
    [KOCAuthorize]
    public class AdslCustomerController : ApiController
    {
        [Route("getCustomer")]
        [HttpPost]
        public HttpResponseMessage getCustomer(DTOs.Adsl.DTORequestClasses.DTOGetCustomerRequest request)
        {
            using (var db = new KOCSAMADLSEntities(false))
            {
                var filter = request.getFilter();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countsql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countsql).First();
                var res = db.customer.SqlQuery(querySql).ToList();

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

        [Route("saveCustomerCard")]
        [HttpPost]
        public HttpResponseMessage saveCustomerCard(DTOcustomer ct)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                if (db.customer.Any(c => c.customerid == ct.customerid))
                {
                    var item = db.customer.Where(c => c.customerid == ct.customerid).First();
                    item.customername = ct.customername;
                    item.gsm = ct.gsm;
                    item.tc = ct.tc;
                    item.ilKimlikNo = ct.ilKimlikNo;
                    item.ilceKimlikNo = ct.ilceKimlikNo;
                    item.bucakKimlikNo = ct.bucakKimlikNo;
                    item.mahalleKimlikNo = ct.mahalleKimlikNo;
                    item.phone = ct.phone;
                    item.birthdate = ct.birthdate;
                    item.lastupdated = DateTime.Now;
                    item.updatedby = KOCAuthorizeAttribute.getCurrentUser().userId;
                    item.email = ct.email;
                    item.superonlineCustNo = ct.superonlineCustNo;
                    item.description = ct.description;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            }
        }

        [Route("insertCustomer")]
        [HttpPost]
        public HttpResponseMessage insertCustomer(customer ct)
        {
            using (var db = new KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var item = new customer();
                    item.customername = ct.customername.ToUpper();
                    item.superonlineCustNo = ct.superonlineCustNo;
                    item.gsm = ct.gsm;
                    item.tc = ct.tc;
                    item.phone = ct.phone;
                    item.birthdate = ct.birthdate;
                    item.ilKimlikNo = ct.ilKimlikNo;
                    item.ilceKimlikNo = ct.ilceKimlikNo;
                    item.bucakKimlikNo = ct.bucakKimlikNo;
                    item.mahalleKimlikNo = ct.mahalleKimlikNo;
                    item.yolKimlikNo = 61;
                    item.binaKimlikNo = 61;
                    item.daire = 61;
                    item.creationdate = DateTime.Now;
                    item.lastupdated = DateTime.Now;
                    item.updatedby = KOCAuthorizeAttribute.getCurrentUser().userId;
                    item.email = ct.email;
                    item.deleted = false;
                    item.description = ct.description;
                    db.customer.Add(item);
                    db.SaveChanges();

                    tran.Commit();
                    var customer = db.customer.First(r => r.tc == ct.tc);
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Tamamlandı." };
                    return Request.CreateResponse(HttpStatusCode.OK, new { errormessage, customer }, "application/json");
                }
                catch (Exception)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "İşlem Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, new { errormessage }, "application/json");
                }
        }
    }
}
