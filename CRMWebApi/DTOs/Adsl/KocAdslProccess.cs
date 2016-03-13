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
        //Koç SL Başlangıç
        public DateTime KSL_S { get; set; }
        //Koç SL Bitiş
        public DateTime KSL_E { get; set; }
        //Bayi SL Başlangıç 
        public DateTime BSL_S { get; set; }
        //Bayi SL Bitiş 
        public DateTime BSL_E { get; set; }
        
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
            //Evrak Alma Taskı ise
            else if (taskType == 7)
            {

            }
            //Teslimat Taskı ise
            else if(taskType == 8)
            {

            }
        }
    }
}