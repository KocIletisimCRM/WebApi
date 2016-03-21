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
        //Bu sürecin SL süreleri
        public Dictionary<int, SLTime> SLs = new Dictionary<int, SLTime>();        
        public void Update(DTOtaskqueue tq)
        {
            var taskType = WebApiConfig.AdslTasks[tq.task.taskid].tasktypes.TaskTypeId;

            //Başlangıç Taskı ise
            if (WebApiConfig.ADSLProccessStarterTaskTypes.Contains(taskType))
            {
                S_TON = tq.taskorderno;
                Kr_TON = null;
                K_TON = null;
                Ktk_TON = null;
            }
            //Randevu Taskı ise
            else if (taskType == 2)
            {
                Kr_TON = tq.taskorderno;
                K_TON = null;
                Ktk_TON = null;
            }
            //Kurulum ve Rand.suz Kurulum Taskı ise
            else if (taskType == 3 || taskType == 4)
            {
                K_TON = tq.taskorderno;
                Ktk_TON = null;
            }
            //SOL Kapama Taskı ise
            else if (taskType == 5)
            {
                Ktk_TON = tq.taskorderno;
            }

            if (WebApiConfig.AdslTaskSl.ContainsKey(tq.task.taskid))
            {
                //Bayi SL Başlangıç
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.task.taskid][0])
                {
                    if (!SLs[sl].BStart.HasValue)
                    {
                        SLs[sl].BStart = tq.attachmentdate;
                        SLs[sl].BayiID = tq.attachedpersonel.personelid;
                    }
                }
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.task.taskid][1])
                {
                    if (!SLs[sl].BEnd.HasValue) SLs[sl].BEnd = tq.consummationdate;
                }
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.task.taskid][2])
                {
                    if (!SLs[sl].KStart.HasValue) SLs[sl].KStart = tq.appointmentdate;
                }
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.task.taskid][3])
                {
                    if (!SLs[sl].KEnd.HasValue) SLs[sl].KEnd = tq.consummationdate;
                }
            }
        }
    }
}