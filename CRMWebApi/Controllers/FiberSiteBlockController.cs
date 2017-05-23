using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CRMWebApi.DTOs.Fiber;
using CRMWebApi.Models.Fiber;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using CRMWebApi.KOCAuthorization;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/SiteBlock")]
    public class FiberSiteBlockController : ApiController
    {
        [Route("getBlocks")]
        [HttpPost]
        public HttpResponseMessage getBlocks(DTOGetBlockFilter request)
        {
            using (var db = new CRMEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                string querySQL = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSQL = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSQL).First();
                var res = db.block.SqlQuery(querySQL).ToList();
                var siteids = res.Select(s => s.siteid).Distinct().ToList();
                var sites = db.site.Where(s => siteids.Contains(s.siteid)).ToList();

                var personelids = res.Select(s => s.salesrepresentative).Distinct().ToList();
                var personels = db.personel.Where(p => personelids.Contains(p.personelid)).ToList();

                res.ForEach(r =>
                {
                    r.site = sites.Where(s => s.siteid == r.siteid).FirstOrDefault();
                    r.salespersonel = personels.Where(p => p.personelid == r.salesrepresentative).FirstOrDefault();
                });
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                return Request.CreateResponse(HttpStatusCode.OK, new DTOPagedResponse(DTOResponseError.NoError(), res.Select(s => s.toDTO()).ToList(), paginginfo, querySQL), "application/json");
            }

        }

        [Route("editBlock")]
        [HttpPost]
        public HttpResponseMessage editBlock(DTOblock request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var dblock = db.block.Where(r => r.blockid == request.blockid).FirstOrDefault();
                var errormessage = new DTOResponseError();

                dblock.blockname = request.blockname;
                dblock.binakodu = request.binakodu;
                dblock.cocierge = request.cocierge;
                dblock.cociergecontact = request.cociergecontact;
                dblock.groupid = request.groupid;
                dblock.hp = request.hp;
                dblock.kocsaledate = request.kocsaledate;
                dblock.locationid = request.locationid;
                dblock.objid = request.objid;
                dblock.projectno = request.projectno;
                dblock.readytosaledate = request.readytosaledate;
                // dblock.salesrepresentative = request.salespersonel.personelid;
                // dblock.siteid = request.site.siteid;
                dblock.sosaledate = request.sosaledate;
                dblock.superintendent = request.superintendent;
                dblock.superintendentcontact = request.superintendentcontact;
                dblock.telocadia = request.telocadia;
                dblock.updatedby = user.userId;
                dblock.lastupdated = DateTime.Now;
                dblock.verticalproductionline = request.verticalproductionline;
                db.SaveChanges();
                errormessage.errorMessage = "İşlem Başarılı";
                errormessage.errorCode = 1;

                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("multiEditBlock"), HttpPost]
        public HttpResponseMessage multiEditBlock(DTOMultiEditBlockDate request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                try
                {
                    var dblocks = db.block.Where(r => request.BIDS.Contains(r.blockid)).ToList();

                    foreach (var b in dblocks)
                    {
                        b.updatedby = user.userId;
                        b.lastupdated = DateTime.Now;
                        b.sosaledate = request.FSD;
                        if (request.SSC) b.readytosaledate = request.SOSR;
                        if (request.KSC) b.kocsaledate = request.KSR;
                    }
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, "Tamamlandı", "application/json");
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Tamamlanmadı", "application/json");
                }
            }
        }

        [Route("insertBlock")]
        [HttpPost]
        public HttpResponseMessage insertBlock(DTOblock requst)
        {
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError();
                var user = KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser();
                var block = new block
                {
                    blockname = requst.blockname,
                    siteid = requst.site.siteid,
                    hp = requst.hp,
                    telocadia = requst.telocadia,
                    projectno = requst.projectno,
                    readytosaledate = requst.readytosaledate,
                    sosaledate = requst.sosaledate,
                    kocsaledate = requst.kocsaledate,
                    salesrepresentative = requst.salespersonel.personelid,
                    superintendent = requst.superintendent,
                    superintendentcontact = requst.superintendentcontact,
                    cocierge = requst.cocierge,
                    cociergecontact = requst.cociergecontact,
                    verticalproductionline = requst.verticalproductionline,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    updatedby = user.userId,
                    deleted = false,
                    binakodu = requst.binakodu,
                    locationid = requst.locationid,
                    objid = requst.objid

                };
                db.block.Add(block);
                for (int i = 0; i < (block.hp ?? 0); i++)
                {
                    var hp = new customer
                    {
                        blockid = block.blockid,
                        creationdate = DateTime.Now,
                        lastupdated = DateTime.Now,
                        updatedby = user.userId,
                        deleted = false,
                        flat = (i + 1).ToString()
                    };
                    db.customer.Add(hp);
                }
                db.SaveChanges();
                errormessage.errorMessage = "İşlem Başarılı";
                errormessage.errorCode = 1;

                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("getSites")]
        [HttpPost]
        public HttpResponseMessage getSites(DTOGetSiteFilter request)
        {
            using (var db = new CRMEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "deleted", value = 0, op = 2 });
                string querySQL = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSQL = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSQL).First();
                var res = db.site.SqlQuery(querySQL).ToList();

                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                return Request.CreateResponse(HttpStatusCode.OK, new DTOPagedResponse(DTOResponseError.NoError(), res.Select(s => s.toDTO()).ToList(), paginginfo, querySQL), "application/json");
            }

        }

        [Route("editSite")]
        [HttpPost]
        public HttpResponseMessage editSite(DTOsite request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {

                var dsite = db.site.Where(r => r.siteid == request.siteid).FirstOrDefault();
                var errormessage = new DTOResponseError();
                dsite.sitename = request.sitename;
                dsite.siteaddress = request.siteaddress;
                dsite.sitedistrict = request.sitedistrict;
                dsite.description = request.description;
                dsite.lastupdated = DateTime.Now;
                dsite.updatedby = user.userId;
                dsite.siteregioncode = request.siteregioncode;
                dsite.region = request.region;
                dsite.siteregioncode = request.siteregioncode;

                db.SaveChanges();
                errormessage.errorMessage = "İşlem Başarılı";
                errormessage.errorCode = 1;

                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }

        }

        [Route("insertSite")]
        [HttpPost]
        public HttpResponseMessage insertSite(DTOsite request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            {
                var errormessage = new DTOResponseError();
                var dsite = new site
                {
                    sitename = request.sitename,
                    sitedistrict = request.sitedistrict,
                    siteaddress = request.siteaddress,
                    description = request.description,
                    creationdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    deleted = false,
                    updatedby = user.userId,
                    siteregioncode = request.siteregioncode,
                    region = request.region
                };
                db.site.Add(dsite);
                db.SaveChanges();
                errormessage.errorMessage = "İşlem Başarılı";
                errormessage.errorCode = 1;

                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }

        }

        [Route("editSiteMultiple")]
        [HttpPost]
        public HttpResponseMessage editSiteMultiple(DTORequestSBEditMultiple request)
        {
            if ((request.siteIds == null && request.blockIds == null) || request.personelid == 0)
                return Request.CreateResponse(HttpStatusCode.OK, "Seçimleri Yapınız !", "application/json");
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new CRMEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    if (request.blockIds != null && request.blockIds.Count > 0)
                    {
                        var k = db.block.Where(r => request.blockIds.Contains(r.blockid)).ToList();
                        k.ForEach(r =>
                        {
                            r.salesrepresentative = request.personelid;
                            r.lastupdated = DateTime.Now;
                            r.updatedby = user.userId;
                        });
                    }
                    else
                    {
                        var k = db.block.Where(r => request.siteIds.Contains(r.siteid)).ToList();
                        k.ForEach(r =>
                        {
                            r.salesrepresentative = request.personelid;
                            r.lastupdated = DateTime.Now;
                            r.updatedby = user.userId;
                        });
                    }

                    db.SaveChanges();
                    tran.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "Tamamlandı", "application/json");
                }
                catch (Exception)
                {
                    tran.Rollback();
                    return Request.CreateResponse(HttpStatusCode.OK, "Tamamlanmadı", "application/json");
                }
            }
        }
    }
}
