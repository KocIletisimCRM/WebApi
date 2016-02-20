using CRMWebApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Basvuru")]
    public class MailBasvuruController : ApiController
    {
        [Route("insertBasvuru")]
        [HttpPost]
        public HttpResponseMessage insertBasvuru(DTOBasvuru basvuru)
        {
            /*var user = KOCAuthorizeAttribute.getCurrentUser();
            using (var db = new KOCSAMADLSEntities(false))
            {
                adsl_taskqueue taskqueue = new adsl_taskqueue
                {
                    taskid = request.task.taskid,
                    attachedobjectid = request.attachedcustomer.customerid,
                    attachedpersonelid = user.userId,
                    creationdate = DateTime.Now,
                    attachmentdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    updatedby = user.userId,
                    deleted = false
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();*/

            var end = sendemail(basvuru);
            if (end)
                return Request.CreateResponse(HttpStatusCode.OK, basvuru.adsoyad, "application/json");//databaseden gelen basvuruid olacak
            else
                return Request.CreateResponse(HttpStatusCode.NotFound, basvuru.adsoyad, "application/json");
        }

        public bool sendemail(DTOBasvuru info)
        {

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("adslbayidestek@kociletisim.com.tr"); // Mail'in kimden olduğu adresi buraya yazılır.
            mail.Subject = "KOÇ İLETİŞİM MÜŞTERİ BAŞVURU"; // mail'in konusu
            //int customer = Convert.ToInt32(info[0].ToString());
            //int bayiid = Convert.ToInt32(info[1]);

            mail.To.Add("huseyinkoz@kociletisim.com.tr");

            mail.Body = string.Format(@" Merhaba  {0},
                       Başvuru sitemizden form dolduran {1} bilgi için dönüş beklemektedir. İletişim numarası : {2} 'dır. İyi Çalışmalar Dileriz",
                       "Koç İletişim Personeli".ToString(),  //.ToUpper()
                       info.adsoyad,
                       info.gsm);
            // mail'in ana kısmı, içeriği.. 

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587); // gmail üzerinden gönderileceğinden smtp.gmail.com ve onun 587 nolu portu kullanılır.
            smtp.Credentials = new NetworkCredential("yazilimkoc@gmail.com", "612231Tb"); //hangi e-posta üzerinden gönderileceği. E posta, şifre'si yazılır.
            smtp.EnableSsl = true;
            try
            {
                smtp.Send(mail); // mail gönderilir.
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
