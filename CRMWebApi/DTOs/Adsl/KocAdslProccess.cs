using CRMWebApi.Models.Adsl;
using System.Collections.Generic;
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
        public int Last_TON // taskı yanlış sonucla ilerlerterek task türetiliyor sonra başka sonucla kapatılıp önceki türeyenler iptal edilince son yask olarak iptal alıyoruz processin devamını kacırıyoruz 
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
        //Bu sürecin SL süreleri
        public Dictionary<int, SLTime> SLs = new Dictionary<int, SLTime>();
        public void Update(adsl_taskqueue tq)
        {
            var taskType = WebApiConfig.AdslTasks[tq.taskid].tasktype;
            //Başlangıç Taskı ise
            if (WebApiConfig.AdslTaskTypes[taskType].startsProccess)
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

            if (WebApiConfig.AdslTaskSl.ContainsKey(tq.taskid)) Last_TON = tq.taskorderno;
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
                if(tq.appointmentdate.HasValue || tq.consummationdate.HasValue)
                foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][2])
                {
                    if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                    if (!SLs[sl].KStart.HasValue) SLs[sl].KStart = tq.appointmentdate ?? tq.consummationdate;
                    SLs[sl].CustomerId = tq.attachedobjectid.Value;
                }
                //Bayi SL Bitiş
                if (tq.consummationdate.HasValue)
                {
                    foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][1])
                    {
                        if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                        if (!SLs[sl].BEnd.HasValue) SLs[sl].BEnd = tq.consummationdate;
                    }
                    //Koç SL Bitiş
                    foreach (var sl in WebApiConfig.AdslTaskSl[tq.taskid][3])
                    {
                        if (!SLs.ContainsKey(sl)) SLs[sl] = new SLTime();
                        if (!SLs[sl].KEnd.HasValue) SLs[sl].KEnd = tq.consummationdate;
                    }
                }
            }
        }
    }
}