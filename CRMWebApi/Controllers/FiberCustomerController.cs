using CRMWebApi.DTOs.Fiber;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using CRMWebApi.KOCAuthorization;
using CRMWebApi.Models.Fiber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/Customer")]
    public class FiberCustomerController : ApiController
    {
        [Route("getCustomers")]
        [HttpPost]
        public HttpResponseMessage getCustomers(DTOFilterGetCustomerRequest request)
        {
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                //filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                var countSql = filter.getCountSQL();
                var querySql = filter.getFilterSQL();
                var res = db.customer.SqlQuery(querySql).ToList();

                var blockIds = res.Select(b => b.blockid).Distinct().ToList();
                var blocks = db.block.Where(b => blockIds.Contains(b.blockid)).ToList();

                var issIds = res.Where(i => i.iss != null).Select(i => i.iss).Distinct().ToList();
                var isss = db.issStatus.Where(i => issIds.Contains(i.id)).ToList();

                var cststatusIds = res.Where(c => c.customerstatus != null).Select(c => c.customerstatus).Distinct().ToList();
                var cststatus = db.customer_status.Where(c => cststatusIds.Contains(c.ID)).ToList();

                var netstatusIds = res.Where(c => c.netstatu != null).Select(c => c.netstatu).Distinct().ToList();
                var netstatus = db.netStatus.Where(c => netstatusIds.Contains(c.id)).ToList();

                var tvStatusIds = res.Where(c => c.tvstatu != null).Select(c => c.tvstatu).Distinct().ToList();
                var tvstatus = db.TvKullanımıStatus.Where(c => tvStatusIds.Contains(c.id)).ToList();

                var telStatusIds = res.Where(c => c.telstatu != null).Select(c => c.telstatu).Distinct().ToList();
                var telstatus = db.telStatus.Where(c => telStatusIds.Contains(c.id)).ToList();

                var ttvStatusIds = res.Where(c => c.turkcellTv != null).Select(c => c.turkcellTv).Distinct().ToList();
                var ttvstatus = db.TurkcellTVStatus.Where(c => ttvStatusIds.Contains(c.id)).ToList();

                var gsmStatusIds = res.Where(c => c.gsmstatu != null).Select(c => c.gsmstatu).Distinct().ToList();
                var gsmstatus = db.gsmKullanımıStatus.Where(c => gsmStatusIds.Contains(c.id)).ToList();

                res.ForEach(r =>
                {
                    r.block = blocks.Where(b => b.blockid == r.blockid).FirstOrDefault();
                    r.issStatus = isss.Where(b => b.id == r.iss).FirstOrDefault();
                    r.customer_status = cststatus.Where(b => b.ID == r.customerstatus).FirstOrDefault();
                    r.netStatus = netstatus.Where(b => b.id == r.netstatu).FirstOrDefault();
                    r.telStatus = telstatus.Where(b => b.id == r.telstatu).FirstOrDefault();
                    r.TvKullanımıStatus = tvstatus.Where(b => b.id == r.tvstatu).FirstOrDefault();
                    r.TurkcellTVStatus = ttvstatus.Where(b => b.id == r.turkcellTv).FirstOrDefault();
                    r.gsmKullanımıStatus = gsmstatus.Where(b => b.id == r.gsmstatu).FirstOrDefault();
                });

                return Request.CreateResponse(HttpStatusCode.OK,
                new DTOResponse(DTOResponseError.NoError(), res.Where(r => r.deleted != true).OrderByDescending(r => r.deleted).Select(r => r.toDTO()).ToList())
                , "application/json");
            }
        }

        [Route("insertCustomer")]
        [HttpPost]
        public HttpResponseMessage insertCustomer(DTOcustomer request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                    var p = new customer
                    {
                        blockid = request.block.blockid,
                        tckimlikno = request.tckimlikno,
                        customername = request.customername,
                        flat = request.flat,
                        gsm = request.gsm,
                        phone = request.phone,
                        birthdate = request.birthdate,
                        creationdate = DateTime.Now,
                        lastupdated = DateTime.Now,
                        updatedby = user.userId,
                        deleted = false,
                        customerstatus = request.customer_status.ID,
                        telstatu = request.telStatus.id,
                        tvstatu = request.TvKullanımıStatus.id,
                        turkcellTv = request.TurkcellTVStatus.id,
                        netstatu = request.netStatus.id,
                        description = request.description,
                        gsmstatu = request.gsmKullanımıStatus.id,
                        iss = request.issStatus.id,
                        emptorcustomernum = request.emptorcustomernum,
                    };
                    db.customer.Add(p);
                    db.SaveChanges();
                    tran.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "Hata Oluştu" };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
        }

        [Route("pasifCustomer")]
        [HttpPost]
        public HttpResponseMessage pasifCustomer(int customerid)
        {
            using (var db = new CRMEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var upa = db.customer.Where(r => r.customerid == customerid).FirstOrDefault();
                    upa.deleted = null;
                    db.SaveChanges();
                    tran.Commit();
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "Müşteri Düzenleme Tamamlandı." };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "Müşteri Düzenleme Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                }
        }
    }
}
