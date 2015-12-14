using CRMWebApi.DTOs.Adsl;
using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Customer")]
    public class AdslCustomerController : ApiController
    {
        [Route("getCustomer")]
        [HttpPost]
        [KOCAuthorize]
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
        [KOCAuthorize]
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
                    item.phone = ct.phone;
                    item.birthdate = ct.birthdate;
                    item.lastupdated = DateTime.Now;
                    item.updatedby = KOCAuthorizeAttribute.getCurrentUser().userId ;

                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "ok", "application/json");
            }
        }
    }
}
