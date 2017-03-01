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
using System.Collections;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Stock")]
    [KOCAuthorize]
    public class AdslStockController : ApiController
    {
        #region Stok Hareketleri Sayfası
        [Route("getStockMovements")]
        [HttpPost]
        public HttpResponseMessage getStockMovements(DTOGetStockMovementRequest request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            {
                if (request.fromobject != null && request.fromobject.value == null && request.toobject.value == null)
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
                    if (user.userRole == 2147483647 || user.hasRole(KOCUserTypes.StockRoomStuff)) { }
                    else if (user.hasRole(KOCUserTypes.TeamLeader))
                    {
                        var rolelist = Enum.GetValues(typeof(KOCUserTypes)).OfType<KOCUserTypes>().Where(r => user.hasRole(r)).Select(r => (int)r).ToList();
                        rolelist.Add(user.userRole);
                        whereClauses.Add($"(fromobjecttype in ({string.Join(",", rolelist)}) or toobjecttype in ({string.Join(",", rolelist)}))");
                    }
                    else whereClauses.Add($"((fromobject = {user.userId} and fromobjecttype!= 16777217) or (toobject = {user.userId} and toobjecttype!= 16777217))"); // aynı id'ye sahip personel ve müşterilerde müşterinin hareketini personelinmiş gibi göstermesin diye type eklendi (16777217 -> müşteri type)
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
                var fromPerObjectIds = res.Where(t => t.fromobjecttype != (int)KOCUserTypes.ADSLCustomer).Select(s => s.fromobject).Distinct().ToList();
                var fromCusObjectIds = res.Where(t => t.fromobjecttype == (int)KOCUserTypes.ADSLCustomer).Select(s => s.fromobject).Distinct().ToList();
                var fromPersonels = db.personel.Where(p => fromPerObjectIds.Contains(p.personelid)).ToList();
                var fromCustomers = db.customer.Where(c => fromCusObjectIds.Contains(c.customerid)).ToList();


                var toPerObjectIds = res.Where(t => t.toobjecttype != (int)KOCUserTypes.ADSLCustomer).Select(s => s.toobject).Distinct().ToList();
                var toCusObjectIds = res.Where(t => t.toobjecttype == (int)KOCUserTypes.ADSLCustomer).Select(s => s.toobject).Distinct().ToList();
                var toPersonels = db.personel.Where(p => toPerObjectIds.Contains(p.personelid)).ToList();
                var toCustomers = db.customer.Where(c => toCusObjectIds.Contains(c.customerid)).ToList();


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
                        var userControl = db.stockmovement.Where(s => s.serialno == seri && s.deleted == false).OrderByDescending(s => s.movementid).Select(s => s.toobject).FirstOrDefault();
                        if (seri != null && (userControl != userID) && ((r.toobjecttype & (int)KOCUserTypes.StockRoomStuff) != (int)KOCUserTypes.StockRoomStuff))//satınalmadan depoya çıkış için özel durum
                        {
                            errormessage.errorCode = -1;
                            errormessage.errorMessage = "Yalnızca Kendinize Ait Ürünleri Başkasına Çıkabilirsiniz";
                        }

                        #region hareket uygunsa
                        else
                        {
                            var count = db.stockmovement.Where(s => s.serialno == seri).Count();
                            //satınalmadan depoya ürün girerken seri numarası kontrolü yap
                            if (r.fromobjecttype == 33554433 && (r.toobjecttype & (int)KOCUserTypes.StockRoomStuff) == (int)KOCUserTypes.StockRoomStuff && (int)count > 0)
                            {
                                errormessage.errorCode = -1;
                                errormessage.errorMessage = seri + " Seri numarası daha önce girilmiş! Lütfen Kontrol Ediniz!";
                            }
                            else
                            {
                                var stm = db.stockmovement.Where(s => s.serialno == seri && s.deleted == false && s.confirmationdate == null).ToList();
                                stm.ForEach(s =>
                                {
                                    s.updatedby = userID;
                                    s.lastupdated = DateTime.Now;
                                    s.deleted = true;
                                }); // bir seri onaylanmadan başka birisine çıkıldığında onaylanmayan hareketi sil (Hüseyin KOZ) 27.10.2016

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
                                if (KOCAuthorizeAttribute.getCurrentUser().userRole == (int)KOCUserTypes.StockRoomStuff)// (long)KocCRMRoles.kscrStockStaff
                                {
                                    if (r.toobjecttype == (int)KOCUserTypes.StockRoomStuff)// ise bu bir satınalma işlemidir.
                                    {

                                        sm.fromobjecttype = (int)KOCUserTypes.ADSLProcurementAssosiation;
                                        sm.fromobject = (int)KOCUserTypes.ADSLProcurementAssosiation;
                                        sm.confirmationdate = DateTime.Now;
                                    }
                                    else
                                    {
                                        sm.fromobjecttype = (int)KOCUserTypes.StockRoomStuff;
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
                        #endregion 
                    }

                }

            }
            return Request.CreateResponse(HttpStatusCode.OK, tqid, "application/json");
        }

        [Route("SaveStockMovement")]
        [HttpPost]
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
        public HttpResponseMessage confirmSM(int[] movementIds)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            DTOResponseError errormessage = new DTOResponseError();
            if (movementIds.Length > 0)
            {
                using (var db = new KOCSAMADLSEntities())
                using (var tran = db.Database.BeginTransaction())
                    try
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
                            }
                            db.SaveChanges();
                            tran.Commit();
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
                    catch (Exception)
                    {
                        tran.Rollback();
                        errormessage = new DTOResponseError { errorCode = 2, errorMessage = "işlem Tamamlanamadı!" };
                        return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                    }
            }
            else
            {
                errormessage.errorMessage = "Onaylamak İçin En Az 1 Stok Hareketi Seçmelisiniz!";
                errormessage.errorCode = -1;
                return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
            }
        }

        [Route("InsertStock")]
        [HttpPost]
        public HttpResponseMessage InsertStock(DTOstockmovement r)
        { // Bağlantı problemi veya (fromobject,fromobjecttype,toobject,toobjecttype,serial,amount) bilgileri gönderidiğinde çalışacak yeni stock hareketi 
            if (r.deleted == true)
            {
                // *** yeni stok hareketi olacağına deleted ile karar vereceğiz (true'ysa seriyi satınalma -> depoya -> müşteriye -> personele aktar)
                var sm = new DTOstockmovement();
                sm.serialno = r.serialno;
                sm.amount = 1;
                sm.toobjecttype = 2; // depocu
                sm.toobject = 1007; // depocu
                sm.fromobjecttype = 33554433;
                sm.fromobject = 33554433;
                sm.stockcardid = 1117;
                movement(sm);

                var sm1 = new DTOstockmovement();
                sm1.serialno = r.serialno;
                sm1.amount = 1;
                sm1.toobjecttype = 16777217; // müşteri
                sm1.toobject = r.fromobject; // müşteri
                sm1.fromobjecttype = 2;
                sm1.fromobject = 1007;
                sm1.stockcardid = 1117;
                movement(sm1);

                var sm2 = new DTOstockmovement();
                sm2.serialno = r.serialno;
                sm2.amount = 1;
                sm2.toobjecttype = r.toobjecttype; // işlem yapan personel
                sm2.toobject = r.toobject; // işlem yapan personel
                sm2.fromobjecttype = 16777217;
                sm2.fromobject = r.fromobject;
                sm2.stockcardid = 1117;
                movement(sm2);
            }
            else if (r.movementid == -1)
            {
                // *** yeni stok hareketi olacağına movementid ile karar vereceğiz (-1'se seriyi satınalma -> depoya -> müşteriye aktar)
                var sm = new DTOstockmovement();
                sm.serialno = r.serialno;
                sm.amount = 1;
                sm.toobjecttype = 2; // depocu
                sm.toobject = 1007; // depocu
                sm.fromobjecttype = 33554433;
                sm.fromobject = 33554433;
                sm.stockcardid = 1117;
                movement(sm);

                var sm1 = new DTOstockmovement();
                sm1.serialno = r.serialno;
                sm1.amount = 1;
                sm1.toobjecttype = 16777217; // müşteri
                sm1.toobject = r.toobject; // müşteri
                sm1.fromobjecttype = 2;
                sm1.fromobject = 1007;
                sm1.stockcardid = 1117;
                movement(sm1);
            }
            else
                movement(r);

            return Request.CreateResponse(HttpStatusCode.OK, true, "application/json");
        }

        private void movement(DTOstockmovement r)
        {
            var userID = KOCAuthorizeAttribute.getCurrentUser().userId;
            using (var db = new KOCSAMADLSEntities())
            {
                adsl_stockmovement sm = new adsl_stockmovement();
                sm.serialno = r.serialno;
                sm.lastupdated = DateTime.Now;
                sm.updatedby = userID;
                sm.creationdate = DateTime.Now;
                sm.toobjecttype = r.toobjecttype;
                sm.stockcardid = r.stockcardid;
                sm.toobject = r.toobject;
                sm.deleted = false;
                sm.amount = r.amount;
                sm.fromobjecttype = r.fromobjecttype;
                sm.fromobject = r.fromobject;
                sm.movementdate = DateTime.Now;
                if (r.fromobjecttype == 16777217 || r.toobjecttype == 16777217 || r.fromobjecttype == 33554433) sm.confirmationdate = DateTime.Now; // hareket müşteridense onaylı olmalı
                sm.updatedby = userID;
                db.stockmovement.Add(sm);
                db.SaveChanges();
            }
        }

        [Route("getStock")]
        [HttpPost]
        public HttpResponseMessage getStock(DTOGetStockMovementRequest request)
        { // getStockMovements --> fromobject (id) ve toobject (id) olarak geldiğinde isim kontrolü ile çakışma olduğu için oluşturuldu 
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            {
                if (request.fromobject != null && request.fromobject.value == null && request.toobject.value == null)
                {
                    request.fromobject.value = "";
                }
                var filter = request.getFilter();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var countSql = filter.getCountSQL();

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

        [Route("getSerialOnCustomer")]
        [HttpPost]
        public HttpResponseMessage getSerialOnCustomer(adsl_stockmovement request)
        { // gönderilen filtre sorgusu sonucu bulunan müşteriye stok hareketlerinden halen müşteride bulunan stoklardan movemenid'si küçük olanı döndürür (lazım olursa liste yapılıp döndürülebilir)
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            {
                var ret = db.getSerialsOnCustomerAdsl(request.fromobject, request.stockcardid).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, ret, "application/json");
            }
        }

        [Route("getSerialOnPersonel")]
        [HttpPost]
        public HttpResponseMessage getSerialOnPersonel(adsl_stockmovement request)
        {
            var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities())
            {
                var ret = db.getSerialsOnPersonelAdsl(request.fromobject, request.stockcardid).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, ret, "application/json");
            }
        }

        [Route("getStocksOnPersonel")]
        [HttpPost]
        public HttpResponseMessage getStocksOnPersonel(DTOGetPersonelStock request)
        { // Personellerin Üzerlerindeki serileri görebilmeleri için oluşturuldu (Stok-seri kontrolü)
            ArrayList list = new ArrayList();
            var user = KOCAuthorizeAttribute.getCurrentUser();
            var personel = request.personelid != null ? request.personelid.Value : user.userId;
            using (var db = new KOCSAMADLSEntities())
            {
                var type = db.stockcard.ToList();
                for (int i = 0; i < type.Count; i++)
                {
                    var serials = db.getSerialsOnPersonelAdsl(personel, type[i].stockid).ToList();
                    for (int j = 0; j < serials.Count; j++)
                    {
                        DTOStockReturn res = new DTOStockReturn();
                        res.stockid = type[i].stockid;
                        res.stockname = type[i].productname;
                        res.personelid = personel;
                        res.serials = serials[j];
                        list.Add(res);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, list, "application/json");
            }
        }
    }
}
