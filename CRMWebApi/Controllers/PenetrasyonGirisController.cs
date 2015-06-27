using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
     [RoutePrefix("api/Penetrasyon")]
    public class PenetrasyonGirisController : ApiController
    {
         [Route("getCustomer")]
         [HttpGet]
         public HttpResponseMessage getCustomer(int siteid,int blockid,string name)
         {
            
             using (var db = new CRMEntities())
             {
                 db.Configuration.AutoDetectChangesEnabled = false;
                 db.Configuration.LazyLoadingEnabled = false;
                 db.Configuration.ProxyCreationEnabled = false;
                 List<object> parameters = new List<object>();
                 parameters.Add(siteid);
                 parameters.Add(blockid);
                 parameters.Add(name);
                 string sqlcommand = @"select s.sitename,b.blockname, c.* from taskqueue tq left join customer c on tq.attachedobjectid=c.customerid
							left join block b on c.blockid=b.blockid
							left join site s on b.siteid=s.siteid
                            where taskid=86 and status is null and tq.creationdate >'2015-01-01 13:32:20.3594437'
                            and c.deleted=0 and tq.deleted=0 
                            and (s.siteid=@siteid or @siteid is null) 
                            and (b.blockid=@blockid or @blockid is null)
                            and (c.customername like CONCAT('%', @name, '%') or @name is null) ";
                 
                 var res=db.customer.SqlQuery( sqlcommand ,parameters).ToList();
                 
                 
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
                 var x = db.block.Where(b => b.siteid == siteid && b.deleted == false).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK, x, "application/json");
             }
         }

    }
}