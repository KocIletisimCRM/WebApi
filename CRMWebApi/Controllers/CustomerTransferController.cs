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
         [HttpPost]
        public HttpResponseMessage findCustomer(string cname)
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
         public HttpResponseMessage findSite(string sitename)         
         {
             if (sitename == null) sitename = "";
             using (var db=new CRMEntities())
             {
                 var x = db.site.Where(s => s.sitename.Contains(sitename)).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK,x,"application/json");
             }
         }

         [Route("findBlock")]
         [HttpGet]
         public HttpResponseMessage findBlock(int siteid, string blockname) 
         {
             if (blockname == null) blockname = "";
             using (var db=new CRMEntities())
             {
                 var x = db.block.Where(b => b.blockname.Contains(blockname) && b.siteid==siteid ).ToList();
                 return Request.CreateResponse(HttpStatusCode.OK, x, "application/json");
             }
         }



         [Route("findCustomer")]
         [HttpGet]
         public HttpResponseMessage _findCustomer([FromUri]string cname)
         {
             return findCustomer(cname);
         }
    }
}
