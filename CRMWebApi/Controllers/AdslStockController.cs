using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Net.Http;
using System.Web.Http;
using CRMWebApi.KOCAuthorization;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Stock")]
    public class AdslStockController : ApiController
    {
        #region Stok Hareketleri Sayfası
        [Route("getStockMovements")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage getStockMovements(DTOGetStockMovementRequest request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            {
                if(request.fromobject!=null && request.fromobject.value == null && request.toobject.value == null)
                {
                    request.fromobject.value = "";
                }
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
                            newClauses[0].Add(clause.Replace("))", ")").Replace("stockmovement.fromobject1", "stockmovement.fromobject") + ")");

                        else if (clause.Contains("toobject1"))
                            newClauses[1].Add(clause.Replace("))", ")").Replace("stockmovement.toobject1", "stockmovement.toobject") + ")");
                        else
                            newClauses[clause.Contains("fromobject") ? 0 : 1].Add(clause + ")");
                        pagingWhereClauses.Remove(clause);
                    }
                    var whereClauses = new List<string>();

                    if (pagingWhereClauses.Skip(1).Any()) whereClauses.Add(string.Join(") AND", pagingWhereClauses.Skip(1)) + ")");
                    if (newClauses[0].Any()) whereClauses.Add($"({string.Join(" OR ", newClauses[0])})");
                    if (newClauses[1].Any()) whereClauses.Add($"({string.Join(" OR ", newClauses[1])})");
                    if (user.hasRole(KOCUserTypes.TeamLeader))
                    {
                        var rolelist = Enum.GetValues(typeof(KOCUserTypes)).OfType<KOCUserTypes>().Where(r => user.hasRole(r)).Select(r => (int)r).ToList();
                        whereClauses.Add($"(fromobjecttype in ({string.Join(",", rolelist)}) or toobjecttype in ({string.Join(",", rolelist)}))");
                    }
                    else whereClauses.Add($"(fromobject = {user.userId} or toobject = {user.userId})");
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


                var toObjectIds = res.Select(s => s.toobject).Distinct().ToList();
                var toPersonels = db.personel.Where(p => toObjectIds.Contains(p.personelid)).ToList();
                var toCustomers = db.customer.Where(c => toObjectIds.Contains(c.customerid)).ToList();


                var stockcardids = res.Select(s => s.stockcardid).Distinct().ToList();
                var stockcards = db.stockcard.Where(s => stockcardids.Contains(s.stockid)).ToList();

                res.ForEach(r =>
                {
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
        [KOCAuthorize]
        public HttpResponseMessage SaveStockMovementMultiple(adsl_stockmovement[] sms)
        {
            var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
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
            return Request.CreateResponse(HttpStatusCode.OK,
                    errormessage
                    , "application/json");
        }

        [Route("InsertStockMovement")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage InsertStockMovement(adsl_stockmovement r, int? tqid, string serinos)
        {
            //  var serinos = Request.Params.AllKeys;
            var serials = serinos.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var errormessage = new DTOResponseError();
            if (!serials.Any()) serials.Add(null);
            if (ModelState.IsValid)
            {
                var userID = KOCAuthorizeAttribute.getCurrentUser().userId;//depocu
                var userType = KOCAuthorizeAttribute.getCurrentUser().userRole;
                using (var db = new KOCSAMADLSEntities())
                {
                    foreach (var seri in serials)
                    {
                        //serino kontrolü yap. varsa ekleme.
                        var userControl = db.stockmovement.Where(s => s.serialno == seri).OrderByDescending(s => s.movementid).Select(s => s.toobject).FirstOrDefault();
                        if ((userControl != userID) && ((r.toobjecttype & (int)KOCUserTypes.StockRoomStuff) != (int)KOCUserTypes.StockRoomStuff))//satınalmadan depoya çıkış için özel durum
                        {
                            errormessage.errorCode = -1;
                            errormessage.errorMessage = "Yalnızca Kendinize Ait Ürünleri Başkasına Çıkabilirsiniz";
                        }
                        else
                        {
                            var count = db.stockmovement.Where(s => s.serialno == seri).Count();
                            //satınalmadan depoya ürün girerken seri numarası kontrolü yap
                            if ((r.toobjecttype & (int)KOCUserTypes.StockRoomStuff) == (int)KOCUserTypes.StockRoomStuff && (int)count > 0)
                            {
                                errormessage.errorCode = -1;
                                errormessage.errorMessage = seri + " Seri numarası daha önce girilmiş! Lütfen Kontrol Ediniz!";
                            }
                            else
                            {


                                adsl_stockmovement sm = new adsl_stockmovement();
                                sm.serialno = seri;
                                sm.lastupdated = DateTime.Now;
                                sm.creationdate = DateTime.Now;
                                sm.toobjecttype = r.toobjecttype;
                                sm.stockcardid = r.stockcardid;
                                sm.toobject = r.toobject;
                                sm.deleted = false;
                                sm.amount = seri == null ? r.amount : 1;
                                sm.relatedtaskqueue = tqid;
                                if ((KOCAuthorizeAttribute.getCurrentUser().userRole & (int)KOCUserTypes.StockRoomStuff) == (int)KOCUserTypes.StockRoomStuff)// (long)KocCRMRoles.kscrStockStaff
                                {
                                    if ((r.toobjecttype & (int)KOCUserTypes.StockRoomStuff) == (int)KOCUserTypes.StockRoomStuff)
                                    {

                                        sm.fromobjecttype = (int)KOCUserTypes.ADSLProcurementAssosiation;
                                        sm.fromobject = (int)KOCUserTypes.ADSLProcurementAssosiation;
                                        sm.confirmationdate = DateTime.Now;
                                    }
                                    else
                                    {
                                        sm.fromobjecttype = (int)KOCUserTypes.ADSLStockRoomAssosiation;
                                        sm.fromobject = KOCAuthorizeAttribute.getCurrentUser().userId;
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
        [KOCAuthorize]
        public HttpResponseMessage SaveStockMovement(adsl_stockmovement r, int? tqid, string serinos)
        {
            var userID = KOCAuthorizeAttribute.getCurrentUser().userId;
            if (r.movementid <= 0)
                return InsertStockMovement(r, tqid, serinos);
            else
                using (var db = new KOCSAMADLSEntities())
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

                        if (r.toobject == userID && r.confirmationdate != null && Record.confirmationdate == null)  //confirmationroles.Contains(r.toobjecttype) &&  if şartlarına eklenecek yetkilendirmeden sonra
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


        [Route("confirmSM")]
        [HttpPost]
        [KOCAuthorize]
        public HttpResponseMessage confirmSM(int[] movementIds)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            DTOResponseError errormessage = new DTOResponseError();
            if (movementIds.Length > 0)
            {
                using (var db = new KOCSAMADLSEntities(false))
                {
                    //yalnızca kendine çıkılan stoklar onaylanabilir
                    if (db.stockmovement.Where(s => movementIds.Contains(s.movementid)).Any(a => a.toobject == user.userId))
                    {
                        foreach (var item in movementIds)
                        {
                            var sm = db.stockmovement.Where(s => s.movementid == item && s.confirmationdate == null).FirstOrDefault();
                            sm.confirmationdate = DateTime.Now;
                            sm.updatedby = user.userId;
                            sm.lastupdated = DateTime.Now;
                            db.SaveChanges();
                        }
                        errormessage.errorMessage = "Onaylama işlemi başarılı";
                        errormessage.errorCode = 1;
                        return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                    }
                    else
                    {
                        errormessage.errorMessage = "Sadece Üzerinize Atanmış Stokları Onaylayabilirsiniz";
                        errormessage.errorCode = -1;
                        return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");

                    }

                }
            }
            else
            {
                errormessage.errorMessage = "Onaylamak İçin En Az 1 Stok Hareketi Seçmelisiniz!";
                errormessage.errorCode = -1;
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }
    }
}
