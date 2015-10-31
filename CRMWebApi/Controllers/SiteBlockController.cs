using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs;
using System.Data.SqlClient;
using CRMWebApi.DTOs.DTORequestClasses;
using System.Diagnostics;


namespace CRMWebApi.Controllers
{
   [RoutePrefix("api/SiteBlock")]
   public  class SiteBlockController : ApiController
    {
        [Route("getBlocks")]
        [HttpPost]
        public HttpResponseMessage getBlocks(DTOs.DTORequestClasses.DTOGetBlockFilter request)
        {
            using (var db=new CRMEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
                string querySQL = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSQL = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSQL).First();
                var res = db.block.SqlQuery(querySQL).ToList();
                var siteids = res.Select(s => s.siteid).Distinct().ToList();
                var sites = db.site.Where(s => siteids.Contains(s.siteid)).ToList();

                var personelids = res.Select(s => s.salesrepresentative).Distinct().ToList();
                var personels = db.personel.Where(p => personelids.Contains(p.personelid)).ToList();

                res.ForEach(r=>{
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
        public HttpResponseMessage editBlock(DTOs.DTOblock request)
        {
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
                    dblock.updatedby = 7;
                    dblock.lastupdated = DateTime.Now;
                    dblock.verticalproductionline = request.verticalproductionline;
                    db.SaveChanges();
                    errormessage.errorMessage = "İşlem Başarılı";
                    errormessage.errorCode = 1;
       
                return Request.CreateResponse(HttpStatusCode.OK,errormessage, "application/json");
            }

        }

        [Route("getSites")]
        [HttpPost]
        public HttpResponseMessage getSites(DTOs.DTORequestClasses.DTOGetSiteFilter request)
        {
            using (var db = new CRMEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var filter = request.getFilter();
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
        public HttpResponseMessage editSite(DTOs.DTOsite request)
        {
            using (var db = new CRMEntities())
            {

                var dsite = db.site.Where(r => r.siteid == request.siteid).FirstOrDefault();
                var errormessage = new DTOResponseError();
                dsite.sitename = request.sitename;
                dsite.siteaddress = request.siteaddress;
                dsite.description = request.description;
                dsite.lastupdated = DateTime.Now;
                dsite.updatedby = 7;
                dsite.siteregioncode = request.siteregioncode;
                dsite.region = request.region;
                dsite.siteregioncode = request.siteregioncode;
          
                db.SaveChanges();
                errormessage.errorMessage = "İşlem Başarılı";
                errormessage.errorCode = 1;

                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }

        }
    }
}
