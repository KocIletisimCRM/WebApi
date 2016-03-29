using CRMWebApi.DTOs;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Teknar_Proxy_Lib;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Address")]
    public class ADSLAddressController : ApiController
    {
        [Route("getAdress")]
        [HttpPost]
        public async Task<HttpResponseMessage>  getAdress(DTOGetAdressFilter request)
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
                    }                    return Request.CreateResponse(HttpStatusCode.OK, res.Select(s=>s.toDTO()).ToList(), "application/json");
                }
                else if (filter.tableName=="bucak")
                {
                    var res = db.bucak.SqlQuery(querySql).ToList();
                    if (res.Count < 1)
                    {
                        var bucakData = $"lnaj1hV9YSL2LjqEADiS9aLNc2015111912awiAQeCo723cW7/IyM3kk045ditq+ESdsbTdVfgMbN5Anwk3lx6H/cc9QTTadbSJA==&t=vl&u={request.adres.value}";
                        var url = "http://adreskodu.dask.gov.tr/site-element/control/load.ashx";
                        int error = 1;
                        while (error==1)
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

                                return Request.CreateResponse(HttpStatusCode.OK,"-1", "application/json");

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
                    if (res.Count==0)
                    {
                        var ilData = "xMf1xgor4FG+LM9PJdCPruS0m2015111822y3yj7YxRo232eFmoz/WNhy11S9YjcSvl8NMI1rT2WwjdmPRLtK6lxgOEhjoQr+ezQ==&t=il&u=0";
                        var url = "http://adreskodu.dask.gov.tr/site-element/control/load.ashx";
                        db.il.AddRange((await TeknarProxyService.DeserializeJSON<daskResponseJSON>(url, ilData)).Result.yt.Skip(1).Where(s=>!(new int[] { 34, 61}).Contains(s.value.Value)).Select(s => new il
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
                    return Request.CreateResponse(HttpStatusCode.OK, res.Select(s=>s.toDTO()).ToList(), "application/json");
                }

            }

        }
    }
}
