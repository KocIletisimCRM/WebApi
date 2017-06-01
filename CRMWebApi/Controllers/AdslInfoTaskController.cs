using CRMWebApi.DTOs.Adsl;
using CRMWebApi.DTOs.Adsl.DTORequestClasses;
using CRMWebApi.Models.Adsl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("CustomerInfos")]
    public class AdslInfoTaskController : ApiController
    {
        public static string userNameHeader = "X-KOC-UserName";
        public static string passwordHeader = "X-KOC-Pass";
        public static string userTypeHeader = "X-KOC-UserType";
        public static string userHeader = "email";
        public static string passHeader = "parola";

        string clean(string val)
        {
            if (string.IsNullOrEmpty(val)) return string.Empty;
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(val, "");
        }

        string isgsm(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return string.Empty;
            Regex digitsOnly = new Regex(@"[^\d]");
            var ph = digitsOnly.Replace(phone, "");
            if (ph.StartsWith("905") && ph.Length == 12) return ph;
            else if (ph.StartsWith("05") && ph.Length == 11) return $"9{ph}";
            else if (ph.StartsWith("5") && ph.Length == 10) return $"90{ph}";
            return string.Empty;
        }

        private bool checktc(string tc)
        {
            using (var db = new KOCSAMADLSEntities())
                return db.customer.FirstOrDefault(r => r.tc == tc && r.deleted == false) == null;
        }

        [Route("Infos"), HttpPost]
        public HttpResponseMessage Infos(InfoRequest info)
        {
            if (info == null) return Request.CreateResponse(HttpStatusCode.OK, new { code = 1, message = "Eksik Bilgi." }, "application/json");
            var mail = HttpContext.Current.Request.Headers[userHeader];
            var pass = HttpContext.Current.Request.Headers[passHeader];
            using (var db = new KOCSAMADLSEntities())
            {
                var user = db.personel.Where(r => r.email == mail && r.password == pass).FirstOrDefault();
                if (user == null) return Request.CreateResponse(HttpStatusCode.OK, new { code = 1, message = "Personel Bilgileri Hatalıdır." }, "application/json");

                info.gsm = isgsm(info.gsm);
                info.tc = clean(info.tc);
                info.telefon = clean(info.telefon);
                if (string.IsNullOrEmpty(info.tc) || string.IsNullOrEmpty(info.gsm) || string.IsNullOrEmpty(info.adsoyad) || string.IsNullOrEmpty(info.adres) || string.IsNullOrEmpty(info.fault) || info.il == null || info.ilce == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { code = 1, message = "Eksik Bilgi." }, "application/json");

                // TC Kimlik tekil durum kaldırıldığında değişecek
                var tckimlik = info.tc;
                while (true)
                {
                    if (checktc(tckimlik))
                        break;
                    else
                        tckimlik = tckimlik.Substring(0, tckimlik.Length - 5) + (Convert.ToInt32(tckimlik.Substring(tckimlik.Length - 2, 2)) + 1);
                }
                if (tckimlik != info.tc)
                {
                    info.adres = "TC : " + info.tc + " " + info.adres;
                    info.tc = tckimlik;
                }

                var customer = new customer
                {
                    customername = info.adsoyad.ToUpper(),
                    tc = info.tc,
                    gsm = info.gsm,
                    phone = info.telefon,
                    ilKimlikNo = info.il,
                    ilceKimlikNo = info.ilce,
                    bucakKimlikNo = info.bucak,
                    mahalleKimlikNo = info.mahalle,
                    yolKimlikNo = 61,
                    binaKimlikNo = 61,
                    daire = 61,
                    updatedby = user.personelid,
                    description = info.adres,
                    lastupdated = DateTime.Now,
                    creationdate = DateTime.Now,
                    deleted = false,
                    email = info.email,
                };
                db.customer.Add(customer);
                db.SaveChanges();

                var taskqueue = new adsl_taskqueue
                {
                    appointmentdate = DateTime.Now,
                    attachedobjectid = customer.customerid,
                    attachedpersonelid = user.personelid,
                    attachmentdate = DateTime.Now,
                    creationdate = DateTime.Now,
                    deleted = false,
                    description = info.taskdetay,
                    lastupdated = DateTime.Now,
                    status = null,
                    taskid = 1219, // Akıllı Nokta Data bilgilendirme Taskı
                    updatedby = user.personelid,
                    fault = info.fault
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.10.2016
                db.SaveChanges();
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, message = "OK" }, "application/json");
        }

        [Route("Adres"), HttpPost, HttpGet]
        public async Task<HttpResponseMessage> Adres(AddressContent content)
        {
            var mail = HttpContext.Current.Request.Headers[userHeader];
            var pass = HttpContext.Current.Request.Headers[passHeader];
            using (var db = new KOCSAMADLSEntities())
            {
                var user = db.personel.Where(r => r.email == mail && r.password == pass).FirstOrDefault();
                if (user == null) return Request.CreateResponse(HttpStatusCode.OK, new { code = 1, message = "Personel Bilgileri Hatalıdır." }, "application/json");
                Request.Headers.Add(userNameHeader, mail);
                Request.Headers.Add(passwordHeader, pass);
                Request.Headers.Add(userTypeHeader, "ADSL");
                ADSLAddressController adres = new ADSLAddressController();
                adres.Request = new HttpRequestMessage();
                adres.Request.Headers.Add(userNameHeader, mail);
                adres.Request.Headers.Add(passwordHeader, pass);
                adres.Request.Headers.Add(userTypeHeader, "ADSL");
                adres.RequestContext = RequestContext;
                adres.Request.Method = Request.Method;
                adres.Request.Content = Request.Content;
                return await adres.getAdress(new DTOGetAdressFilter { adres = new DTOFieldFilter { fieldName = content.fieldName, op = content.op, value = content.value } });
            }
        }
    }

    public class InfoRequest
    {
        public string tc { get; set; }
        public string adsoyad { get; set; } // customername
        public string gsm { get; set; }
        public string telefon { get; set; } // phone
        public int? il { get; set; } // ilKimlikNo
        public int? ilce { get; set; } // ilceKimlikNo
        public int? bucak { get; set; } // bucakKimlikNo
        public int? mahalle { get; set; } // mahalleKimlikNo
        public string email { get; set; }
        public string taskdetay { get; set; } // taskdescription
        public string adres { get; set; } //description
        public string fault { get; set; }
    }

    public class AddressContent
    {
        public string fieldName { get; set; }
        public int op { get; set; }
        public object value { get; set; }
    }
}
