using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOatama
    {
        public int id { get; set; }
        public int? closedtasktype { get; set; }
        public int? formedtasktype { get; set; }
        public int? closedtask { get; set; }
        public int? formedtask { get; set; }
        public int? offpersonel { get; set; }
        public int? appointedpersonel { get; set; }

        public DTOtask adsl_taskclosed { get; set; }
        public DTOtask adsl_taskformed { get; set; }
        public DTOTaskTypes adsl_tasktypesclosed { get; set; }
        public DTOTaskTypes adsl_tasktypesformed { get; set; }
        public DTOpersonel adsl_personeloff { get; set; }
        public DTOpersonel adsl_personelappoint { get; set; }
    }
}