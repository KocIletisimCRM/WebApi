using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
     [RoutePrefix("api/Nakil")]
    public class CustomerTransferController : ApiController
    {
         [Route("findCustomer")]
         [HttpGet]
         public HttpResponseMessage findCustomer([FromUri]string cname)
        {
            if (cname == null) cname = ""; 

            using (var db = new CRMEntities())
            {           
              var   x = db.customerinfo.Where(c => c.customerinfo1.Contains(cname)).ToList() ;
             return Request.CreateResponse(HttpStatusCode.OK,x, "application/json");
            }            
        }

         [Route("findSite")]
         [HttpGet]
         public HttpResponseMessage findSite()         
         {
             
             using (var db=new CRMEntities())
             {
                 var x = db.site.Where(s =>s.deleted==false).OrderBy(o=>o.sitename).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK,x,"application/json");
             }
         }

         [Route("findBlock")]
         [HttpGet]
         public HttpResponseMessage findBlock(int siteid) 
         {
             if (siteid == null) siteid = 100;
             using (var db=new CRMEntities())
             {
                 var x = db.block.Where(b=> b.siteid==siteid && b.deleted==false).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK, x, "application/json");
             }
         }

         [Route("saveTransfer")]
         [HttpPost]


         [Route("findFlat")]
         [HttpGet]
         public HttpResponseMessage findFlat(int blockid)
         {
             using (var db=new CRMEntities())
             {
                 var res = db.flatinfo.Where(c => c.blockid == blockid).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK,res,"application/json");
             }
         }

         public HttpResponseMessage saveTransfer(object info)
         {
             int oldcustomerid=0;
             string oldflat = "";
             string oldblock= "";

             using (var db=new CRMEntities())
             {
              var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<DTOs.CustomerTransferObject>(info.ToString());
             //eski daireyi siliyoruz.
              foreach (var item in  db.customer.Where(c=> c.blockid==obj.blockid && c.flat==obj.flat))
              {
                  item.deleted = true;
                  oldcustomerid = item.customerid;
                  
                  
              }
              //yeni kişinin dairesini güncelle
              foreach (var item in db.customer.Where(c => c.customerid==obj.customerid))
              {
                  oldflat = item.flat;
                  oldblock = item.blockid.ToString();
                  item.blockid = obj.blockid;
                  item.flat = obj.flat;


              }
                 //eski müşteriye ait kat ziyaretlerini sildim.
              foreach (var item in db.taskqueue.Where(tq=>tq.attachedobjectid==oldcustomerid && tq.taskid==86))
              {
                  item.deleted = true;
                  
              }
                 //taşınılan daireye yeni müşteri oluştur (boş kayıt)
              string sql = @"INSERT INTO customer (blockid,flat,deleted,creationdate,lastupdated,updatedby)
                                                       VALUES ({0},{1},0,GETDATE(),GETDATE(),'9')";
              string querySQL = string.Format(sql, oldblock,oldflat);
              var res = db.Database.ExecuteSqlCommand(querySQL);
            
              db.SaveChanges();
              return null;
             }
            
         }

         public void insert_customer(customer c){

          using (var db = new CRMEntities())
                {
                    c.lastupdated = DateTime.Now;
                    c.creationdate = DateTime.Now;
                    c.deleted = false;
                    //User Control
                    db.customer.Add(c);
                    db.SaveChanges();
          
                }
         }

         //[Route("findCustomer")]
         //[HttpGet]
         //public HttpResponseMessage _findCustomer([FromUri]string cname)
         //{
         //    return findCustomer(cname);
         //}
    }
}
