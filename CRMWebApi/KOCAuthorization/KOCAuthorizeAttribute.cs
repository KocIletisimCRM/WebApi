using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using CRMWebApi.Models.Adsl;
using CRMWebApi.Models.Fiber;
using System.Linq;
using System.Collections.Concurrent;
using System;
using System.Net.Http;

namespace CRMWebApi.KOCAuthorization
{
    public class KOCAuthorizeAttribute : AuthorizeAttribute
    {
        private static ConcurrentDictionary<string, KOCAuthorizedUser> ActiveUsers = new ConcurrentDictionary<string, KOCAuthorizedUser>();

        private static string createToken()
        {
            return System.Convert.ToBase64String(
                Guid.NewGuid().ToByteArray().Union(
                    Guid.NewGuid().ToByteArray().Union(
                        Guid.NewGuid().ToByteArray()
                    )
                ).ToArray()
            );
        }

        public static KOCAuthorizedUser getCurrentUser()
        {
            KOCAuthorizedUser user;
            ActiveUsers.
                Where(u => (DateTime.Now - u.Value.lastActivityTime) > SessionTimeout).
                Select(u => u.Key).ToList().AsParallel().ForAll(t =>
                {
                    ActiveUsers.TryRemove(t, out user);
                }); 
            ActiveUsers.TryGetValue(Token, out user);
            if (user != null) user.lastActivityTime = DateTime.Now;
            return user;
        }

        private static TimeSpan SessionTimeout = TimeSpan.FromMinutes(20);
        private static string UserName { get { return HttpContext.Current.Request.Headers[userNameHeader]; } }
        private static string Password { get { return HttpContext.Current.Request.Headers[passwordHeader]; } }
        private static string UserType { get { return HttpContext.Current.Request.Headers[userTypeHeader]; } }
        private static string Token { get { return HttpContext.Current.Request.Headers[authorizedTokenHeader]; } }

        private string ErrorMessage { get; set; }
        public static string userNameHeader = "X-KOC-UserName";
        public static string passwordHeader = "X-KOC-Pass";
        public static string userTypeHeader = "X-KOC-UserType";
        public static string authorizedTokenHeader = "X-KOC-Token";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var userType = UserType;
            var token = Token;
            var pass = HttpContext.Current.Request.Headers[passwordHeader];

            if (string.IsNullOrWhiteSpace(userType)) // yetkilendirilmiş istek
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage = "Yetki anahtarı belirtilmemiş...";
                    HandleUnauthorizedRequest(actionContext);
                    return;
                }
                else if (!ActiveUsers.ContainsKey(token))
                {
                    ErrorMessage = "Geçersiz yetki anahtarı...";
                    HandleUnauthorizedRequest(actionContext);
                    return;
                }
            }
            else
            {
                if (userType.Trim().ToUpper() == "ADSL")
                {
                    using (var db = new KOCSAMADLSEntities())
                    {
                       
                        var emails = db.personel.Where(p=>p.deleted==false && p.email!=null).ToList();

                            var user = emails.Where(p => p.email.ToString().Split(';')[0] == UserName && p.password == Password).FirstOrDefault();

                            #region girişi kontrol et
                            if (user != null)
                            {
                                token = createToken();
                                ActiveUsers[token] = new KOCAuthorizedUser
                                {
                                    userId = user.personelid,
                                    userName = user.email,
                                    userFullName = user.personelname,
                                    userRole = user.roles,
                                    lastActivityTime = DateTime.Now,
                                    creationTime = DateTime.Now
                                };
                                HttpContext.Current.Response.Headers.Add(authorizedTokenHeader, token);

                            }
                            else
                            {
                                ErrorMessage = "Kullanıcı bilgileri hatalı...";
                                HandleUnauthorizedRequest(actionContext);
                            }
                            #endregion
                        }
                }
                else if(userType.Trim().ToUpper() == "FIBER")
                {
                    using (var db = new CRMEntities())
                    {
                        var user = db.personel.Where(p => p.deleted==false &&p.email == UserName && p.password == Password).FirstOrDefault();
                        if (user != null)
                        {
                            token = createToken();
                            ActiveUsers[token] = new KOCAuthorizedUser
                            {
                                userId = user.personelid,
                                userName = user.email,
                                userFullName = user.personelname,
                                userRole = (int)user.roles,
                                lastActivityTime = DateTime.Now,
                                creationTime = DateTime.Now
                            };
                            HttpContext.Current.Response.Headers.Add(authorizedTokenHeader, token);
                        }
                        else
                        {
                            ErrorMessage = "Kullanıcı bilgileri hatalı...";
                            HandleUnauthorizedRequest(actionContext);
                        }
                    }
                }
            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(new { loginError = ErrorMessage });
        }
    }
}