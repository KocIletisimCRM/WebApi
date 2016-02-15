using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class KocAdslProccess
    {
        public int S_TON { get; set; }
        public int? Kr_TON { get; set; }
        public int? K_TON { get; set; }
        public int? Ktk_TON { get; set; }

        public void Update(DTOtaskqueue tq)
        {

            switch (WebApiConfig.AdslTasks[tq.task.taskid].tasktypes.TaskTypeId)
            {
                case 1:
                    {
                        S_TON = tq.taskorderno;
                        Kr_TON = null;
                        K_TON = null;
                        Ktk_TON = null;
                        break;
                    }
                case 2:
                    {
                        Kr_TON = tq.taskorderno;
                        K_TON = null;
                        Ktk_TON = null;
                        break;
                    }
                case 3:
                case 4:
                    {
                        K_TON = tq.taskorderno;
                        Ktk_TON = null;
                        break;
                    }
                case 5:
                    {
                        Ktk_TON = tq.taskorderno;
                        break;
                    }
                default:
                    break;
            }
        }
    }
}