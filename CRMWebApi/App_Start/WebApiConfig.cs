using CRMWebApi.NetflowWebServis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        static AuthHeader authHeader = new AuthHeader();

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
                                else
                                {
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
                                        else
                                        {
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
        public static ConcurrentDictionary<int, DTOs.Adsl.KocAdslProccess> AdslProccesses = new ConcurrentDictionary<int, DTOs.Adsl.KocAdslProccess>();
        //public static ConcurrentDictionary<int, int> AdslProccessIndexes = new ConcurrentDictionary<int, int>();
        public static ConcurrentDictionary<int, Models.Adsl.adsl_taskqueue> AdslTaskQueues = new ConcurrentDictionary<int, CRMWebApi.Models.Adsl.adsl_taskqueue>();
        public static ConcurrentDictionary<int, HashSet<int>> AdslSubTasks = new ConcurrentDictionary<int, HashSet<int>>(); // Bu taska bağlı türemiş tasklar
        public static ConcurrentDictionary<int, Models.Adsl.customer> AdslCustomers = new ConcurrentDictionary<int, Models.Adsl.customer>();
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
        public static Dictionary<int, Models.Adsl.adsl_campaigns> AdslCampaigns = new Dictionary<int, Models.Adsl.adsl_campaigns>(); // müşteri kampanyası için
        public static ConcurrentDictionary<int, Models.Adsl.adsl_customerproduct> AdslCustomerProducts = new ConcurrentDictionary<int, Models.Adsl.adsl_customerproduct>(); // Raporda müşterinin kampanyasını göstermek için
        public static ConcurrentDictionary<int, int> K_PersonelForProccess = new ConcurrentDictionary<int, int>(); // bir şüreçin kurulumunu gerçekleştiren personel için aynı anda kısıtlı sayıda kurulum izni (key : proccessid, value : personelid)
        ///public static ConcurrentDictionary<int, DTOs.Adsl.Cozum> Rapor = new ConcurrentDictionary<int, DTOs.Adsl.Cozum>();

        public static async Task loadAdslTaskQueues(DateTime lastUpdated)
        {
            Stopwatch stw = Stopwatch.StartNew();
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    try
                    {
                        await conn.OpenAsync().ConfigureAwait(false);
                        var selectCommand = conn.CreateCommand();
                        selectCommand.CommandText = $"select * from taskqueue where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'"; // aynı gün içerisinde yapılan değişiklikler işleme alınamadığı için HH:mm:ss eklendi (Hüseyin KOZ)
                        using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                        {
                            var proccessIds = new HashSet<int>();
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
                                if (t.previoustaskorderid.HasValue)
                                {
                                    if (AdslSubTasks.ContainsKey(t.previoustaskorderid.Value))
                                        AdslSubTasks[t.previoustaskorderid.Value].Add(t.taskorderno);
                                    else
                                    {
                                        AdslSubTasks[t.previoustaskorderid.Value] = new HashSet<int>(new int[] { t.taskorderno });
                                    }
                                }
                                if (t.deleted == true)
                                {
                                    if (t.previoustaskorderid.HasValue && AdslSubTasks.ContainsKey(t.previoustaskorderid.Value))
                                    {
                                        HashSet<int> c;
                                        AdslSubTasks[t.previoustaskorderid.Value].Remove(t.taskorderno);
                                        if (AdslSubTasks[t.previoustaskorderid.Value].Count == 0) AdslSubTasks.TryRemove(t.previoustaskorderid.Value, out c);
                                    }
                                    if (AdslTaskQueues.ContainsKey(t.taskorderno))
                                    {
                                        //Derinlemesine temizlik :) Silinen task ve bu taska bağlı tüm taskları listeden çıkart
                                        if (!proccessIds.Contains(t.relatedtaskorderid ?? t.taskorderno)) proccessIds.Add(t.relatedtaskorderid ?? t.taskorderno);
                                        var queue = new Queue<int>();
                                        queue.Enqueue(t.taskorderno);
                                        while (queue.Count > 0)
                                        {
                                            var ton = queue.Dequeue();
                                            Models.Adsl.adsl_taskqueue ax;
                                            foreach (var item in AdslTaskQueues.Where(r => r.Value.previoustaskorderid == ton).Select(r => r.Value.taskorderno)) queue.Enqueue(item);
                                            AdslTaskQueues.TryRemove(ton, out ax);
                                        }
                                        DTOs.Adsl.KocAdslProccess kx;
                                        if (AdslProccesses.ContainsKey(t.taskorderno))
                                            AdslProccesses.TryRemove(t.taskorderno, out kx);
                                    }
                                }
                                else
                                {
                                    AdslTaskQueues[t.taskorderno] = t;
                                    if (t.previoustaskorderid == null)
                                    {
                                        /*Süreç Başlangıç Taskları*/
                                    if (AdslTaskTypes[AdslTasks[t.taskid].tasktype].startsProccess)
                                        {
                                            AdslProccesses[t.taskorderno] = new DTOs.Adsl.KocAdslProccess();
                                        }
                                        if (!proccessIds.Contains(t.relatedtaskorderid ?? t.taskorderno)) proccessIds.Add(t.relatedtaskorderid ?? t.taskorderno);
                                    }
                                    else
                                    {
                                        AdslProccesses[t.relatedtaskorderid ?? t.taskorderno] = new DTOs.Adsl.KocAdslProccess();
                                        if (!proccessIds.Contains(t.relatedtaskorderid ?? t.taskorderno)) proccessIds.Add(t.relatedtaskorderid ?? t.taskorderno);
                                    }
                                }
                            }
                            var tttt = stw.Elapsed;
                            DTOs.Adsl.KocAdslProccess.updateProccesses(new Queue<int>(proccessIds));
                        }

                    }
                    catch (Exception)
                    {
                        loadAdslTaskQueues(lastUpdated);
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
                    selectCommand.CommandText = $"select customerid,tc,customername,lastupdated,ilKimlikNo,ilceKimlikNo,superonlineCustNo,gsm,phone,description from customer where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
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
                                description = sqlreader.IsDBNull(9) ? null : (string)sqlreader[9],
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
                    selectCommand.CommandText = $"select personelid,personelname,lastupdated,relatedpersonelid, ilKimlikNo, ilceKimlikNo, roles, email, kurulumpersonelid, notes, mobile, responseregions from personel where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
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
                                kurulumpersonelid = sqlreader.IsDBNull(8) ? null : (int?)sqlreader[8],
                                notes = sqlreader.IsDBNull(9) ? null : (string)sqlreader[9],
                                mobile = sqlreader.IsDBNull(10) ? null : (string)sqlreader[10],
                                responseregions = sqlreader.IsDBNull(11) ? null : (string)sqlreader[11],
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
        public static async Task loadAdslCampaigns(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select id,name,lastupdated from campaigns where lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_campaigns
                            {
                                id = (int)sqlreader[0],
                                name = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                lastupdated = sqlreader.IsDBNull(2) ? null : (DateTime?)sqlreader[2],
                            });
                            AdslCampaigns[t.id] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadAdslCustomerProducts(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select taskid,campaignid from customerproduct where deleted=0 and lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}' group by taskid, campaignid";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_customerproduct
                            {
                                taskid = (int)sqlreader[0],
                                campaignid = sqlreader.IsDBNull(1) ? null : (int?)sqlreader[1],
                            });
                            AdslCustomerProducts[(int)t.taskid] = t;
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
                loadAdslCampaigns(AdslLastUpdated),
                loadAdslCustomerProducts(AdslLastUpdated),
                loadAdslSl(AdslLastUpdated)
            }).ConfigureAwait(false);
            await loadAdslTaskQueues(AdslLastUpdated).ConfigureAwait(false);
            AdslLastUpdated = lastUpdated;
            aLockObject.Release();
        }

        // Cari işlemler raporu için cari hareket ve gerekli bilgilerin alınması işlemleri start
        public static Dictionary<int, Models.Cari.GelirGiderTurleri> CariGelirGiderTurleri = new Dictionary<int, Models.Cari.GelirGiderTurleri>();
        public static Dictionary<int, Models.Cari.Hareketler> CariHareketler = new Dictionary<int, Models.Cari.Hareketler>();
        public static Dictionary<int, Models.Adsl.adsl_personel> CariPersonels = new Dictionary<int, Models.Adsl.adsl_personel>();
        public static DateTime CariLastUpdated = new DateTime(1900, 1, 1);
        public static SemaphoreSlim cLockObject = new SemaphoreSlim(1);
        public static string JoinHakedis = null;

        public static async Task loadCariGelirGiderTurleri ()
        {
            using (var db = new Models.Cari.KOCCariEntities())
            {
                JoinHakedis = null;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select * from GelirGiderTurleri";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Cari.GelirGiderTurleri
                            {
                                Id = (int)sqlreader[0],
                                GrupId = (int)sqlreader[1],
                                Etiket = (string)sqlreader[2],
                            });
                            CariGelirGiderTurleri[t.Id] = t;
                        }
                    }
                }
                Queue<int> hakedis = new Queue<int>();
                hakedis.Enqueue(6); // Bayi Hakediş Ödemesi Id'si (Genel Grup Id)
                while (hakedis.Count > 0)
                {
                    var t = hakedis.Dequeue();
                    if (JoinHakedis == null)
                        JoinHakedis = "(" + t;
                    else
                        JoinHakedis += "," + t;
                    foreach (var item in CariGelirGiderTurleri.Where(k => k.Value.GrupId == t))
                        hakedis.Enqueue(item.Value.Id);
                }
                JoinHakedis += ")";
            }
        }
        public static async Task loadCariHareketler ()
        {
            using (var db = new Models.Cari.KOCCariEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select * from Hareketler where BorcluFirma=9 and GelirGiderTuru in {JoinHakedis}"; // 9 DSL Çözüm Merkezi (Hakediş Ödeyen Birim)
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Cari.Hareketler
                            {
                                Id = (int)sqlreader[0],
                                BorcluFirma = (int)sqlreader[1],
                                AlacakliFirma = (int)sqlreader[2],
                                GelirGiderTuru = (int)sqlreader[3],
                                IslemTarihi = (DateTime)sqlreader[4],
                                Donem = (DateTime)sqlreader[5],
                                VadeTarihi = (DateTime)sqlreader[6],
                                Tutar = (decimal)sqlreader[7],
                                KdvMatrah = (decimal)sqlreader[8],
                                Aciklama = sqlreader.IsDBNull(9) ? null : (string)sqlreader[9],
                                Muavin = sqlreader.IsDBNull(10) ? null : (int?)sqlreader[10],
                                Kaynak = sqlreader.IsDBNull(11) ? null : (int?)sqlreader[11],
                                Odendi = (bool)sqlreader[12],
                            });
                            CariHareketler[t.Id] = t;
                        }
                    }
                }
            }
        }
        public static async Task loadCariPersonels(DateTime lastUpdated)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                using (var conn = db.Database.Connection as SqlConnection)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = $"select personelid,personelname,lastupdated, ilKimlikNo, ilceKimlikNo from personel where email not like '%@kociletisim.com.tr' and lastupdated > '{lastUpdated.ToString("yyyy-MM-dd HH:mm:ss")}'";
                    using (var sqlreader = await selectCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess).ConfigureAwait(false))
                    {
                        while (await sqlreader.ReadAsync().ConfigureAwait(false))
                        {
                            var t = (new Models.Adsl.adsl_personel
                            {
                                personelid = (int)sqlreader[0],
                                personelname = sqlreader.IsDBNull(1) ? null : (string)sqlreader[1],
                                lastupdated = sqlreader.IsDBNull(2) ? null : (DateTime?)sqlreader[2],
                                ilKimlikNo = sqlreader.IsDBNull(3) ? null : (int?)sqlreader[3],
                                ilceKimlikNo = sqlreader.IsDBNull(4) ? null : (int?)sqlreader[4],
                            });
                            CariPersonels[t.personelid] = t;
                        }
                    }
                }
            }
        }
        public static async Task updateCariData()
        {
            await cLockObject.WaitAsync().ConfigureAwait(false);
            var lastUpdated = DateTime.Now;
            await Task.WhenAll(new Task[] {
                loadCariGelirGiderTurleri(),
                loadCariPersonels(CariLastUpdated),
            }).ConfigureAwait(false);
            await loadCariHareketler().ConfigureAwait(false);
            CariLastUpdated = lastUpdated;
            cLockObject.Release();
        }
        // Cari işlemler raporu için cari hareket ve gerekli bilgilerin alınması işlemleri end

        static int?[] ils = { 4, 5, 8, 19, 24, 25, 28, 29, 36, 52, 53, 55, 57, 60, 61, 69, 75, 76 }; // Bölge içi iller
        private static void insertTaskqueue(DTOs.Adsl.DTOcustomer request)
        { // yeni task oluşturma
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var oldCust = db.customer.Where(c => c.tc == request.tc && c.deleted == false).ToList();
                    if (oldCust.Count == 0)
                    {
                        var customer = new Models.Adsl.customer
                        {
                            customername = request.customername.ToUpper(),
                            tc = request.tc,
                            gsm = request.gsm,
                            phone = request.phone,
                            ilKimlikNo = request.ilKimlikNo,
                            ilceKimlikNo = request.ilceKimlikNo,
                            bucakKimlikNo = request.bucakKimlikNo,
                            mahalleKimlikNo = request.mahalleKimlikNo,
                            yolKimlikNo = request.yolKimlikNo,
                            binaKimlikNo = request.binaKimlikNo,
                            daire = request.daire,
                            updatedby = 1393,
                            description = request.description,
                            lastupdated = DateTime.Now,
                            creationdate = DateTime.Now,
                            deleted = false,
                            email = request.email,
                            superonlineCustNo = request.superonlineCustNo,
                        };
                        db.customer.Add(customer);
                        db.SaveChanges();

                        var cust = db.customer.Where(c => c.tc == request.tc && c.customername == request.customername).FirstOrDefault();

                        var taskqueue = new Models.Adsl.adsl_taskqueue
                        {
                            appointmentdate = request.appointmentdate,
                            attachedobjectid = cust.customerid,
                            attachedpersonelid = request.salespersonel,
                            attachmentdate = DateTime.Now,
                            creationdate = DateTime.Now,
                            deleted = false,
                            description = request.taskdescription,
                            lastupdated = DateTime.Now,
                            status = null,
                            taskid = request.taskid,
                            updatedby = 1393,
                            fault = request.fault
                        };
                        db.taskqueue.Add(taskqueue);
                        db.SaveChanges();
                        taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.10.2016
                        db.SaveChanges();
                    }
                    tran.Commit();
                    WebApiConfig.updateAdslData();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    tran.Rollback();
                }
        }
        private static void saveTaskqueue (Models.Adsl.adsl_taskqueue tq)
        { // task kapatmak için kullanılacak
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            using (var tran = db.Database.BeginTransaction())
                try
                {
                    var tsm = db.taskstatematches.Where(r => r.taskid == tq.taskid && r.stateid == tq.status).FirstOrDefault();
                    var dtq = db.taskqueue.Where(r => r.taskorderno == tq.taskorderno).First();
                    var automandatoryTasks = new List<int>();
                    if (tsm != null)
                        automandatoryTasks.AddRange((tsm.automandatorytasks ?? "").Split(',').Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => Convert.ToInt32(r)).ToList());
                    if (automandatoryTasks != null)
                        foreach (var item in automandatoryTasks)
                        {
                            //var ptq = tq;
                            int saletask = tq.relatedtaskorderid ?? tq.taskorderno;
                            //while (ptq != null)
                            //{
                            //    ptq.task = db.task.Where(t => t.taskid == ptq.taskid).FirstOrDefault();
                            //    if (ptq.task != null && db.tasktypes.First(r => ptq.task.tasktype == r.TaskTypeId).startsProccess)
                            //    {
                            //        saletask = ptq.taskorderno;
                            //        break;
                            //    }
                            //    else
                            //    {
                            //        ptq = db.taskqueue.Where(t => t.taskorderno == ptq.previoustaskorderid).FirstOrDefault();
                            //    }
                            //}
                            if (db.taskqueue.Where(r => r.deleted == false && (r.previoustaskorderid == tq.taskorderno) && r.taskid == item && (r.status == null || r.taskstatepool.statetype != 2)).Any())
                                continue;
                            int? personel_id = tq.attachedpersonelid; // Orhan Özçelik
                            //Otomatik Kurulum Bayisi Ataması (Oluşan task kurulum taskı ise)
                            var oot = db.task.FirstOrDefault(t => t.taskid == item);
                            if (oot == null) continue;
                            if (oot.tasktype == 2)
                            {
                                var satbayi = db.taskqueue.First(r => r.taskorderno == saletask).attachedpersonelid;
                                personel_id = db.personel.First(p => p.personelid == satbayi).kurulumpersonelid; //Kurulum bayisi idsi
                            }
                            var amtq = new Models.Adsl.adsl_taskqueue
                            {
                                appointmentdate = null,
                                attachedpersonelid = personel_id,
                                attachmentdate = personel_id != null ? (DateTime?)DateTime.Now : null,
                                attachedobjectid = tq.attachedobjectid,
                                taskid = item,
                                creationdate = DateTime.Now,
                                deleted = false,
                                lastupdated = DateTime.Now,
                                previoustaskorderid = tq.taskorderno,
                                relatedtaskorderid = saletask,
                                updatedby = 1393, // Yazılım Koç
                            };
                            if ((automandatoryTasks.Contains(38) || automandatoryTasks.Contains(60)) && tq.attachedpersonelid != 1016)
                            {
                                var mailInfo = new List<object>();
                                Controllers.AdslTaskqueueController mailSend = new Controllers.AdslTaskqueueController();
                                mailInfo.Add(tq.attachedobjectid);
                                mailInfo.Add(tq.attachedpersonelid);
                                mailSend.sendemail(mailInfo);
                            }
                            db.taskqueue.Add(amtq);

                            dtq.status = tq.status;
                            dtq.consummationdate = DateTime.Now;
                            dtq.lastupdated = DateTime.Now;
                            dtq.description += "\r\n Web Servis ile kapatıldı.";
                            dtq.appointmentdate = tq.appointmentdate;

                            db.SaveChanges();
                            tran.Commit();
                            WebApiConfig.updateAdslData();
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    tran.Rollback();
                }
        }
        private static void insertOnlyTaskqueue (Models.Adsl.adsl_taskqueue request)
        {
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                Models.Adsl.adsl_taskqueue taskqueue = new Models.Adsl.adsl_taskqueue
                {
                    taskid = request.taskid,
                    attachedobjectid = request.attachedobjectid,
                    attachedpersonelid = request.attachedpersonelid,
                    appointmentdate = request.appointmentdate,
                    creationdate = DateTime.Now,
                    attachmentdate = DateTime.Now,
                    lastupdated = DateTime.Now,
                    description = request.description,
                    updatedby = 1393, // Yazılım Koç
                    deleted = false
                };
                db.taskqueue.Add(taskqueue);
                db.SaveChanges();
                taskqueue.relatedtaskorderid = taskqueue.taskorderno; // başlangıç tasklarının relatedtaskorderid kendi taskorderno tutacak (Hüseyin KOZ) 13.10.2016
                db.SaveChanges();
                WebApiConfig.updateAdslData();
            }
        } 

        private static void customerInfo (Models.Adsl.customer cust, GetWorkflowDetailByUserResponse data, int taskid, int salespersonel)
        {  // yeni task oluşturmak için gerekli bilgileri oluştur... (newCheckin -> yeni müşteri mi yoksa sadece yeni task oluşturma işlemi mi ? false : sadece gelen müşteriye task oluştur)
            using (var db = new Models.Adsl.KOCSAMADLSEntities())
            {
                DTOs.Adsl.DTOcustomer obj = new DTOs.Adsl.DTOcustomer();
                if (cust != null)
                {
                    obj.customername = cust.customername;
                    obj.superonlineCustNo = cust.superonlineCustNo;
                    var tc = cust.superonlineCustNo;
                    var cTc = db.customer.Where(r => r.tc == tc).FirstOrDefault();
                    while (cTc != null)
                    { // süperonline müşteri no tc olarak sistemde kayıtlı ise kayıtlı olmayan tc bul
                        Random rd = new Random();
                        int ek = rd.Next(9);
                        tc += ek + "";
                        cTc = db.customer.Where(r => r.tc == tc).FirstOrDefault();
                    }
                    obj.tc = tc;
                    obj.gsm = cust.gsm;
                    obj.phone = cust.phone;
                    obj.ilKimlikNo = cust.ilKimlikNo;
                    obj.ilceKimlikNo = cust.ilceKimlikNo;
                    obj.bucakKimlikNo = cust.bucakKimlikNo;
                    obj.mahalleKimlikNo = cust.mahalleKimlikNo;
                    obj.yolKimlikNo = cust.yolKimlikNo;
                    obj.binaKimlikNo = cust.binaKimlikNo;
                    obj.daire = cust.daire;
                    obj.email = cust.email;
                    obj.description = cust.description;
                }
                else
                {
                    var tc = data.CustomerId + "";
                    var cTc = db.customer.Where(r => r.tc == tc).FirstOrDefault();
                    while (cTc != null)
                    { // süperonline müşteri no tc olarak sistemde kayıtlı ise kayıtlı olmayan tc bul
                        Random rd = new Random();
                        int ek = rd.Next(9);
                        tc += ek + "";
                        cTc = db.customer.Where(r => r.tc == tc).FirstOrDefault();
                    }
                    obj.tc = tc;
                    obj.superonlineCustNo = data.CustomerId + "";
                    obj.customername = data.CustomerName;
                    obj.gsm = data.CustomerPhone;
                    obj.email = data.CustomerEmail;
                    obj.description = data.CustomerAddress;
                    string ilcead = data.CustomerAddressDistrict;
                    var ilce = ilcead != null ? db.ilce.Where(r => r.ad == ilcead).FirstOrDefault() : null;
                    if (ilce != null)
                    {
                        obj.ilceKimlikNo = ilce.kimlikNo;
                        obj.ilKimlikNo = ilce.ilKimlikNo;
                    }
                    else
                    {
                        string ilad = data.CustomerAddressCity;
                        var il = ilad != null ? db.il.Where(r => r.ad == ilad).FirstOrDefault() : null;
                        if (il != null) obj.ilKimlikNo = il.kimlikNo;
                    }
                    obj.yolKimlikNo = 61;
                    obj.binaKimlikNo = 61;
                    obj.daire = 61;
                }
                obj.taskid = taskid;
                obj.salespersonel = salespersonel;
                string appointment = data.WorkflowStartTime.ToString("yyyy-MM-dd HH':'mm':'ss");
                obj.appointmentdate = Convert.ToDateTime(appointment);
                obj.taskdescription = "$#&" + data.WorkflowId + "$#& Web servis aracılığı ile oluşturuldu.";

                if (obj.ilKimlikNo == null || ils.FirstOrDefault(r => r == obj.ilKimlikNo.Value) != null)
                    insertTaskqueue(obj);
            }
        } 
        //NetFlow Tracker
        static Timer nfT = new Timer(async (o) =>
        {
            try
            {
                await updateAdslData().ConfigureAwait(false);
                var typeIds = AdslTaskTypes.Where(r => r.Value.startsProccess == true).Select(r => r.Key).ToList(); // proces başlatan task tip id'leri
                var taskIds = AdslTasks.Where(r => typeIds.Contains(r.Value.tasktype)).Select(r => r.Key).ToList(); // proces başlatan task id'leri

                // Bu kod 60 dakikada bir çalışacak
                authHeader.Username = "EXT02308383_66010.00002";
                authHeader.Password = "6qRF0687";

                var request = new GetWorkflowListByUserRequest();
                request.TicketingTypeCode = "321";
                //request.SegmentCode = "Residential";
                request.SearchStartDate = DateTime.Now.AddHours(-3);
                //request.SearchStartDate = DateTime.Now.AddDays(-1);
                request.SearchEndDate = DateTime.Now;
                #region Kurulum ve Cihaz Gönderim Taskları
                using (var wsc = new NetflowTellcomWSSoapClient())
                using (var db = new Models.Adsl.KOCSAMADLSEntities())
                {
                    var response = wsc.GetWorkflowIdListByUser(authHeader, request);
                    var workList = response.ToList();
                    for (int i = 0; i < workList.Count; i++)
                    {
                        var data = wsc.GetWorkflowDetailByUser(authHeader, workList[i].WorkflowId).FirstOrDefault();
                        if (data != null && (data.WorkflowStatusCode == "KURULUMKUYRUKTA" || data.WorkflowStatusCode == "KURULUMATANDI"))
                        {
                            string smno = data.CustomerId + "";
                            bool register = false;
                            var cust = db.customer.Where(r => r.superonlineCustNo == smno && r.deleted == false).ToList();
                            foreach (var item in cust)
                            {
                                bool isSaved = false;
                                register = true;
                                var tasks = AdslTaskQueues.Where(r => r.Value.attachedobjectid == item.customerid && taskIds.Contains(r.Value.taskid)).ToList();
                                foreach (var t in tasks)
                                {
                                    var id = t.Value.relatedtaskorderid.HasValue ? t.Value.relatedtaskorderid.Value : t.Key;
                                    if (AdslProccesses.ContainsKey(id) && (AdslProccesses[id].Last_Status == 0 || AdslProccesses[id].Last_Status == 3))
                                    {
                                       isSaved = true;
                                       if (AdslTaskQueues.ContainsKey(AdslProccesses[id].Last_TON) && (AdslTaskQueues[AdslProccesses[id].Last_TON].taskid == 34 || AdslTaskQueues[AdslProccesses[id].Last_TON].taskid == 36))
                                        {  // task, churnler için onay tesis süreci veya onay internet geçişi ise kuruluma onay ver
                                            Models.Adsl.adsl_taskqueue tq = AdslTaskQueues[AdslProccesses[id].Last_TON];
                                            tq.status = 9119; // Onaylandı
                                            string appointment = data.WorkflowStartTime.ToString("yyyy-MM-dd HH':'mm':'ss");
                                            tq.appointmentdate = Convert.ToDateTime(appointment);
                                            saveTaskqueue(tq);
                                        }
                                        break;
                                    }
                                    /*// bu tasklardan süreç id kısmı descriptionda kontrol edilecek varsa işlem yapılmayacak $#&sid$# sid: süreç id
                                    var descs = t.Value.description.Split(new[] { "$#&" }, StringSplitOptions.None);
                                    if (descs.Length > 2 && descs[1] == (data.WorkflowId + ""))
                                    {
                                        isSaved = true;
                                        break;
                                    }*/
                                }
                                if (isSaved)
                                {
                                    register = false;
                                    break;
                                }
                            }
                            if (register || cust.Count == 0)
                            {
                                if (data.SegmentCode == "SOHO")
                                    customerInfo(cust.Count > 0 ? cust[0] : null, data, 56, 1393); // CC Satış Kurumsal
                                else if (data.SegmentCode == "Residential")
                                    customerInfo(cust.Count > 0 ? cust[0] : null, data, data.XdslServiceType.ToString() == "VDSL" ? 57 : 32, 1393);
                            }
                        }
                    }
                }
                #endregion

                #region SAM-SDM Sipariş Tamamlama Taskları
                request.TicketingTypeCode = "623";
                using (var wsc = new NetflowTellcomWSSoapClient())
                using (var db = new Models.Adsl.KOCSAMADLSEntities())
                {
                    var response = wsc.GetWorkflowIdListByUser(authHeader, request);
                    var workList = response.ToList();
                    for (int i = 0; i < workList.Count; i++)
                    {
                        var data = wsc.GetWorkflowDetailByUser(authHeader, workList[i].WorkflowId).FirstOrDefault();
                        if (data != null && (data.WorkflowStatusCode == "BIRINCISEVIYEKUYRUKTA" || data.WorkflowStatusCode == "IKINCISEVIYEKUYRUKTA"))
                        {
                            string smno = data.CustomerId + "";
                            bool register = false;
                            var cust = db.customer.Where(r => r.superonlineCustNo == smno && r.deleted == false).ToList();
                            foreach (var item in cust)
                            {
                                bool isSaved = false;
                                register = true;
                                var tasks = AdslTaskQueues.Where(r => r.Value.attachedobjectid == item.customerid && taskIds.Contains(r.Value.taskid)).ToList();
                                foreach (var t in tasks)
                                {
                                    //var id = t.Value.relatedtaskorderid.HasValue ? t.Value.relatedtaskorderid.Value : t.Key;
                                    //if (AdslProccesses.ContainsKey(id) && (AdslProccesses[id].Last_Status == 0 || AdslProccesses[id].Last_Status == 3))
                                    //{
                                    //    isSaved = true;
                                    //    break;
                                    //}
                                    // bu tasklardan süreç id kısmı descriptionda kontrol edilecek varsa işlem yapılmayacak $#&sid$# sid: süreç id
                                    var descs = t.Value.description.Split(new[] { "$#&" }, StringSplitOptions.None);
                                    if (descs.Length > 2 && descs[1] == (data.WorkflowId + ""))
                                    {
                                        isSaved = true;
                                        break;
                                    }
                                }
                                if (isSaved)
                                {
                                    register = false;
                                    break;
                                }
                            }
                            if (register || cust.Count == 0)
                            {
                                if (data.SegmentCode == "SOHO")
                                    customerInfo(cust.Count > 0 ? cust[0] : null, data, 114, 1393);
                                else if (data.SegmentCode == "Residential")
                                    customerInfo(cust.Count > 0 ? cust[0] : null, data, 33, 1393); // 1393 yazılım koç (Satış CC Churn sorumlu personel) (33 Satış CC Churn taskı)
                            }
                        }
                    }
                }
                #endregion

                #region CC-TİM Sipariş Tamamlama Taskları
                request.TicketingTypeCode = "441";
                using (var wsc = new NetflowTellcomWSSoapClient())
                using (var db = new Models.Adsl.KOCSAMADLSEntities())
                {
                    var response = wsc.GetWorkflowIdListByUser(authHeader, request);
                    var workList = response.ToList();
                    for (int i = 0; i < workList.Count; i++)
                    {
                        var data = wsc.GetWorkflowDetailByUser(authHeader, workList[i].WorkflowId).FirstOrDefault();
                        if (data != null && (data.WorkflowStatusCode == "BIRINCISEVIYEKUYRUKTA" || data.WorkflowStatusCode == "IKINCISEVIYEKUYRUKTA"))
                        {
                            string smno = data.CustomerId + "";
                            bool register = false;
                            var cust = db.customer.Where(r => r.superonlineCustNo == smno && r.deleted == false).ToList();
                            foreach (var item in cust)
                            {
                                register = true;
                                bool isSaved = false;
                                var tasks = AdslTaskQueues.Where(r => r.Value.attachedobjectid == item.customerid && taskIds.Contains(r.Value.taskid)).ToList();
                                foreach (var t in tasks)
                                {
                                    //var id = t.Value.relatedtaskorderid.HasValue ? t.Value.relatedtaskorderid.Value : t.Key;
                                    //if (AdslProccesses.ContainsKey(id) && (AdslProccesses[id].Last_Status == 0 || AdslProccesses[id].Last_Status == 3))
                                    //{
                                    //    isSaved = true;
                                    //    break;
                                    //}
                                    // bu tasklardan süreç id kısmı descriptionda kontrol edilecek varsa işlem yapılmayacak $#&sid$# sid: süreç id
                                    var descs = t.Value.description.Split(new[] { "$#&" }, StringSplitOptions.None);
                                    if (descs.Length > 2 && descs[1] == (data.WorkflowId + ""))
                                    {
                                        isSaved = true;
                                        break;
                                    }
                                }
                                if (isSaved)
                                {
                                    register = false;
                                    break;
                                }
                            }
                            if (register || cust.Count == 0)
                            {
                                if (data.SegmentCode == "SOHO")
                                    customerInfo(cust.Count > 0 ? cust[0] : null, data, 114, 1393);
                                else if (data.SegmentCode == "Residential")
                                    customerInfo(cust.Count > 0 ? cust[0] : null, data, 64, 1393); // 1393 yazılım koç (Satış CC Churn sorumlu personel) (33 Satış CC Churn taskı)
                            }
                        }
                    }
                }
                #endregion

                #region İkinci Donanım Kurulum Taskları
                request.TicketingTypeCode = "290";
                using (var wsc = new NetflowTellcomWSSoapClient())
                using (var db = new Models.Adsl.KOCSAMADLSEntities())
                {
                    var response = wsc.GetWorkflowIdListByUser(authHeader, request);
                    var workList = response.ToList();
                    for (int i = 0; i < workList.Count; i++)
                    {
                        var data = wsc.GetWorkflowDetailByUser(authHeader, workList[i].WorkflowId).FirstOrDefault();
                        if (data != null && (data.WorkflowStatusCode == "KURULUMKUYRUKTA" || data.WorkflowStatusCode == "KURULUMATANDI"))
                        {
                            string smno = data.CustomerId + "";
                            bool register = false;
                            var cust = db.customer.Where(r => r.superonlineCustNo == smno && r.deleted == false).ToList();
                            foreach (var item in cust)
                            {
                                bool isSaved = false;
                                register = true;
                                // Bayi ikinci donanım satışı ve ikinci donanım var mı kontrol et
                                var tasks = AdslTaskQueues.Where(r => r.Value.attachedobjectid == item.customerid && (r.Value.taskid == 88 || r.Value.taskid == 166)).ToList();
                                foreach (var t in tasks)
                                {
                                    var id = t.Value.relatedtaskorderid ?? t.Key;
                                    if (AdslProccesses.ContainsKey(id) && (AdslProccesses[id].Last_Status == 0 || AdslProccesses[id].Last_Status == 3))
                                    {
                                        isSaved = true;
                                        break;
                                    }
                                    /*// bu tasklardan süreç id kısmı descriptionda kontrol edilecek varsa işlem yapılmayacak $#&sid$# sid: süreç id
                                    var descs = t.Value.description.Split(new[] { "$#&" }, StringSplitOptions.None);
                                    if (descs.Length > 2 && descs[1] == (data.WorkflowId + ""))
                                    { // ikinci donanımlarda bayi satışları kontrol edilmeli
                                        isSaved = true;
                                        break;
                                    }*/
                                }
                                if (isSaved)
                                {
                                    register = false;
                                    break;
                                }
                            }
                            if (register)
                            { // müşteri bizde var ve ikinci donanım taskı oluşacaksa
                                Models.Adsl.adsl_taskqueue newTask = new Models.Adsl.adsl_taskqueue();
                                newTask.taskid = 88; //88 İkinci Donanım Giriş Taskı
                                newTask.attachedpersonelid = 1003; // 1003 Banur Aydın (İkinci Donanım sorumlu personel)
                                string appointment = data.WorkflowStartTime.ToString("yyyy-MM-dd HH':'mm':'ss");
                                newTask.appointmentdate = Convert.ToDateTime(appointment);
                                newTask.attachedobjectid = cust[0].customerid;
                                newTask.description = "$#&" + data.WorkflowId + "$#& Web servis aracılığı ile oluşturuldu.";

                                insertOnlyTaskqueue(newTask);
                            }
                            else if (cust.Count == 0)
                                customerInfo(null, data, 88, 1003); // 1003 Banur Aydın (İkinci Donanım sorumlu personel) (88 İkinci Donanım Giriş Taskı)
                        }
                    }
                }
                #endregion

                #region Modem Değişikliği Taskları
                request.TicketingTypeCode = "560";
                using (var wsc = new NetflowTellcomWSSoapClient())
                using (var db = new Models.Adsl.KOCSAMADLSEntities())
                {
                    var response = wsc.GetWorkflowIdListByUser(authHeader, request);
                    var workList = response.ToList();
                    for (int i = 0; i < workList.Count; i++)
                    {
                        var data = wsc.GetWorkflowDetailByUser(authHeader, workList[i].WorkflowId).FirstOrDefault();
                        if (data != null && (data.WorkflowStatusCode == "BIRINCISEVIYEKUYRUKTA" || data.WorkflowStatusCode == "IKINCISEVIYEKUYRUKTA"))
                        {
                            string smno = data.CustomerId + "";
                            bool register = false;
                            var cust = db.customer.Where(r => r.superonlineCustNo == smno && r.deleted == false).ToList();
                            foreach (var item in cust)
                            {
                                bool isSaved = false;
                                register = true;
                                var tasks = AdslTaskQueues.Where(r => r.Value.attachedobjectid == item.customerid && r.Value.taskid == 51).ToList();
                                foreach (var t in tasks)
                                {
                                    var id = t.Value.relatedtaskorderid ?? t.Key;
                                    if (AdslProccesses.ContainsKey(id) && (AdslProccesses[id].Last_Status == 0 || AdslProccesses[id].Last_Status == 3))
                                    {
                                        isSaved = true;
                                        break;
                                    }
                                    /*// bu tasklardan süreç id kısmı descriptionda kontrol edilecek varsa işlem yapılmayacak $#&sid$# sid: süreç id
                                    var descs = t.Value.description.Split(new[] { "$#&" }, StringSplitOptions.None);
                                    if (descs.Length > 2 && descs[1] == (data.WorkflowId + ""))
                                    {
                                        isSaved = true;
                                        break;
                                    }*/
                                }
                                if (isSaved)
                                {
                                    register = false;
                                    break;
                                }
                            }
                            if (register)
                            { // müşteri bizde var ve ikinci donanım taskı oluşacaksa
                                Models.Adsl.adsl_taskqueue newTask = new Models.Adsl.adsl_taskqueue();
                                newTask.taskid = 51; // 51 Bağlantı Problemi Taskı
                                newTask.attachedpersonelid = 1003; // 1003 Banur Aydın (Bağlantı Problemi sorumlu personel)
                                string appointment = data.WorkflowStartTime.ToString("yyyy-MM-dd HH':'mm':'ss");
                                newTask.appointmentdate = Convert.ToDateTime(appointment);
                                newTask.attachedobjectid = cust[0].customerid;
                                newTask.description = "$#&" + data.WorkflowId + "$#& Web servis aracılığı ile oluşturuldu.";

                                insertOnlyTaskqueue(newTask);
                            }
                            else if (cust.Count == 0)
                                customerInfo(null, data, 51, 1003); // 1003 Banur Aydın
                        }
                    }
                }
                #endregion

                #region Bağlantı Problemi Taskları
                request.TicketingTypeCode = "106";
                using (var wsc = new NetflowTellcomWSSoapClient())
                using (var db = new Models.Adsl.KOCSAMADLSEntities())
                {
                    var response = wsc.GetWorkflowIdListByUser(authHeader, request);
                    var workList = response.ToList();
                    for (int i = 0; i < workList.Count; i++)
                    {
                        var data = wsc.GetWorkflowDetailByUser(authHeader, workList[i].WorkflowId).FirstOrDefault();
                        if (data != null && (data.WorkflowStatusCode == "BIRINCISEVIYEKUYRUKTA" || data.WorkflowStatusCode == "IKINCISEVIYEKUYRUKTA"))
                        {
                            string smno = data.CustomerId + "";
                            bool register = false;
                            var cust = db.customer.Where(r => r.superonlineCustNo == smno && r.deleted == false).ToList();
                            foreach (var item in cust)
                            {
                                bool isSaved = false;
                                register = true;
                                var tasks = AdslTaskQueues.Where(r => r.Value.attachedobjectid == item.customerid && r.Value.taskid == 51).ToList();
                                foreach (var t in tasks)
                                {
                                    var id = t.Value.relatedtaskorderid ?? t.Key;
                                    if (AdslProccesses.ContainsKey(id) && (AdslProccesses[id].Last_Status == 0 || AdslProccesses[id].Last_Status == 3))
                                    {
                                        isSaved = true;
                                        break;
                                    }
                                    /*// bu tasklardan süreç id kısmı descriptionda kontrol edilecek varsa işlem yapılmayacak $#&sid$# sid: süreç id
                                    var descs = t.Value.description.Split(new[] { "$#&" }, StringSplitOptions.None);
                                    if (descs.Length > 2 && descs[1] == (data.WorkflowId + ""))
                                    {
                                        isSaved = true;
                                        break;
                                    }*/
                                }
                                if (isSaved)
                                {
                                    register = false;
                                    break;
                                }
                            }
                            if (register)
                            { // müşteri bizde var ve ikinci donanım taskı oluşacaksa
                                Models.Adsl.adsl_taskqueue newTask = new Models.Adsl.adsl_taskqueue();
                                newTask.taskid = 51; // 51 Bağlantı Problemi Taskı
                                newTask.attachedpersonelid = 1003; // 1003 Banur Aydın (Bağlantı Problemi sorumlu personel)
                                string appointment = data.WorkflowStartTime.ToString("yyyy-MM-dd HH':'mm':'ss");
                                newTask.appointmentdate = Convert.ToDateTime(appointment);
                                newTask.attachedobjectid = cust[0].customerid;
                                newTask.description = "$#&" + data.WorkflowId + "$#& Web servis aracılığı ile oluşturuldu.";

                                insertOnlyTaskqueue(newTask);
                            }
                            else if (cust.Count == 0)
                                customerInfo(null, data, 51, 1003); // 1003 Banur Aydın
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, null, 0, 1000 * 60 * 10);

        public async static void Register(HttpConfiguration config)
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
            builder.EntitySet<DTOs.Adsl.SKReport>("CagriSKReports");
            builder.EntitySet<DTOs.Adsl.SLBayiReport>("SLBayiReports");
            builder.EntitySet<DTOs.Adsl.SLKocReport>("SLKocReports");
            builder.EntitySet<DTOs.Adsl.SKPaymentReport>("SKPaymentReports");
            builder.EntitySet<DTOs.Adsl.SKStandbyTaskReport>("SKStandbyTaskReports");
            builder.EntitySet<DTOs.Adsl.SKClosedTasksReport>("SKClosedTasksReports");
            builder.EntitySet<DTOs.Adsl.SKStandbyTasksHours>("SKStandbyHoursReports");
            builder.EntitySet<DTOs.Adsl.PersonelsReport>("PersonelsReports");
            builder.EntitySet<DTOs.Adsl.ISSSuccessRate>("ISSSuccessRates");
            builder.EntitySet<DTOs.Adsl.SKRate>("SKRates");
            builder.EntitySet<DTOs.Adsl.InfoBayiReport>("InfoBayiReports");
            builder.EntitySet<DTOs.Adsl.StockMovementBackSeri>("StockMovementBackSeriReports");
            builder.EntitySet<DTOs.Cari.CariHareketReport>("CariHareketReports");
            builder.EntitySet<DTOs.Adsl.EvrakBasari>("OnlyDocSuccesReports");
            config.MapODataServiceRoute(
                routeName: "ODataRoute",
                routePrefix: "odata",
                model: builder.GetEdmModel()
            );
            TeknarProxyService.Start();
            await Task.WhenAll((new Task[] { updateCariData() ,loadAdslPaymentSystemTypes(), loadAdslPaymentSystem(), loadAdslIls(), loadAdslObjectTypes(), loadAdslTaskTypes(), loadAdslIlces(), updateAdslData()/*, updateFiberData()*/})).ConfigureAwait(false);
        }
    }
}