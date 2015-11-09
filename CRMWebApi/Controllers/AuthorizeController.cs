using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Adsl/Authorize")]
    [KOCAuthorization.KOCAuthorize]
    public class AuthorizeController : ApiController
    {
        [Route("getToken")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage getToken()
        {
            return Request.CreateResponse();
        }

        [Route("getUserInfo")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage getUserInfo()
        {
            return Request.CreateResponse(HttpStatusCode.OK, KOCAuthorization.KOCAuthorizeAttribute.getCurrentUser(),"application/json");
        }
    }
}
