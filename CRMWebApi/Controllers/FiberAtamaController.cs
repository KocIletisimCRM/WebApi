using CRMWebApi.DTOs.Fiber;
using CRMWebApi.DTOs.Fiber.DTORequestClasses;
using CRMWebApi.Models.Fiber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRMWebApi.Controllers
{
    [RoutePrefix("api/Fiber/Atama")]
    public class FiberAtamaController : ApiController
    {  // otomatik zorunlu taskların personele atanması için oluşturulan/oluşturulacak kuralların uygulanması için yapıldı
        [Route("getTaskPersonelAtama")]
        [HttpPost]
        public HttpResponseMessage getTaskPersonelAtama(DTOFilterGetAtamaRequest request)
        { //Otomatik personel atama kurallarını getir
            using (var db = new CRMEntities())
            {
                var filter = request.getFilter();
                var countSql = filter.getCountSQL();
                var rowCount = db.Database.SqlQuery<int>(countSql).First();
                var querySql = filter.getPagingSQL(request.pageNo, request.rowsPerPage);
                var res = db.atama.SqlQuery(querySql).ToList();

                var offpersonelcall = res.Select(s => s.offpersonel).Distinct().ToList();
                var offpersonels = db.personel.Where(s => offpersonelcall.Contains(s.personelid)).ToList();

                var appointpersonelcall = res.Select(s => s.appointedpersonel).Distinct().ToList();
                var appointpersonels = db.personel.Where(s => appointpersonelcall.Contains(s.personelid)).ToList();

                var closedtaskcall = res.Select(s => s.closedtask).Distinct().ToList();
                var closedtasks = db.task.Where(s => closedtaskcall.Contains(s.taskid)).ToList();

                var formedtaskcall = res.Select(s => s.formedtask).Distinct().ToList();
                var formedtasks = db.task.Where(s => formedtaskcall.Contains(s.taskid)).ToList();

                var closedtasktypecall = res.Select(s => s.closedtasktype).Distinct().ToList();
                var closedtasktypes = db.tasktypes.Where(s => closedtasktypecall.Contains(s.TaskTypeId)).ToList();

                var formedtasktypecall = res.Select(s => s.formedtasktype).Distinct().ToList();
                var formedtasktypes = db.tasktypes.Where(s => formedtasktypecall.Contains(s.TaskTypeId)).ToList();

                res.ForEach(s =>
                {
                    s.personeloff = offpersonels.Where(i => i.personelid == s.offpersonel).FirstOrDefault();
                    s.personelappointed = appointpersonels.Where(i => i.personelid == s.appointedpersonel).FirstOrDefault();
                    s.taskclosed = closedtasks.Where(p => p.taskid == s.closedtask).FirstOrDefault();
                    s.taskformed = formedtasks.Where(p => p.taskid == s.formedtask).FirstOrDefault();
                    s.tasktypesclosed = closedtasktypes.Where(p => p.TaskTypeId == s.closedtasktype).FirstOrDefault();
                    s.tasktypesformed = formedtasktypes.Where(p => p.TaskTypeId == s.formedtasktype).FirstOrDefault();
                });
                DTOResponsePagingInfo paginginfo = new DTOResponsePagingInfo
                {
                    pageCount = (int)Math.Ceiling(rowCount * 1.0 / request.rowsPerPage),
                    pageNo = request.pageNo,
                    rowsPerPage = request.rowsPerPage,
                    totalRowCount = rowCount
                };
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DTOPagedResponse(DTOResponseError.NoError(), res.Select(r => r.toDTO()).ToList(), paginginfo, querySql)
                    , "application/json");
            }
        }
        [Route("insertPersonelAtama")]
        [HttpPost]
        public HttpResponseMessage insertPersonelAtama(DTOAtama request)
        {  // yeni otomatik taska personel atama kuralı
            using (var db = new CRMEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "İşlem Başarılı" };
                    int arrayLenght;
                    if (request.formedtaskarray != null)
                        arrayLenght = request.formedtaskarray.Length;
                    else
                        arrayLenght = 1;
                    for (int i = 0; i < arrayLenght; i++)
                    {
                        var p = new atama
                        {
                            closedtasktype = request.closedtasktype,
                            closedtask = request.closedtask,
                            offpersonel = request.offpersonel,
                            formedtasktype = request.formedtasktype,
                            formedtask = request.formedtaskarray != null ? request.formedtaskarray[i] : null,
                            appointedpersonel = request.appointedpersonel,
                        };
                        db.atama.Add(p);
                    }
                    db.SaveChanges();
                    tran.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "Hata Oluştu" };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
        }
        [Route("updatePersonelAtama")]
        [HttpPost]
        public HttpResponseMessage updatePersonelAtama(DTOAtama request)
        {
            using (var db = new CRMEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var upa = db.atama.Where(r => r.id == request.id).FirstOrDefault();
                    upa.closedtasktype = request.closedtasktype;
                    upa.closedtask = request.closedtask;
                    upa.offpersonel = request.offpersonel;
                    upa.formedtasktype = request.formedtasktype;
                    upa.formedtask = request.formedtask;
                    upa.appointedpersonel = request.appointedpersonel;
                    db.SaveChanges();
                    tran.Commit();
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "Atama Düzenleme Tamamlandı." };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "Atama Düzenleme Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                }
        }
        [Route("deletePersonelAtama")]
        [HttpPost]
        public HttpResponseMessage deletePersonelAtama(int id)
        {
            using (var db = new CRMEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var upa = db.atama.Where(r => r.id == id).FirstOrDefault();
                    //delete
                    db.SaveChanges();
                    tran.Commit();
                    var errormessage = new DTOResponseError { errorCode = 1, errorMessage = "işlem Gerçekleştirildi." };
                    return Request.CreateResponse(HttpStatusCode.OK, errormessage, "application/json");
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    var errormessage = new DTOResponseError { errorCode = 2, errorMessage = "işlem Tamamlanamadı!" };
                    return Request.CreateResponse(HttpStatusCode.NotModified, errormessage, "application/json");
                }
        }

    }
}
