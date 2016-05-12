using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Teknar_Proxy_Lib;

namespace CRMWebApi
{
    public static class WebApiConfig
    {
        public static SemaphoreSlim fLockObject = new SemaphoreSlim(1);
        public static DateTime FiberLastUpdated = new DateTime(1900, 1, 1);
        public static Dictionary<int, DTOs.Fiber.KocFiberProccess> FiberProccesses = new Dictionary<int, DTOs.Fiber.KocFiberProccess>();
        public static Dictionary<int, int> FiberProccessIndexes = new Dictionary<int, int>();
        public static Dictionary<int, DTOs.Fiber.DTOtaskqueue> FiberTaskQueues = new Dictionary<int, DTOs.Fiber.DTOtaskqueue>();
        public static Dictionary<int, DTOs.Fiber.DTOcustomer> FiberCustomers = new Dictionary<int, DTOs.Fiber.DTOcustomer>();
        public static Dictionary<int, DTOs.Fiber.DTOtask> FiberTasks = new Dictionary<int, DTOs.Fiber.DTOtask>();
        public static Dictionary<int, DTOs.Fiber.DTOpersonel> FiberPersonels = new Dictionary<int, DTOs.Fiber.DTOpersonel>();
        public static Dictionary<int, DTOs.Fiber.DTOtaskstatepool> FiberStatus = new Dictionary<int, DTOs.Fiber.DTOtaskstatepool>();

        public static async Task loadFiberTaskQueues(DateTime lastUpdated)
        {
            try
            {
                using (var db = new Models.Fiber.CRMEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    using (var conn = db.Database.Connection as SqlConnection)
                    {
                        await conn.OpenAsync().ConfigureAwait(false);
                        var selectCommand = conn.CreateCommand();
                        selectCommand.CommandText = $"select * from taskqueue where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd")}'";
                        using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                        {
                            while (await sqlreader.ReadAsync().ConfigureAwait(false))
                            {
                                var t = (new Models.Fiber.taskqueue
                                {
                                    taskorderno = (int)sqlreader[0],
                                    taskid = Convert.ToInt32(sqlreader[1]),
                                    previoustaskorderid = sqlreader.IsDBNull(2) ? null : (int?)sqlreader[2],
                                    relatedtaskorderid = sqlreader.IsDBNull(3) ? null : (int?)sqlreader[3],
                                    creationdate = sqlreader.IsDBNull(4) ? null : (DateTime?)sqlreader[4],
                                    attachedobjectid = sqlreader.IsDBNull(5) ? null : (int?)sqlreader[5],
                                    attachmentdate = sqlreader.IsDBNull(6) ? null : (DateTime?)sqlreader[6],
                                    attachedpersonelid = sqlreader.IsDBNull(7) ? null : (int?)sqlreader[7],
                                    appointmentdate = sqlreader.IsDBNull(8) ? null : (DateTime?)sqlreader[8],
                                    status = sqlreader.IsDBNull(9) ? null : (int?)sqlreader[9],
                                    consummationdate = sqlreader.IsDBNull(10) ? null : (DateTime?)sqlreader[10],
                                    description = sqlreader.IsDBNull(11) ? null : (string)sqlreader[11],
                                    lastupdated = sqlreader.IsDBNull(12) ? null : (DateTime?)sqlreader[12],
                                    updatedby = sqlreader.IsDBNull(13) ? null : (int?)sqlreader[13],
                                    deleted = sqlreader.IsDBNull(14) ? null : (bool?)sqlreader[14],
                                    assistant_personel = sqlreader.IsDBNull(15) ? null : (int?)sqlreader[15],
                                    fault = sqlreader.IsDBNull(16) ? null : (string)sqlreader[16],
                                });
                                var tDTO = (DTOs.Fiber.DTOtaskqueue)t.toDTO();
                                if (t.deleted == true)
                                {
                                    if (FiberTaskQueues.ContainsKey(t.taskorderno))
                                        FiberTaskQueues.Remove(t.taskorderno);
                                }
                                else {
                                    try
                                    {
                                        tDTO.task = FiberTasks[t.taskid];
                                        //if (t.status != null)
                                        //    tDTO.taskstatepool = FiberStatus[t.status.Value];
                                        FiberTaskQueues[t.taskorderno] = tDTO;
                                        if (t.previoustaskorderid == null)
                                        {
                                            if (tDTO.task.tasktypes.TaskTypeId == 1)
                                            {
                                                FiberProccesses.Add(t.taskorderno, new DTOs.Fiber.KocFiberProccess { S_TON = t.taskorderno });
                                            }
                                            FiberProccessIndexes[t.taskorderno] = t.taskorderno;
                                        }
                                        else {
                                            var proccessNo = FiberProccessIndexes[t.previoustaskorderid.Value];
                                            FiberProccessIndexes[t.taskorderno] = proccessNo;
                                            if (FiberProccesses.ContainsKey(proccessNo))
                                                FiberProccesses[proccessNo].Update(tDTO);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public static async Task loadFiberCustomers(DateTime lastUpdated)
        {
            using (var db = new Models.Fiber.CRMEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select customerid,tckimlikno,customername, customersurname,lastupdated from customer where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Fiber.customer
                            {
                                customerid = (int)sqlreader[0],
                                tckimlikno = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                customername = sqlreader.IsDBNull(2) ? null : (string)sqlreader[2],
                                customersurname = sqlreader.IsDBNull(3) ? null : (string)sqlreader[3],
                                lastupdated = sqlreader.IsDBNull(4) ? null : (DateTime?)sqlreader[4],
                            }).toDTO<DTOs.Fiber.DTOcustomer>();
                            FiberCustomers[t.customerid] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadFiberTasks(DateTime lastUpdated)
        {
            using (var db = new Models.Fiber.CRMEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select taskid,taskname,tasktype,lastupdated from task where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Fiber.task
                            {
                                taskid = (int)sqlreader[0],
                                taskname = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                tasktype = (int)sqlreader[2],
                                lastupdated = sqlreader.IsDBNull(3) ? null : (DateTime?)sqlreader[3],
                            });
                            var tDTO = t.toDTO<DTOs.Fiber.DTOtask>();
                            tDTO.tasktypes = new DTOs.Fiber.DTOTaskTypes { TaskTypeId = t.tasktype };
                            FiberTasks[t.taskid] = tDTO;
                        }
                    }
                }
            }
        }
        public static async Task loadFiberPersonels(DateTime lastUpdated)
        {
            using (var db = new Models.Fiber.CRMEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select personelid,personelname,lastupdated from personel where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Fiber.personel
                            {
                                personelid = (int)sqlreader[0],
                                personelname = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                lastupdated = sqlreader.IsDBNull(2) ? null : (DateTime?)sqlreader[2],
                            }).toDTO<DTOs.Fiber.DTOpersonel>();
                            FiberPersonels[t.personelid] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadFiberStatus(DateTime lastUpdated)
        {
            using (var db = new Models.Fiber.CRMEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select taskstateid,taskstate,lastupdated from taskstatepool where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Fiber.taskstatepool
                            {
                                taskstateid = (int)sqlreader[0],
                                taskstate = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                lastupdated = sqlreader.IsDBNull(2) ? null : (DateTime?)sqlreader[2],
                            }).toDTO<DTOs.Fiber.DTOtaskstatepool>();
                            FiberStatus[t.taskstateid] = t;
                        }
                    }
                }
            }
        }

        public static async Task updateFiberData()
        {
            await fLockObject.WaitAsync().ConfigureAwait(false);
            var lastUpdated = DateTime.Now;
            await Task.WhenAll(new Task[] {
                loadFiberCustomers(FiberLastUpdated),
                loadFiberPersonels(FiberLastUpdated),
                loadFiberTasks(FiberLastUpdated),
                loadFiberStatus(FiberLastUpdated)
            }).ConfigureAwait(false);
            await loadFiberTaskQueues(FiberLastUpdated).ConfigureAwait(false);
            FiberLastUpdated = lastUpdated;
            fLockObject.Release();
        }

        public static SemaphoreSlim aLockObject = new SemaphoreSlim(1);
        public static DateTime AdslLastUpdated = new DateTime(1900, 1, 1);
        public static Dictionary<int, DTOs.Adsl.KocAdslProccess> AdslProccesses = new Dictionary<int, DTOs.Adsl.KocAdslProccess>();
        public static Dictionary<int, int> AdslProccessIndexes = new Dictionary<int, int>();
        public static Dictionary<int, Models.Adsl.adsl_taskqueue> AdslTaskQueues = new Dictionary<int, Models.Adsl.adsl_taskqueue>();
        public static Dictionary<int, Models.Adsl.customer> AdslCustomers = new Dictionary<int, Models.Adsl.customer>();
        public static Dictionary<int, Models.Adsl.adsl_task> AdslTasks = new Dictionary<int, Models.Adsl.adsl_task>();
        public static Dictionary<int, Models.Adsl.adsl_personel> AdslPersonels = new Dictionary<int, Models.Adsl.adsl_personel>();
        public static Dictionary<int, Models.Adsl.adsl_taskstatepool> AdslStatus = new Dictionary<int, Models.Adsl.adsl_taskstatepool>();
        public static Dictionary<int, Models.Adsl.il> AdslIls = new Dictionary<int, Models.Adsl.il>(); // Raporda müşterinin ilini göstermek için
        public static Dictionary<int, Models.Adsl.ilce> AdslIlces = new Dictionary<int, Models.Adsl.ilce>(); // Raporda müşterinin ilcesini göstermek için
        public static Dictionary<int, Models.Adsl.adsl_tasktypes> AdslTaskTypes = new Dictionary<int, Models.Adsl.adsl_tasktypes>(); // startsProccess almak için (startsproccess : başlangıç taskı mı kontrolü)
        public static Dictionary<int, Models.Adsl.paymentsystemtype> AdslPaymentSystemType = new Dictionary<int, Models.Adsl.paymentsystemtype>(); // Hakediş hesaplama için hakediş tipleri
        public static Dictionary<int, Models.Adsl.paymentsystem> AdslPaymentSystem = new Dictionary<int, Models.Adsl.paymentsystem>(); // Hakediş hesaplama için satış prim ve servis hakedişleri
        public static Dictionary<int, DTOs.Adsl.DTOSL> AdslSl = new Dictionary<int, DTOs.Adsl.DTOSL>();
        //Key: Taskid, <Key: 0-3 (0: bayi sl başlangıç, 1: Bayi sl bitiş, 2: Koç SL Başlangıç, 3: Koç SL Bitiş), <SL id>>
        public static Dictionary<int, Dictionary<int, List<int>>> AdslTaskSl = new Dictionary<int, Dictionary<int, List<int>>>();
        public static Dictionary<int, Models.Adsl.adsl_objecttypes> AdslObjectTypes = new Dictionary<int, Models.Adsl.adsl_objecttypes>(); // personel görev tanımlamalrı için

        public static async Task loadAdslTaskQueues(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select * from taskqueue where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'"; // aynı gün içerisinde yapılan değişiklikler işleme alınamadığı için HH:mm:ss eklendi (Hüseyin KOZ)
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        var proccesIds = new HashSet<int>();
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_taskqueue
                            {
                                taskorderno = (int)sqlreader[0],
                                taskid = Convert.ToInt32(sqlreader[1]),
                                previoustaskorderid = sqlreader.IsDBNull(2) ? null : (int?)sqlreader[2],
                                relatedtaskorderid = sqlreader.IsDBNull(3) ? null : (int?)sqlreader[3],
                                creationdate = sqlreader.IsDBNull(4) ? null : (DateTime?)sqlreader[4],
                                attachedobjectid = sqlreader.IsDBNull(5) ? null : (int?)sqlreader[5],
                                attachmentdate = sqlreader.IsDBNull(6) ? null : (DateTime?)sqlreader[6],
                                attachedpersonelid = sqlreader.IsDBNull(7) ? null : (int?)sqlreader[7],
                                appointmentdate = sqlreader.IsDBNull(8) ? null : (DateTime?)sqlreader[8],
                                status = sqlreader.IsDBNull(9) ? null : (int?)sqlreader[9],
                                consummationdate = sqlreader.IsDBNull(10) ? null : (DateTime?)sqlreader[10],
                                description = sqlreader.IsDBNull(11) ? null : (string)sqlreader[11],
                                lastupdated = sqlreader.IsDBNull(12) ? null : (DateTime?)sqlreader[12],
                                updatedby = sqlreader.IsDBNull(13) ? null : (int?)sqlreader[13],
                                deleted = sqlreader.IsDBNull(14) ? null : (bool?)sqlreader[14],
                                assistant_personel = sqlreader.IsDBNull(15) ? null : (int?)sqlreader[15],
                                fault = sqlreader.IsDBNull(16) ? null : (string)sqlreader[16],
                            });
                            if (t.deleted == true)
                            {
                                if (AdslTaskQueues.ContainsKey(t.taskorderno))
                                {
                                    //Derinlemesine temizlik :) Silinen task ve bu taska bağlı tüm taskları listeden çıkart
                                    if (!proccesIds.Contains(t.taskorderno)) proccesIds.Add(t.taskorderno);
                                    var queue = new Queue<int>();
                                    queue.Enqueue(t.taskorderno);
                                    while (queue.Count > 0)
                                    {
                                        var ton = queue.Dequeue();
                                        foreach (var item in AdslTaskQueues.Where(r => r.Value.previoustaskorderid == ton).Select(r => r.Value.taskorderno)) queue.Enqueue(item);
                                        AdslTaskQueues.Remove(ton);
                                        if (AdslProccessIndexes.ContainsKey(ton))
                                            AdslProccessIndexes.Remove(ton);
                                    }
                                    if (AdslProccesses.ContainsKey(t.taskorderno))
                                        AdslProccesses.Remove(t.taskorderno);
                                }
                            }
                            else
                            {
                                AdslTaskQueues[t.taskorderno] = t;
                                if (t.previoustaskorderid == null)
                                {
                                    //Süreç Başlangıç Taskları
                                    if (AdslTaskTypes[AdslTasks[t.taskid].tasktype].startsProccess)
                                    {
                                        AdslProccesses[t.taskorderno] = new DTOs.Adsl.KocAdslProccess();
                                    }
                                    AdslProccessIndexes[t.taskorderno] = t.taskorderno;
                                    if (!proccesIds.Contains(t.taskorderno)) proccesIds.Add(t.taskorderno);
                                }
                                else {
                                    var proccessNo = AdslProccessIndexes[t.previoustaskorderid.Value];
                                    AdslProccessIndexes[t.taskorderno] = proccessNo;
                                    if (!proccesIds.Contains(t.taskorderno)) proccesIds.Add(proccessNo);
                                }
                            }
                        }
                        DTOs.Adsl.KocAdslProccess.updateProccesses(new Queue<int>(proccesIds));
                    }
                }
            }
        }
        public static async Task loadAdslCustomers(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select customerid,tc,customername,lastupdated,ilKimlikNo,ilceKimlikNo,superonlineCustNo,gsm,phone from customer where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.customer
                            {
                                customerid = (int)sqlreader[0],
                                tc = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                customername = sqlreader.IsDBNull(2) ? null : (string)sqlreader[2],
                                lastupdated = sqlreader.IsDBNull(3) ? null : (DateTime?)sqlreader[3],
                                ilKimlikNo = sqlreader.IsDBNull(4) ? null : (int?)sqlreader[4],
                                ilceKimlikNo = sqlreader.IsDBNull(5) ? null : (int?)sqlreader[5],
                                superonlineCustNo = sqlreader.IsDBNull(6) ? null : (string)sqlreader[6],
                                gsm = sqlreader.IsDBNull(7) ? null : (string)sqlreader[7],
                                phone = sqlreader.IsDBNull(8) ? null : (string)sqlreader[8],
                            });
                            AdslCustomers[t.customerid] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslTasks(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select taskid,taskname,tasktype,lastupdated from task where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_task
                            {
                                taskid = (int)sqlreader[0],
                                taskname = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                tasktype = (int)sqlreader[2],
                                lastupdated = sqlreader.IsDBNull(3) ? null : (DateTime?)sqlreader[3],
                            });
                            AdslTasks[t.taskid] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslPersonels(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select personelid,personelname,lastupdated,relatedpersonelid, ilKimlikNo, ilceKimlikNo, roles, email from personel where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_personel
                            {
                                personelid = (int)sqlreader[0],
                                personelname = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                lastupdated = sqlreader.IsDBNull(2) ? null : (DateTime?)sqlreader[2],
                                relatedpersonelid = sqlreader.IsDBNull(3) ? null : (int?)sqlreader[3],
                                ilKimlikNo = sqlreader.IsDBNull(4) ? null : (int?)sqlreader[4],
                                ilceKimlikNo = sqlreader.IsDBNull(5) ? null : (int?)sqlreader[5],
                                roles = (int)sqlreader[6],
                                email = sqlreader.IsDBNull(7) ? null : (string)sqlreader[7],
                            });
                            AdslPersonels[t.personelid] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslStatus(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select taskstateid,taskstate,lastupdated,statetype from taskstatepool where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_taskstatepool
                            {
                                taskstateid = (int)sqlreader[0],
                                taskstate = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                lastupdated = sqlreader.IsDBNull(2) ? null : (DateTime?)sqlreader[2],
                                statetype = sqlreader.IsDBNull(3) ? null : (int?)sqlreader[3],
                            });
                            AdslStatus[t.taskstateid] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslSl(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select * from SL where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.SL
                            {
                                SLID = (int)sqlreader[0],
                                SLName = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                KocSTask = sqlreader.IsDBNull(2) ? null : (string)sqlreader[2],
                                KocETask = sqlreader.IsDBNull(3) ? null : (string)sqlreader[3],
                                KocMaxTime = sqlreader.IsDBNull(4) ? null : (int?)sqlreader[4], // koç'un sl tamamlaması gereken max süre
                                BayiSTask = sqlreader.IsDBNull(5) ? null : (string)sqlreader[5],
                                BayiETask = sqlreader.IsDBNull(6) ? null : (string)sqlreader[6],
                                BayiMaxTime = sqlreader.IsDBNull(7) ? null : (int?)sqlreader[7], // Bayinin sl tamamlaması gereken max süre
                                lastupdated = (DateTime)sqlreader[8],
                                updatedby = (int)sqlreader[9],
                                deleted = sqlreader.IsDBNull(10) ? null : (bool?)sqlreader[10],
                            });
                            var tDTO = t.toDTO<DTOs.Adsl.DTOSL>();
                            AdslSl[t.SLID] = tDTO;
                            if (!string.IsNullOrWhiteSpace(t.BayiSTask))
                            {
                                foreach (var tid in t.BayiSTask.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(r => int.Parse(r)))
                                {
                                    if (!AdslTaskSl.ContainsKey(tid))
                                    {
                                        AdslTaskSl[tid] = (new KeyValuePair<int, List<int>>[]
                                                {
                                                new KeyValuePair<int, List<int>>(0, new List<int>()),
                                                new KeyValuePair<int, List<int>>(1, new List<int>()),
                                                new KeyValuePair<int, List<int>>(2, new List<int>()),
                                                new KeyValuePair<int, List<int>>(3, new List<int>())
                                                }).ToDictionary(r => r.Key, r => r.Value);
                                    }
                                    AdslTaskSl[tid][0].Add(t.SLID);
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(t.BayiETask))
                            {
                                foreach (var tid in t.BayiETask.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(r => int.Parse(r)))
                                {
                                    if (!AdslTaskSl.ContainsKey(tid))
                                    {
                                        AdslTaskSl[tid] = (new KeyValuePair<int, List<int>>[]
                                                {
                                                new KeyValuePair<int, List<int>>(0, new List<int>()),
                                                new KeyValuePair<int, List<int>>(1, new List<int>()),
                                                new KeyValuePair<int, List<int>>(2, new List<int>()),
                                                new KeyValuePair<int, List<int>>(3, new List<int>())
                                                }).ToDictionary(r => r.Key, r => r.Value);
                                    }
                                    AdslTaskSl[tid][1].Add(t.SLID);
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(t.KocSTask))
                            {
                                foreach (var tid in t.KocSTask.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(r => int.Parse(r)))
                                {
                                    if (!AdslTaskSl.ContainsKey(tid))
                                    {
                                        AdslTaskSl[tid] = (new KeyValuePair<int, List<int>>[]
                                                {
                                                new KeyValuePair<int, List<int>>(0, new List<int>()),
                                                new KeyValuePair<int, List<int>>(1, new List<int>()),
                                                new KeyValuePair<int, List<int>>(2, new List<int>()),
                                                new KeyValuePair<int, List<int>>(3, new List<int>())
                                                }).ToDictionary(r => r.Key, r => r.Value);
                                    }
                                    AdslTaskSl[tid][2].Add(t.SLID);
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(t.KocETask))
                            {
                                foreach (var tid in t.KocETask.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(r => int.Parse(r)))
                                {
                                    if (!AdslTaskSl.ContainsKey(tid))
                                    {
                                        AdslTaskSl[tid] = (new KeyValuePair<int, List<int>>[]
                                                {
                                                new KeyValuePair<int, List<int>>(0, new List<int>()),
                                                new KeyValuePair<int, List<int>>(1, new List<int>()),
                                                new KeyValuePair<int, List<int>>(2, new List<int>()),
                                                new KeyValuePair<int, List<int>>(3, new List<int>())
                                                }).ToDictionary(r => r.Key, r => r.Value);
                                    }
                                    AdslTaskSl[tid][3].Add(t.SLID);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static async Task loadAdslIls()
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select * from il";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.il
                            {
                                kimlikNo = (int)sqlreader[0],
                                ad = (string)sqlreader[1],
                            });
                            AdslIls[t.kimlikNo] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslIlces()
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select kimlikNo,ad from ilce";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.ilce
                            {
                                kimlikNo = (int)sqlreader[0],
                                ad = (string)sqlreader[1],
                            });
                            AdslIlces[t.kimlikNo] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslTaskTypes()
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select * from tasktypes";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_tasktypes
                            {
                                TaskTypeId = (int)sqlreader[0],
                                TaskTypeName = (string)sqlreader[1],
                                startsProccess = (bool)sqlreader[2],
                            });
                            AdslTaskTypes[t.TaskTypeId] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslPaymentSystemTypes()
        { // Hakediş hesaplama için hakediş tipleri
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select * from paymentsystemtype";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.paymentsystemtype
                            {
                                id = (int)sqlreader[0],
                                paymentType = (string)sqlreader[1],
                            });
                            AdslPaymentSystemType[t.id] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslPaymentSystem()
        { // Hakediş hesaplama için satış prim ve servis hakedişleri
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select * from paymentsystem";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.paymentsystem
                            {
                                id = (int)sqlreader[0],
                                paymentType = (int)sqlreader[1],
                                payment = sqlreader.IsDBNull(2) ? null : (double?)sqlreader[2],
                                upperLimitAmount = sqlreader.IsDBNull(3) ? null : (int?)sqlreader[3],
                                upperLimitSL = sqlreader.IsDBNull(4) ? null : (int?)sqlreader[4],
                            });
                            AdslPaymentSystem[t.id] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslObjectTypes()
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select * from objecttypes";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_objecttypes
                            {
                                typeid = (int)sqlreader[0],
                                typname = (string)sqlreader[1],
                            });
                            AdslObjectTypes[t.typeid] = t;
                        }
                    }
                }
            }
        }

        public static async Task updateAdslData()
        {
            await aLockObject.WaitAsync().ConfigureAwait(false);
            var lastUpdated = DateTime.Now;
            await Task.WhenAll(new Task[] {
                loadAdslCustomers(AdslLastUpdated),
                loadAdslPersonels(AdslLastUpdated),
                loadAdslTasks(AdslLastUpdated),
                loadAdslStatus(AdslLastUpdated),
                loadAdslSl(AdslLastUpdated)
            }).ConfigureAwait(false);
            await loadAdslTaskQueues(AdslLastUpdated).ConfigureAwait(false);
            AdslLastUpdated = lastUpdated;
            aLockObject.Release();
        }

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Web API routes
            config.MapHttpAttributeRoutes();
            config.EnableCors(new EnableCorsAttribute("*", "*", "*", "X-KOC-Token"));
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DTOs.Adsl.SKReport>("SKReports");
            builder.EntitySet<DTOs.Adsl.SLBayiReport>("SLBayiReports");
            builder.EntitySet<DTOs.Adsl.SLKocReport>("SLKocReports");
            builder.EntitySet<DTOs.Adsl.SKPaymentReport>("SKPaymentReports");
            builder.EntitySet<DTOs.Adsl.SKStandbyTaskReport>("SKStandbyTaskReports");
            config.MapODataServiceRoute(
                routeName: "ODataRoute",
                routePrefix: "odata",
                model: builder.GetEdmModel()
            );
            TeknarProxyService.Start();
            Task.WaitAll(new Task[] { loadAdslPaymentSystemTypes(), loadAdslPaymentSystem(), loadAdslIls(), loadAdslObjectTypes(), loadAdslTaskTypes(), loadAdslIlces(), updateAdslData()/*, updateFiberData()*/});
        }
    }
}