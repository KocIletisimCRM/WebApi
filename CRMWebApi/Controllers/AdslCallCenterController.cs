using CRMWebApi.DTOs.Adsl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CRMWebApi.Models.Adsl;
using Teknar_Proxy_Lib;
using CRMWebApi.DTOs;
using System.Threading.Tasks;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/CallCenter")]
    public class AdslCallCenterController : ApiController
    {
        [Route("Callip")]
        [HttpPost]
        public HttpResponseMessage Callip()
        {
            string ip = null;
            if (Request.Properties.ContainsKey("MS_HttpContext"))
                ip = ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            return Request.CreateResponse(HttpStatusCode.OK, new { onay = control(Request), ips = ip }, "application/json");
        }

        [Route("saveAdslSalesTask")]
        [HttpPost]
        public HttpResponseMessage saveSalesTask(DTOcustomer request)
        {
            if (!control(Request)) // client ip controlü
                return Request.CreateResponse(HttpStatusCode.OK, false, "application/json");

            int[] ils = { 19, 61 }; // Bölge içi iller kaydedilecek taskid'si bölge içi veya dışı olarak seçim yapılacak
            bool inCheck = false; // Bölge içinde olan müşteri kontrolü
            foreach (int il in ils)
                if (request.ilKimlikNo == il)
                {
                    inCheck = true;
                    break;
                }
            if (!inCheck)
            { // Bölge içi gibi gelen taskı aynı türden bölge dışına çevir (Çağrı Satış Yalın -> Çağrı Satış Yalın Dış vb.)

            }

            using (var db = new KOCSAMADLSEntities())
            using (var transaction = db.Database.BeginTransaction())
                try
                {
                    var person = db.personel.FirstOrDefault(r => r.personelid == request.salespersonel);
                    if (person == null)
                        request.salespersonel = 1458; // Eğer gönderilen personel database'de yoksa ÇAĞRI MERKEZİ (KOÇ İLETİŞİM) satış yapsın

                    if (request.customerid == 0)
                    {
                        var oldCust = db.customer.Where(c => c.tc == request.tc && c.deleted == false).ToList();
                        if (oldCust.Count == 0)
                        {
                            var customer = new customer
                            {
                                customername = request.customername.ToUpper(),
                                tc = request.tc,
                                gsm = request.gsm,
                                phone = request.phone,
                                ilKimlikNo = request.ilKimlikNo,
                                ilceKimlikNo = request.ilceKimlikNo,
                                bucakKimlikNo = request.bucakKimlikNo,
                                mahalleKimlikNo = request.mahalleKimlikNo,
                                yolKimlikNo = 61,
                                binaKimlikNo = 61,
                                daire = 61,
                                updatedby = 1458, // ÇAĞRI MERKEZİ (KOÇ İLETİŞİM)
                                description = request.description,
                                lastupdated = DateTime.Now,
                                creationdate = DateTime.Now,
                                deleted = false,
                                email = request.email,
                                superonlineCustNo = request.superonlineCustNo,
                            };
                            db.customer.Add(customer);
                            db.SaveChanges();

                            request.customerid = customer.customerid;
                        }
                        else
                            return Request.CreateResponse(HttpStatusCode.OK, "Girilen TC Numarası Başkasına Aittir", "application/json");
                    }

                    var taskqueue = new adsl_taskqueue
                    {
                        attachedobjectid = request.customerid,
                        attachedpersonelid = request.salespersonel ?? 1458, // yoksa ÇAĞRI MERKEZİ (KOÇ İLETİŞİM)'a ata
                        attachmentdate = DateTime.Now,
                        creationdate = DateTime.Now,
                        deleted = false,
                        description = request.taskdescription,
                        lastupdated = DateTime.Now,
                        status = null,
                        taskid = request.taskid,
                        updatedby = 1458,
                        fault = request.fault
                    };

                    db.taskqueue.Add(taskqueue);
                    db.SaveChanges();

                    if (request.productids != null)
                    {
                        foreach (var item in request.productids)
                        {
                            var customerproducst = new adsl_customerproduct
                            {
                                taskid = taskqueue.taskorderno,
                                customerid = request.customerid,
                                productid = item,
                                campaignid = request.campaignid,
                                creationdate = DateTime.Now,
                                lastupdated = DateTime.Now,
                                updatedby = 1458,
                                deleted = false
                            };
                            db.customerproduct.Add(customerproducst);
                        }
                        db.SaveChanges();
                    }
                    transaction.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "Tamamlandı", "application/json");
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return Request.CreateResponse(HttpStatusCode.OK, e.Message, "application/json");
                }
        }

        [Route("getAdress")]
        [HttpPost]
        public async Task<HttpResponseMessage> getAdress(DTOGetAdressFilter request)
        {
            if (!control(Request))
                return Request.CreateResponse(HttpStatusCode.OK, false, "application/json");

            using (var db = new KOCSAMADLSEntities())
            {
                //var il = TeknarProxyService.DeserializeJSON<il>();
                var filter = request.getFilter();
                var querySql = filter.getFilterSQL();
                if (filter.tableName == "ilce")
                {
                    var res = db.ilce.SqlQuery(querySql).ToList();
                    if (res.Count < 2)
                    {
                        var ilData = $"lnaj1hV9YSL2LjqEADiS9aLNc2015111912awiAQeCo723cW7/IyM3kk045ditq+ESdsbTdVfgMbN5Anwk3lx6H/cc9QTTadbSJA==&t=ce&u={request.adres.value}";
                        var url = "http://adreskodu.dask.gov.tr/site-element/control/load.ashx";
                        try
                        {
                            var dres = (await TeknarProxyService.DeserializeJSON<daskResponseJSON>(url, ilData));
                            db.ilce.AddRange(dres.Result.yt.Skip(1).Where(s => !(new int[] { 2097 }).Contains(s.value.Value)).Select(s => new ilce
                            {
                                ad = s.text,
                                ilKimlikNo = Convert.ToInt32(request.adres.value),
                                kimlikNo = s.value.Value
                            }));
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        res = db.ilce.SqlQuery(querySql).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
                }
                else if (filter.tableName == "bucak")
                {
                    var res = db.bucak.SqlQuery(querySql).ToList();
                    if (res.Count < 1)
                    {
                        var bucakData = $"lnaj1hV9YSL2LjqEADiS9aLNc2015111912awiAQeCo723cW7/IyM3kk045ditq+ESdsbTdVfgMbN5Anwk3lx6H/cc9QTTadbSJA==&t=vl&u={request.adres.value}";
                        var url = "http://adreskodu.dask.gov.tr/site-element/control/load.ashx";
                        int error = 1;
                        while (error == 1)
                        {
                            try
                            {
                                var dres = (await TeknarProxyService.DeserializeJSON<daskResponseJSON>(url, bucakData));
                                db.bucak.AddRange(dres.Result.yt.Skip(1).Where(s => !(new int[] { 0 }).Contains(s.value.Value)).Select(s => new bucak
                                {
                                    ad = s.text,
                                    ilceKimlikNo = Convert.ToInt32(request.adres.value),
                                    kimlikNo = s.value.Value
                                }));
                                db.SaveChanges();
                                error = 0;
                            }
                            catch (Exception)
                            {

                                return Request.CreateResponse(HttpStatusCode.OK, "-1", "application/json");
                            }
                        }


                        res = db.bucak.SqlQuery(querySql).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
                }
                else if (filter.tableName == "mahalleKoy")
                {
                    var res = db.mahalleKoy.SqlQuery(querySql).ToList();
                    if (res.Count < 1)
                    {
                        var bucakData = $"lnaj1hV9YSL2LjqEADiS9aLNc2015111912awiAQeCo723cW7/IyM3kk045ditq+ESdsbTdVfgMbN5Anwk3lx6H/cc9QTTadbSJA==&t=mh&u={request.adres.value}";
                        var url = "http://adreskodu.dask.gov.tr/site-element/control/load.ashx";
                        int error = 1;
                        while (error == 1)
                        {
                            try
                            {
                                var dres = (await TeknarProxyService.DeserializeJSON<daskResponseJSON>(url, bucakData));
                                db.mahalleKoy.AddRange(dres.Result.yt.Skip(1).Where(s => !(new int[] { 0 }).Contains(s.value.Value)).Select(s => new mahalleKoy
                                {
                                    ad = s.text,
                                    bucakKimlikNo = Convert.ToInt32(request.adres.value),
                                    kimlikNo = s.value.Value
                                }));
                                db.SaveChanges();
                                error = 0;
                            }
                            catch (Exception)
                            {

                                return Request.CreateResponse(HttpStatusCode.OK, "-1", "application/json");

                            }
                        }
                        res = db.mahalleKoy.SqlQuery(querySql).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
                }
                else if (filter.tableName == "cadde")
                {
                    var res = db.cadde.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else if (filter.tableName == "bina")
                {
                    var res = db.bina.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else if (filter.tableName == "daire")
                {
                    var res = db.daire.SqlQuery(querySql).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
                }
                else
                {
                    var res = db.il.SqlQuery(querySql).ToList();
                    if (res.Count == 0)
                    {
                        var ilData = "xMf1xgor4FG+LM9PJdCPruS0m2015111822y3yj7YxRo232eFmoz/WNhy11S9YjcSvl8NMI1rT2WwjdmPRLtK6lxgOEhjoQr+ezQ==&t=il&u=0";
                        var url = "http://adreskodu.dask.gov.tr/site-element/control/load.ashx";
                        db.il.AddRange((await TeknarProxyService.DeserializeJSON<daskResponseJSON>(url, ilData)).Result.yt.Skip(1).Where(s => !(new int[] { 34, 61 }).Contains(s.value.Value)).Select(s => new il
                        {
                            ad = s.text,
                            kimlikNo = s.value.Value
                        }));
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        res = db.il.SqlQuery(querySql).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res.Select(s => s.toDTO()).ToList(), "application/json");
                }

            }

        }

        [Route("getCampaignInfo")]
        [HttpPost]
        public HttpResponseMessage getCampaignInfo(DTOFiterGetCampaignRequst request)
        {
            if (!control(Request))
                return Request.CreateResponse(HttpStatusCode.OK, false, "application/json");

            var filter = request.getFilter();
            using (var db = new KOCSAMADLSEntities(false))
            {
                if (request.isCategoryFilter())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, db.campaigns.SqlQuery(filter.getFilterSQL()).Select(tt => new { tt.category }).Distinct().OrderBy(t => t.category).ToList(), "application/json");
                }
                else if (request.isSubcategoryFilter())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, db.campaigns.SqlQuery(filter.getFilterSQL()).Select(tt => new { tt.subcategory }).Distinct().OrderBy(t => t.subcategory).ToList(), "application/json");
                }
                else if (request.isCampaignFilter())
                    return Request.CreateResponse(HttpStatusCode.OK, db.campaigns.SqlQuery(filter.getFilterSQL()).Select(tt => new { tt.name, tt.id }).OrderBy(t => t.name).ToList(), "application/json");
                else
                {
                    var cids = db.campaigns.SqlQuery(filter.getFilterSQL()).Select(s => s.id).ToList();
                    var pids = db.vcampaignproducts.Where(v => cids.Contains(v.cid)).Select(s => s.pid).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK,
                        db.product_service.Where(pp => pids.Contains(pp.productid)).OrderBy(s => s.category).ToList().GroupBy(g => g.category, g => g.toDTO()).Select(g => new { category = g.Key, products = g })
                        , "application/json");
                }
            }
        }

        [Route("confirmCustomer")]
        [HttpPost]
        public HttpResponseMessage confirmCustomer(DTOcustomer request)
        {
            if (!control(Request))
                return Request.CreateResponse(HttpStatusCode.OK, false, "application/json");

            using (var db = new KOCSAMADLSEntities(false))
            {
                if (request.tc != null)
                {
                    var res = db.customer.Where(c => c.tc == request.tc && c.deleted == false).FirstOrDefault();
                    if (res != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, res.toDTO(), "application/json");
                    }
                    else
                    {
                        DTOResponseError error = new DTOResponseError();
                        error.errorCode = -1;
                        return Request.CreateResponse(HttpStatusCode.OK, error.errorCode, "application/json");
                    }
                }
                else if (request.superonlineCustNo != null)
                {
                    var res = db.customer.Where(c => c.superonlineCustNo == request.superonlineCustNo && c.deleted == false).OrderByDescending(n => n.customerid).ToList();
                    if (res.Count > 0)
                    {
                        customer retCust = null;
                        customer gecici = null;
                        foreach (customer eleman in res)
                        { // müşterilerin taskları kontrol edilecek ana hiyerarşi tasklarında iptal olmayan ilk müşteriyi geri döndürecem
                            gecici = eleman;
                            var iTask = db.taskqueue.Where(t => t.deleted == false && t.attachedobjectid == eleman.customerid && t.status == 9116).ToList();
                            if (iTask.Count > 0)
                            {
                                foreach (adsl_taskqueue tt in iTask)
                                {
                                    if (db.task.Where(t => t.taskid == tt.taskid && (t.tasktype == 1 || t.tasktype == 2 || t.tasktype == 3 || t.tasktype == 5)).FirstOrDefault() == null)
                                    {
                                        retCust = eleman;
                                        break;
                                    }
                                }
                                if (retCust != null)
                                    break;
                            }
                            else
                            {
                                retCust = eleman;
                                break;
                            }
                        }
                        if (retCust == null)
                            retCust = gecici;
                        return Request.CreateResponse(HttpStatusCode.OK, retCust.toDTO(), "application/json");
                    }
                    else
                    {
                        DTOResponseError error = new DTOResponseError();
                        error.errorCode = -1;
                        return Request.CreateResponse(HttpStatusCode.OK, error.errorCode, "application/json");
                    }
                }
                else
                {
                    DTOResponseError error = new DTOResponseError();
                    error.errorCode = -1;
                    return Request.CreateResponse(HttpStatusCode.OK, error.errorCode, "application/json");
                }
            }

        }

        // client ip'si kontrolü yapılır. Güvenlik için localden erişim yapmalıdır. (entegre için onların ip'si eklendi. silinecek (213.14.169.225))
        private Boolean control (HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                if (((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress == "46.2.78.85") // hüseyin ev
                    return true;
                if (((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress == "213.14.169.225")
                    return true;
                if (((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress == "213.153.197.167")
                    return true;
                var ip = ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                var digit = ip.Split('.');
                if (digit != null && digit.Length > 3 && digit[0] == "192" && digit[1] == "168" && digit[2] == "1")
                    return true;
                return false;
            }
            else
            {
                return false;
            }
        }
    }
}
