﻿using CRMWebApi.DTOs.Fiber;
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

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/Stock")]
    public class StockController : ApiController
    {

        #region Stok Hareketleri Sayfası
        [Route("getStockMovements")]
        [HttpPost]
        public HttpResponseMessage getStockMovements(DTOGetStockMovementRequest request)
        {
            var userID = 12;
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSql = filter.getCountSQL();
                if (querySql.Contains("fromobject") || querySql.Contains("toobject"))
                {
                    var sqlPartitions = querySql.Split(new string[] { "paging as (", ") SELECT *" }, StringSplitOptions.RemoveEmptyEntries);
                    var pagingWhereClauses = sqlPartitions[1].Split(new string[] { "stockmovement WHERE", ") AND" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    var subTablesClauses = pagingWhereClauses.Where(sc => sc.Contains("stockmovement.fromobject") || sc.Contains("stockmovement.toobject")).ToList();
                    var newClauses = new Dictionary<int, List<string>>();
                    newClauses[0] = new List<string>();
                    newClauses[1] = new List<string>();
                    for (int i = 0; i < subTablesClauses.Count(); i++)
                    {
                        var clause = subTablesClauses[i];
                        if (clause.Contains("fromobject1"))
                            newClauses[0].Add(clause.Replace("))", ")").Replace("stockmovement.fromobject1", "stockmovement.fromobject") + " AND fromobjecttype = 3000)");
                        else if (clause.Contains("fromobject2"))
                            newClauses[0].Add(clause.Replace("))", ")").Replace("stockmovement.fromobject2", "stockmovement.fromobject") + " AND fromobjecttype = 2000)");
                        else if (clause.Contains("fromobject3"))
                            newClauses[0].Add(clause.Replace("))", ")").Replace("stockmovement.fromobject3", "stockmovement.fromobject") + " AND fromobjecttype = 1000)");
                        else if (clause.Contains("toobject1"))
                            newClauses[1].Add(clause.Replace("))", ")").Replace("stockmovement.toobject1", "stockmovement.toobject") + " AND toobjecttype = 3000)");
                        else if (clause.Contains("toobject2"))
                            newClauses[1].Add(clause.Replace("))", ")").Replace("stockmovement.toobject2", "stockmovement.toobject") + " AND toobjecttype = 2000)");
                        else if (clause.Contains("toobject3"))
                            newClauses[1].Add(clause.Replace("))", ")").Replace("stockmovement.toobject3", "stockmovement.toobject") + " AND toobjecttype = 1000)");
                        else
                            newClauses[clause.Contains("fromobject") ? 0 : 1].Add(clause + ")");
                        pagingWhereClauses.Remove(clause);
                    }
                    var whereClauses = new List<string>();

                    if (pagingWhereClauses.Skip(1).Any()) whereClauses.Add(string.Join(") AND", pagingWhereClauses.Skip(1)) + ")");
                    if (newClauses[0].Any()) whereClauses.Add($"({string.Join(" OR ", newClauses[0])})");
                    if (newClauses[1].Any()) whereClauses.Add($"({string.Join(" OR ", newClauses[1])})");
                    whereClauses.Add($"(fromobject = {userID} or toobject = {userID})");
                    var whereClause = string.Join(" AND ", whereClauses);
                    querySql = $"{sqlPartitions[0]}paging as ({pagingWhereClauses[0]}stockmovement WHERE{whereClause}) SELECT *{sqlPartitions[2]} ";

                    countSql = $"{sqlPartitions[0]}paging as ({pagingWhereClauses[0]}stockmovement WHERE{whereClause}) SELECT COUNT(*) FROM _paging";
                }

                var performance = new DTOQueryPerformance();
                var perf = Stopwatch.StartNew();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                performance.CountSQLDuration = perf.Elapsed;
                perf.Restart();
                var res = db.stockmovement.SqlQuery(querySql).ToList();
                performance.QuerSQLyDuration = perf.Elapsed;
                perf.Restart();
                var fromObjectIds = res.Select(s => s.fromobject).Distinct().ToList();
                var fromPersonels = db.personel.Where(p => fromObjectIds.Contains(p.personelid)).ToList();
                var fromCustomers = db.customer.Where(c => fromObjectIds.Contains(c.customerid)).ToList();
                var fromSites = db.site.Where(s => fromObjectIds.Contains(s.siteid)).ToList();
                var fromBlocks = db.block.Where(b => fromObjectIds.Contains(b.blockid)).ToList();

                var toObjectIds = res.Select(s => s.toobject).Distinct().ToList();
                var toPersonels = db.personel.Where(p => toObjectIds.Contains(p.personelid)).ToList();
                var toCustomers = db.customer.Where(c => toObjectIds.Contains(c.customerid)).ToList();
                var toSites = db.site.Where(s => toObjectIds.Contains(s.siteid)).ToList();
                var toBlocks = db.block.Where(b => toObjectIds.Contains(b.blockid)).ToList();


                var stockcardids = res.Select(s => s.stockcardid).Distinct().ToList();
                var stockcards = db.stockcard.Where(s => stockcardids.Contains(s.stockid)).ToList();

                res.ForEach(r => {
                    r.frompersonel = fromPersonels.Where(p => p.personelid == r.fromobject).FirstOrDefault();
                    if (r.frompersonel == null)
                    {
                        r.fromcustomer = fromCustomers.Where(c => c.customerid == r.fromobject).FirstOrDefault();
                    }
                    r.topersonel = toPersonels.Where(p => p.personelid == r.toobject).FirstOrDefault();
                    if (r.topersonel == null)
                    {
                        r.tocustomer = toCustomers.Where(c => c.customerid == r.toobject).FirstOrDefault();
                    }
                    r.stockcard = stockcards.Where(s => s.stockid == r.stockcardid).FirstOrDefault();

                });
                performance.LookupDuration = perf.Elapsed;
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                return Request.CreateResponse(HttpStatusCode.OK,
                     new DTOPagedResponse(DTOResponseError.NoError(), res.Where(r => r.deleted == false).Select(s => s.toDTO()).ToList(), paginginfo, querySql, performance)
                     , "application/json");
            }
        }

        [Route("SaveStockMovementMultiple")]
        [HttpPost]
        public HttpResponseMessage SaveStockMovementMultiple(stockmovement[] sms)
        {
            foreach (var sm in sms)
            {
                if (sm.movementid == 0)
                {
                    if (sm.amount > 0 || sm.serialno != null)
                        InsertStockMovement(sm, sm.relatedtaskqueue, sm.serialno ?? "");
                }
                else
                {
                    if (sm.amount == 0)
                    {
                        //silinecek
                    }
                    else
                        SaveStockMovement(sm, sm.relatedtaskqueue, sm.serialno ?? "");
                }
            }
            return null;
        }

        [Route("InsertStockMovement")]
        [HttpPost]
        public HttpResponseMessage InsertStockMovement(stockmovement r, int? tqid, string serinos)
        {
            //  var serinos = Request.Params.AllKeys;
            var serials = serinos.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var errormessage = new DTOResponseError();
            if (!serials.Any()) serials.Add(null);
            if (ModelState.IsValid)
            {
                var userID = 12;//depocu
                var userType = 2;
                using (var db = new CRMEntities())
                {
                    foreach (var seri in serials)
                    {
                        //serino kontrolü yap. varsa ekleme.
                        var userControl = db.stockmovement.Where(s => s.serialno==seri).Select(s => s.fromobject).FirstOrDefault();
                        if (userControl != userID)
                        {
                            errormessage.errorCode = -1;
                            errormessage.errorMessage = "Yalnızca Kendinize Ait Ürünleri Başkasına Çıkabilirsiniz";
                        }
                        else
                        {
                            var count = db.stockmovement.Where(s => s.serialno == seri).Count();
                            if ((int)count >= 0)
                            {
                                errormessage.errorCode = -1;
                                errormessage.errorMessage = seri + " Seri numarası daha önce girilmiş! Lütfen Kontrol Ediniz!";
                            }
                            else
                            {


                                stockmovement sm = new stockmovement();
                                sm.serialno = seri;
                                sm.lastupdated = DateTime.Now;
                                sm.creationdate = DateTime.Now;
                                sm.toobjecttype = r.toobjecttype;
                                sm.stockcardid = r.stockcardid;
                                sm.toobject = r.toobject;
                                sm.deleted = false;
                                sm.amount = seri == null ? r.amount : 1;
                                sm.relatedtaskqueue = tqid;
                                if (userID == userID)// (long)KocCRMRoles.kscrStockStaff
                                {
                                    if (r.toobjecttype == 5000)
                                    {

                                        sm.fromobjecttype = 4000;
                                        sm.fromobject = 1000;
                                        sm.confirmationdate = DateTime.Now;
                                    }
                                    else
                                    {
                                        sm.fromobjecttype = 5000;
                                        sm.fromobject = userID;
                                    }
                                }
                                else
                                {
                                    sm.fromobjecttype = userType;// Convert.ToInt32(User.Identity.TitleCode);
                                    sm.fromobject = userID;
                                }
                                if (r.relatedtaskqueue != null) sm.confirmationdate = DateTime.Now;
                                sm.movementdate = DateTime.Now;
                                sm.updatedby = userID;
                                db.stockmovement.Add(sm);
                                db.SaveChanges();
                            }
                        }
                    }

                }

            }
            return Request.CreateResponse(HttpStatusCode.OK, tqid, "application/json");
        }


        [Route("SaveStockMovement")]
        [HttpPost]
        public HttpResponseMessage SaveStockMovement(stockmovement r, int? tqid, string serinos)
        {
            var userID = 7;
            if (r.movementid <= 0)
                return InsertStockMovement(r, tqid, serinos);
            else
                using (var db = new CRMEntities())
                {
                    var Records = db.stockmovement.Include(s => s.stockcard).Where(s => s.movementid == r.movementid);
                    if (Records.Count() > 0)
                    {
                        //var confirmationroles = (new KocCRMRoles[]{
                        //        KocCRMRoles.kscrProductionStaff,
                        //        KocCRMRoles.kscrSalesStaff,
                        //        KocCRMRoles.kscrStockStaff,
                        //        KocCRMRoles.kscrTechnicalStaff,
                        //        KocCRMRoles.kscrBackOfficeStaff,
                        //        KocCRMRoles.kscrCallCenterStaff
                        //    }).Select(role => (long)role).ToList();
                        var Record = Records.First();

                        if (r.toobject == userID && Record.confirmationdate == null)  //confirmationroles.Contains(r.toobjecttype) &&  if şartlarına eklenecek yetkilendirmeden sonra
                            Record.confirmationdate = DateTime.Now;
                        else
                        {
                            if (Record.stockcard.hasserial == true && string.IsNullOrWhiteSpace(r.serialno))
                                return Request.CreateResponse(HttpStatusCode.OK, tqid, "application/json");//seri numarası girilmesi gerekirken girilmemişse veya boşluk gibi bir karakter girilmişse
                            Record.amount = string.IsNullOrWhiteSpace(r.serialno) ? r.amount : 1;
                            Record.relatedtaskqueue = r.relatedtaskqueue;
                            Record.serialno = r.serialno;
                            Record.stockcardid = r.stockcardid;
                            Record.toobject = r.toobject;
                            Record.toobjecttype = r.toobjecttype;
                        }
                        Record.updatedby = userID;
                        Record.lastupdated = DateTime.Now;
                        db.SaveChanges();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, tqid, "application/json");
                }
        }
        #endregion
    }
}
