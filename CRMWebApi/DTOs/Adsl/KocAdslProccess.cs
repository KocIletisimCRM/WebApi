using CRMWebApi.Models.Adsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public int Last_TON { get; set; }
        //Bu sürecin SL süreleri
        public Dictionary<int, SLTime> SLs = new Dictionary<int, SLTime>();        
        public void Update(adsl_taskqueue tq)
        {
            var taskType = WebApiConfig.AdslTasks[tq.taskid].tasktype;
            //Başlangıç Taskı ise
            if (WebApiConfig.ADSLProccessStarterTaskTypes.Contains(taskType))
            {
                Last_TON = tq.taskorderno;
                S_TON = tq.taskorderno;
                Kr_TON = null;
                K_TON = null;
                Ktk_TON = null;
            }
            //Randevu Taskı ise
            else if (taskType == 2)
            {
                Last_TON = tq.taskorderno;
                Kr_TON = tq.taskorderno;
                K_TON = null;
                Ktk_TON = null;
            }
            //Kurulum ve Rand.suz Kurulum Taskı ise
            else if (taskType == 3 || taskType == 4)
            {
                Last_TON = tq.taskorderno;
                K_TON = tq.taskorderno;
                Ktk_TON = null;
            }
            //SOL Kapama Taskı ise
            else if (taskType == 5)
            {
                Last_TON = tq.taskorderno;
                Ktk_TON = tq.taskorderno;
            }
            // Task SL taskı mı ?
            if (WebApiConfig.AdslTaskSl.ContainsKey(tq.taskid))
            { 
              //Bayi SL Başlangıç
                Last_TON = tq.taskorderno;
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][0])
                {
                    if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                    if (!SLs[sl].BStart.HasValue)
                    {
                        SLs[sl].BStart = tq.attachmentdate;
                        //SLs[sl].BayiID = tq.attachedpersonel.personelid; (tq.attachedpersonel olmadıgından hata veriyordu tasklar gelmiyordu bu sebeple kapattım)
                    }
                }
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][1])
                {
                    if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                    if (!SLs[sl].BEnd.HasValue) SLs[sl].BEnd = tq.consummationdate;
                }
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][2])
                {
                    if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                    if (!SLs[sl].KStart.HasValue) SLs[sl].KStart = tq.appointmentdate;
                }
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][3])
                {
                    if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                    if (!SLs[sl].KEnd.HasValue) SLs[sl].KEnd = tq.consummationdate;
                }
            }
        }
    }
}