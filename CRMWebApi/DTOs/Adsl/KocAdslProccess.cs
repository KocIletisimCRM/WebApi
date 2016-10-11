using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CRMWebApi.DTOs.Adsl
{
    public class KocAdslProccess
    {
        //Başlangıç Task Order No (Satış, Arıza gibi)
        public int S_TON { get; set; }
        //Randevu Task order No
        public int? Kr_TON { get; set; }
        //Kurulum Task Order No
        public int? K_TON { get; set; }
        //Kurulum Task Kapama Task Order No
        public int? Ktk_TON { get; set; }
        //Son Task Order No (Process'de dallanma oluşturup task kapamaya ulaşmayan tasklar alınmayacak)

        public bool proccessCancelled = false;
        private int last_TON = 0;
        public int Last_TON 
        {
            get { return last_TON; }
            set
            {
                last_TON = value;
                var last_tq = WebApiConfig.AdslTaskQueues[last_TON];
                var stateType = last_tq.status == null ? 0 : WebApiConfig.AdslStatus[last_tq.status.Value].statetype;
                if (stateType == 2)
                {
                    //proccessCancelled = true;
                    //SLs.Clear();  
                }
            }
        }
        //Last Taskın StateType'ni içerir
        public int Last_Status { get; set; }
        //Bu sürecin SL süreleri
        public Dictionary<int, SLTime> SLs = new Dictionary<int, SLTime>();
        public void Update(adsl_taskqueue tq)
        {
            var taskType = WebApiConfig.AdslTasks[tq.taskid].tasktype;
            int stataType = tq.status.HasValue ? WebApiConfig.AdslStatus.ContainsKey(tq.status.Value) ? WebApiConfig.AdslStatus[tq.status.Value].statetype.Value : 0 : 0;
            //Başlangıç Taskı ise
            if (WebApiConfig.AdslTaskTypes[taskType].startsProccess)
            {
                Last_Status = stataType;
                Last_TON = tq.taskorderno;
                S_TON = tq.taskorderno;
                Kr_TON = null;
                K_TON = null;
                Ktk_TON = null;
            }
            //Randevu Taskı ise
            else if (taskType == 2)
            {
                Last_Status = stataType;
                Last_TON = tq.taskorderno;
                Kr_TON = tq.taskorderno;
                K_TON = null;
                Ktk_TON = null;
            }
            //Kurulum ve Rand.suz Kurulum Taskı ise
            else if (taskType == 3 || taskType == 4)
            {
                Last_Status = stataType;
                Last_TON = tq.taskorderno;
                K_TON = tq.taskorderno;
                Ktk_TON = null;
            }
            //SOL Kapama Taskı ise
            else if (taskType == 5)
            {
                Last_Status = stataType;
                Last_TON = tq.taskorderno;
                Ktk_TON = tq.taskorderno;
            }

            if (WebApiConfig.AdslTaskSl.ContainsKey(tq.taskid))
            {
                Last_Status = stataType;
                Last_TON = tq.taskorderno;
            }
            if (proccessCancelled) return;
            // Task SL taskı mı ?
            if (WebApiConfig.AdslTaskSl.ContainsKey(tq.taskid))
            {
                //Bayi SL Başlangıç
                if (tq.attachmentdate.HasValue)
                    foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][0])
                    {
                        if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                        if (!SLs[sl].BStart.HasValue)
                        {
                            SLs[sl].BStart = tq.attachmentdate;
                            SLs[sl].BayiID = tq.attachedpersonelid;
                            SLs[sl].CustomerId = tq.attachedobjectid.Value;
                        }
                    }
                //Koç SL Başlangıç
                if (tq.appointmentdate.HasValue || tq.consummationdate.HasValue)
                    foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][2])
                    {
                        if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                        if (!SLs[sl].KStart.HasValue) SLs[sl].KStart = tq.appointmentdate ?? tq.consummationdate;
                        SLs[sl].CustomerId = tq.attachedobjectid.Value;
                    }
                //Bayi SL Bitiş (9156 iptal onayı bekliyor durumu sl sonlandırmamalı geçici olarak id ekledim çözüm sorulacak)
                if (tq.consummationdate.HasValue && tq.status != null && WebApiConfig.AdslStatus.ContainsKey(tq.status.Value) && WebApiConfig.AdslStatus[tq.status.Value].statetype.Value != 2 && tq.status != 9156) 
                {
                    foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][1])
                    {
                        //if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                        if (SLs.ContainsKey(sl) && !SLs[sl].BEnd.HasValue) SLs[sl].BEnd = tq.consummationdate;
                    }
                    //Koç SL Bitiş
                    foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][3])
                    {
                        //if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                        if (SLs.ContainsKey(sl) && !SLs[sl].KEnd.HasValue) SLs[sl].KEnd = tq.consummationdate;
                    }
                }
            }
        }

        public static void updateProccesses(Queue<int> proccessIds)
        {
            WebApiConfig.raporLogs(DateTime.Now, true, "Update Proccess");
            var i = 0;
            Stopwatch stw = new Stopwatch();
            while (proccessIds.Count > 0)
            {
                var proccessId = proccessIds.Dequeue();
                var subTasks = new Queue<int>();
                subTasks.Enqueue(proccessId);
                while (subTasks.Count > 0)
                {
                    stw.Start();
                    i++;
                    var taskId = subTasks.Dequeue();
                    //if (WebApiConfig.AdslTaskQueues.ContainsKey(taskId) && WebApiConfig.AdslProccessIndexes.ContainsKey(taskId) && WebApiConfig.AdslProccesses.ContainsKey(WebApiConfig.AdslProccessIndexes[taskId]))
                    KocAdslProccess kap = null;
                    adsl_taskqueue atq = null;
                    int proccessID = 0;
                    if (WebApiConfig.AdslProccessIndexes.TryGetValue(taskId, out proccessID))
                        if (WebApiConfig.AdslTaskQueues.TryGetValue(taskId, out atq))
                            if (WebApiConfig.AdslProccesses.TryGetValue(proccessID, out kap))
                            {
                                kap.Update(atq);
                            }
                    //foreach (var item in WebApiConfig.AdslTaskQueues.Where(r => r.Value.previoustaskorderid == taskId).OrderBy(r => r.Value.taskorderno)) subTasks.Enqueue(item.Value.taskorderno);
                    ConcurrentBag<int> stks = new ConcurrentBag<int>();
                    if (WebApiConfig.AdslSubTasks.TryGetValue(taskId, out stks))
                        foreach (var item in stks) subTasks.Enqueue(item);
                    stw.Stop();
                    var ttt = stw.Elapsed;
                    var btt = stw.ElapsedMilliseconds / (i);
                }
            }
            WebApiConfig.raporLogs(DateTime.Now, false, "Update Proccess");
        }
    }
}