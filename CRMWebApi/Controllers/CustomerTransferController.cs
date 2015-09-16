// compile with: /doc:DocFileName.xml
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
              var   x = db.customerinfo.Where(c => c.customerinfo1.Contains(cname)).OrderBy(o=>o.customerinfo1).ToList() ;
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
         public HttpResponseMessage findBlock(int ? siteid) 
         {
             if (siteid == null) siteid = 100;
             using (var db=new CRMEntities())
             {
                 var x = db.block.Where(b=> b.siteid==siteid && b.deleted==false).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK, x, "application/json");
             }
         }

        


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

         [Route("saveTransfer")]
         [HttpPost]
         public HttpResponseMessage saveTransfer(object info)
         {
             int oldcustomerid=0;
             string oldflat = "";
             int? oldblock= null;
             int? oldpersonelid = null;
       
             using (var db=new CRMEntities())
             {
              var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<DTOs.CustomerTransferObject>(info.ToString());
             //eski daireyi siliyoruz.
              foreach (var item in  db.customer.Where(c=> c.blockid==obj.blockid && c.flat==obj.flat))
              {
                  item.deleted = true;
                  oldcustomerid = item.customerid;
              }
              //eski müşteriye ait kat ziyaretlerini sildim.
              foreach (var item in db.taskqueue.Where(tq => tq.attachedobjectid == oldcustomerid && tq.taskid == 86))
              {
                  item.deleted = true;

              }
              //yeni kişinin dairesini güncelle
              foreach (var item in db.customer.Where(c => c.customerid==obj.customerid))
              {
                  oldflat = item.flat;
                  oldblock = item.blockid;
                  item.blockid = obj.blockid;
                  item.flat = obj.flat;
              }
               
              //taşınılan daireye yeni müşteri oluştur (boş kayıt)
              string sql = @"INSERT INTO customer (blockid,flat,deleted,creationdate,lastupdated,updatedby)
                                                       VALUES ({0},{1},0,GETDATE(),GETDATE(),'9')";
              string querySQL = string.Format(sql, oldblock,oldflat);
              var res = db.Database.ExecuteSqlCommand(querySQL);

               //boşaltılan daireye ait kat ziyareti oluşturmak için eski bilgiler çekiliyor.
              var newcustomerid = db.customer.Where(c => c.blockid == oldblock).Max(m=>m.customerid) ;
                 
              var oldpersonelinfo = db.taskqueue.Where(t => t.attachedobjectid == obj.customerid && t.taskid == 86).OrderByDescending(o => o.taskorderno).FirstOrDefault();
              if (oldpersonelinfo!=null)//eğer kat ziyaret taskı daha önce o kişiye oluşturulmuşsa sildiğimiz için yeniden ekliyoruz.
              {
                  oldpersonelid = oldpersonelinfo.attachedpersonelid;
                  string tqsql = @"INSERT INTO taskqueue(taskid,creationdate,attachedobjectid,attachedpersonelid,attachmentdate,lastupdated,description,updatedby,deleted)
                           VALUES (86,GETDATE(),{0},{1},GETDATE(),GETDATE(),'nakil işlemi sonucu oluşturulan kat ziyareti taskı',9,0)";
                  string tqquerysql = string.Format(tqsql, newcustomerid, oldpersonelid);
                  var res2 = db.Database.ExecuteSqlCommand(tqquerysql);
              }
          
            
              db.SaveChanges();
              return Request.CreateResponse(HttpStatusCode.OK, "Tasima İslemi Gerçekleştirildi", "application/json");
             }
            
         }

         [Route("pasiveCustomer")]
         [HttpGet]
         public HttpResponseMessage pasiveCustomer([FromUri]int custid) 
         {
             int? oldblock = null; string oldflat = null; int? oldpersonelid = null;
            
             using (var db= new CRMEntities())
             {
                 var oldcustomer = db.customer.Where(c => c.customerid == custid).FirstOrDefault();
                 oldblock = oldcustomer.blockid;
                 oldflat = oldcustomer.flat;
                 oldcustomer.deleted = true;

                 string sql = @"INSERT INTO customer (blockid,flat,deleted,creationdate,lastupdated,updatedby)
                                VALUES ({0},{1},0,GETDATE(),GETDATE(),'9')";
                 var newcustomerid = db.customer.Where(c => c.blockid == oldblock).Max(m => m.customerid);


                 var oldpersonelinfo = db.taskqueue.Where(t => t.attachedobjectid == custid && t.taskid == 86).OrderByDescending(o => o.taskorderno).FirstOrDefault();
                 if (oldpersonelinfo != null)//eğer kat ziyaret taskı daha önce o kişiye oluşturulmuşsa sildiğimiz için yeniden ekliyoruz.
                 {
                     oldpersonelid = oldpersonelinfo.attachedpersonelid;
                     string tqsql = @"INSERT INTO taskqueue(taskid,creationdate,attachedobjectid,attachedpersonelid,attachmentdate,lastupdated,description,updatedby,deleted)
                           VALUES (86,GETDATE(),{0},{1},GETDATE(),GETDATE(),'Müşteri pasife çekildikten sonra oluşan kat ziyareti',9,0)";
                     string tqquerysql = string.Format(tqsql, newcustomerid, oldpersonelid);
                     var res2 = db.Database.ExecuteSqlCommand(tqquerysql);
                 }
                 string querysql = string.Format(sql,oldblock,oldflat);
                 var res = db.Database.ExecuteSqlCommand(querysql);

                 string updatesql = @"update taskqueue set deleted=1 where attachedobjectid={0} and status is null";
                 updatesql = string.Format(updatesql,custid);
                 var result = db.Database.ExecuteSqlCommand(updatesql);

                 db.SaveChanges();
                 return Request.CreateResponse(HttpStatusCode.OK,"Müşteriyi Pasife Çektiniz.","application/json");
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
