using System.Net;
using System.Net.Http;
using System.Web.Http;
using CRMWebApi.Models.Fiber;
using CRMWebApi.DTOs.Fiber;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Login")]
    public class LoginController : ApiController
    {
        [Route("login")]
        [HttpPost]
        public HttpResponseMessage login(DTOpersonel request)
        {
            var userID = 12;
            using (var db = new CRMEntities())
            {
               
                return Request.CreateResponse(HttpStatusCode.OK,"", "application/json");
            }
        }

    }
}
