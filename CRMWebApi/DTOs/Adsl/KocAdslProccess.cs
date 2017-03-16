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
            List<int> containLastTask = new List<int> { 7, 10 }; // task type ana hiyerarşi içinde diğilse ve task sl içermiyorsa last task atanması için (Hüseyin KOZ)

            // Model Güncelleme Talebi taskları ve kurulum taskları için
            if(taskType == 3)
            {
                if(tq.taskid == 153 || tq.taskid == 155)
                {
                    if(WebApiConfig.AdslPersonels.ContainsKey(tq.attachedpersonelid ?? 0))
                    {
                        if (stataType == 1 || stataType == 3)
                        {
                            WebApiConfig.AdslPersonels[tq.attachedpersonelid.Value].T_153_155.TryAdd(tq.relatedtaskorderid ?? tq.taskorderno, true);
                            WebApiConfig.K_PersonelForProccess.TryAdd(tq.relatedtaskorderid ?? tq.taskorderno, tq.attachedpersonelid.Value);
                        }
                    }
                }
                else
                {
                    if (WebApiConfig.AdslPersonels.ContainsKey(tq.attachedpersonelid ?? 0))
                    {
                        bool b;
                        int x;
                        if (stataType == 1 || stataType == 2 || stataType == 3) // task iptale çekilince de işlem bitirilmeli iptal ekledim (Hüseyin KOZ) 02.11.2016
                        {
                            WebApiConfig.AdslPersonels[tq.attachedpersonelid.Value].T_153_155.TryRemove(tq.relatedtaskorderid ?? tq.taskorderno, out b);
                            WebApiConfig.K_PersonelForProccess.TryRemove(tq.relatedtaskorderid ?? tq.taskorderno, out x);
                        }
                    }
                }
            }

            //Başlangıç Taskı ise
            if (WebApiConfig.AdslTaskTypes[taskType].startsProccess)
            {
                Last_Status = stataType == 1 ? 0 : stataType;
                Last_TON = tq.taskorderno;
                S_TON = tq.taskorderno;
                Kr_TON = null;
                K_TON = null;
                Ktk_TON = null;
            }
            //Randevu Taskı ise
            else if (taskType == 2)
            {
                Last_Status = stataType == 1 ? 0 : stataType;
                Last_TON = tq.taskorderno;
                Kr_TON = tq.taskorderno;
                K_TON = null;
                Ktk_TON = null;
            }
            //Kurulum ve Rand.suz Kurulum Taskı ise
            else if (taskType == 3 || taskType == 4)
            {
                Last_Status = stataType == 1 ? 0 : stataType;
                Last_TON = tq.taskorderno;
                K_TON = tq.taskorderno;
                Ktk_TON = null;
            }
            //SOL Kapama Taskı ise
            if (WebApiConfig.AdslTaskTypes[taskType].endsProccess)
            {
                Last_Status = stataType;
                Last_TON = tq.taskorderno;
                Ktk_TON = tq.taskorderno;
            }

            if (WebApiConfig.AdslTaskSl.ContainsKey(tq.taskid) || containLastTask.Contains(taskType))
            {
                Last_Status = stataType == 1 ? 0 : stataType;
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
            var i = 0;
            Stopwatch stw = new Stopwatch();
            while (proccessIds.Count > 0)
            {
                var proccessId = proccessIds.Dequeue();
                //Proccess yeniden oluşturulduğunda personel üzerindeki engeller kaldırılıyor (Ali AKAY) 01.11.2016
                int x;
                bool b;
                WebApiConfig.K_PersonelForProccess.TryRemove(proccessId, out x);
                if (WebApiConfig.AdslPersonels.ContainsKey(x)) WebApiConfig.AdslPersonels[x].T_153_155.TryRemove(proccessId, out b);
                
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
                    // AdslProccessIndexes dictionary nesnesi atrık kullanılmıyor (Hüseyin KOZ) 01.11.2016
                    //int proccessID = 0;
                    //if (WebApiConfig.AdslProccessIndexes.TryGetValue(taskId, out proccessID))
                    if (WebApiConfig.AdslTaskQueues.TryGetValue(taskId, out atq))
                        if (WebApiConfig.AdslProccesses.TryGetValue(atq.relatedtaskorderid ?? taskId, out kap))
                        {
                            kap.Update(atq);
                        }
                    //foreach (var item in WebApiConfig.AdslTaskQueues.Where(r => r.Value.previoustaskorderid == taskId).OrderBy(r => r.Value.taskorderno)) subTasks.Enqueue(item.Value.taskorderno);
                    HashSet<int> stks = new HashSet<int>();
                    if (WebApiConfig.AdslSubTasks.TryGetValue(taskId, out stks))
                        foreach (var item in stks) subTasks.Enqueue(item);
                    stw.Stop();
                    var ttt = stw.Elapsed;
                    var btt = stw.ElapsedMilliseconds / (i);
                }
            }
        }
    }
}