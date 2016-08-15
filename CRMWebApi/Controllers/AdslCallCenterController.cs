﻿using CRMWebApi.DTOs.Adsl;
using System.ServiceModel.Channels;
using System;
using System.Collections.Generic;
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
        // client ip'si kontrolü yapılır. Güvenlik için localden erişim yapmalıdır.
        [Route("ipControl")]
        [HttpPost]
        public HttpResponseMessage ipControl(DTOCallCenter request)
        {
            if (request.gsm == "213.153.197.167") // koç iletişim dış ip
            {
                return Request.CreateResponse(HttpStatusCode.OK, true, "application/json");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, false, "application/json");
            }
        }

        [Route("saveAdslSalesTask")]
        [HttpPost]
        public HttpResponseMessage saveSalesTask(DTOcustomer request)
        {
            using (var db = new KOCSAMADLSEntities())
            {
                var person = db.personel.FirstOrDefault(r => r.personelid == request.salespersonel);
                if (person == null)
                    request.salespersonel = 1393; // Eğer gönderilen personel database'de yoksa yazılım koç satış yapsın
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
                        yolKimlikNo = request.yolKimlikNo,
                        binaKimlikNo = request.binaKimlikNo,
                        daire = request.daire,
                        updatedby = 1393, // yazılım koç
                        description = request.description,
                        lastupdated = DateTime.Now,
                        creationdate = DateTime.Now,
                        deleted = false,
                        email = request.email,
                        superonlineCustNo = request.superonlineCustNo,
                    };
                    db.customer.Add(customer);
                    db.SaveChanges();

                    var cust = db.customer.Where(c => c.tc == request.tc && c.customername == request.customername).FirstOrDefault();

                    var taskqueue = new adsl_taskqueue
                    {
                        appointmentdate = request.appointmentdate > DateTime.Now ? DateTime.Now : request.appointmentdate, // netflow tarihi ileri tarih olamaz
                        attachedobjectid = cust.customerid,
                        attachedpersonelid = request.salespersonel ?? 1393, // yoksa yazılım koç'a ata
                        attachmentdate = DateTime.Now,
                        creationdate = DateTime.Now,
                        deleted = false,
                        description = request.taskdescription,
                        lastupdated = DateTime.Now,
                        status = null,
                        taskid = request.taskid,
                        updatedby = 1393,
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
                                customerid = customer.customerid,
                                productid = item,
                                campaignid = request.campaignid,
                                creationdate = DateTime.Now,
                                lastupdated = DateTime.Now,
                                updatedby = 1393,
                                deleted = false
                            };
                            db.customerproduct.Add(customerproducst);
                        }
                        db.SaveChanges();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, taskqueue.taskorderno, "application/json");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Girilen TC Numarası Başkasına Aittir", "application/json");
            }
        }

        [Route("getAdress")]
        [HttpPost]
        public async Task<HttpResponseMessage> getAdress(DTOGetAdressFilter request)
        {
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
    }
}
