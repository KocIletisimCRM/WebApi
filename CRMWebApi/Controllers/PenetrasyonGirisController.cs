﻿using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using CRMWebApi.DTOs;
using System.Data.SqlClient;


namespace CRMWebApi.Controllers
{
     [RoutePrefix("api/Penetrasyon")]
    public class PenetrasyonGirisController : ApiController
    {
         public class getCustomerParams
         {
             public int? siteid { get; set; }
             public int? blockid { get; set; }
             public string name { get; set; }
             public Boolean closedtasks { get; set; }
         }
         [Route("getCustomer")]
         [HttpPost]
         public HttpResponseMessage getCustomer(getCustomerParams param)
         {
             SqlParameter siteid = new SqlParameter("@siteid", System.Data.SqlDbType.Int);
             siteid.Value = (object)param.siteid ?? DBNull.Value;
             SqlParameter blockid = new SqlParameter("@blockid", System.Data.SqlDbType.Int);
             blockid.Value = (object)param.blockid ?? DBNull.Value;
             SqlParameter name = new SqlParameter("@name", System.Data.SqlDbType.VarChar);
             name.Value = (object)param.name ?? DBNull.Value;
            

             using (var db = new CRMEntities())
             {
                 db.Configuration.AutoDetectChangesEnabled = false;

                 db.Configuration.LazyLoadingEnabled = false;
                 db.Configuration.ProxyCreationEnabled = false;
               
                 var penetarations = db.taskqueue.Where(tq => tq.taskid == 86  && (tq.status ==null || param.closedtasks) && tq.creationdate.Value.Year == 2015 && tq.deleted == false);


                 var res = db.customer.Include(c=>c.block.site)
                     .Where(c=>
                         penetarations.Any(p=>p.attachedobjectid==c.customerid) && 
                         (param.name == null || c.customername.Contains(param.name)) &&
                         (param.blockid == null || c.blockid==param.blockid)&&
                         (param.siteid==null || c.block.siteid==param.siteid)
                     ).ToList();
//                 string sqlcommand = @"select s.sitename,b.blockname, c.* from taskqueue tq left join customer c on tq.attachedobjectid=c.customerid
//							left join block b on c.blockid=b.blockid
//							left join site s on b.siteid=s.siteid
//                            where taskid=86 and status is null and tq.creationdate >'2015-01-01 13:32:20.3594437'
//                            and c.deleted=0 and tq.deleted=0 
//                            and (s.siteid=@siteid or @siteid is null) 
//                            and (b.blockid=@blockid or @blockid is null)
//                            and (c.customername like CONCAT('%',@name,'%') or @name is null) ";
                 
                 //var res=db.customer.SqlQuery(sqlcommand, new object[]{siteid, blockid, name}).ToList();
                 
                 
                 return Request.CreateResponse(HttpStatusCode.OK, res.Select(s=>s.toDTO()), "application/json");
             }
         }


         [Route("getSite")]
         [HttpGet]
         public HttpResponseMessage getSite() {
             
             using (var db=new CRMEntities())
             {  
                 db.Configuration.AutoDetectChangesEnabled = false;
                 db.Configuration.LazyLoadingEnabled = false;
                 db.Configuration.ProxyCreationEnabled = false;
                 var res = db.site.Where(s => s.deleted == false).OrderBy(o => o.sitename).ToList();
                  return Request.CreateResponse(HttpStatusCode.OK,res,"application/json");
             }
         }

         [Route("getBlock")]
         [HttpGet]
         public HttpResponseMessage getBlock(int siteid)
         {
             if (siteid == null) siteid = 100;
             using (var db = new CRMEntities())
             {
                 var x = db.block.Where(b => b.siteid == siteid && b.deleted == false).OrderBy(o=>o.blockname).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK, x, "application/json");
             }
         }

         [Route("getTurkcellTv")]
         [HttpGet]
         public HttpResponseMessage getTurkcellTv()
         {
             using (var db = new CRMEntities())
             {
                 var res = db.TurkcellTVStatus.Where(g => g.deleted == 0);
                 return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
             }

         }
         [Route("getGsmStatus")]
         [HttpGet]
         public HttpResponseMessage getGsmStatus()
         {
             using (var db = new CRMEntities())
             {
                 var res = db.gsmKullanımıStatus.Where(g => g.deleted == 0);
                 return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
             }

         }
         [Route("getNetStatus")]
         [HttpGet]
         public HttpResponseMessage getNetStatus()
         {
             using (var db = new CRMEntities())
             {
                 var res = db.netStatus.Where(g => g.deleted == 0);
                 return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
             }

         }
         [Route("getIISstatus")]
         [HttpGet]
         public HttpResponseMessage getIISstatus()
         {
             using (var db = new CRMEntities())
             {
                 var res = db.issStatus.Where(g => g.deleted == 0);
                 return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
             }

         }
         [Route("getTvStatus")]
         [HttpGet]
         public HttpResponseMessage getTvStatus()
         {
             using (var db = new CRMEntities())
             {
                 var res = db.TvKullanımıStatus.Where(g => g.deleted == 0);
                 return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
             }

         }

         [Route("getCustomerStatus")]
         [HttpGet]
         public HttpResponseMessage getCustomerStatus()
         {
             using (var db = new CRMEntities())
             {
                 var res = db.customer_status.Where(g => g.deleted == 0);
                 return Request.CreateResponse(HttpStatusCode.OK, res.ToList(), "application/json");
             }

         }
         [Route("getCustomerCard")]
         [HttpGet]
         public HttpResponseMessage getCustomerCard(int custid)
         {
             using (var db = new CRMEntities())
             {
                 var res = db.customer.Where(g => g.deleted == false&& g.customerid==custid);
                 return Request.CreateResponse(HttpStatusCode.OK, res, "application/json");
             }

         }

         [Route("saveCustomerCard")]
         [HttpPost]
         public HttpResponseMessage saveCustomerCard(DTOKatZiyareti ct) {
             using (var db=new CRMEntities())
             {
                 if (db.customer.Any(c => c.customerid == ct.customerid))
                 {
                     var item = db.customer.Where(c => c.customerid == ct.customerid).First();

                     item.customername = ct.customername;
                     item.customersurname = ct.customersurname;
                     item.gsm = ct.gsm;
                     item.netstatu = ct.netstatu;
                     item.telstatu = ct.telstatu;
                     item.iss = ct.iss;
                     item.turkcellTv = ct.turkcellTv;
                     item.tvstatu = ct.tvstatu;
                     item.description = ct.description;
                     item.lastupdated = DateTime.Now;
                     item.updatedby = 9;

                 }
                 if (ct.closedKatZiyareti==true)
                 {
                     var res = db.taskqueue.Where(tq => tq.attachedobjectid == ct.customerid && tq.taskid == 86 && tq.status == null).FirstOrDefault();
                     res.status = 1079;
                 }
                
                 db.SaveChanges();
                 return Request.CreateResponse(HttpStatusCode.OK,"ok","application/json");
             }
         
         }


    }
}