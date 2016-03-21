using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOAtama
    {
        public int id { get; set; }
        public int? closedtasktype { get; set; }
        public int? formedtasktype { get; set; }
        public int? closedtask { get; set; }
        public int? formedtask { get; set; }
        public int? offpersonel { get; set; }
        public int? appointedpersonel { get; set; }
        public int?[] formedtaskarray { get; set; }

        public DTOtask taskclosed { get; set; }
        public DTOtask taskformed { get; set; }
        public DTOTaskTypes tasktypesclosed { get; set; }
        public DTOTaskTypes tasktypesformed { get; set; }
        public DTOpersonel personeloff { get; set; }
        public DTOpersonel personelappointed { get; set; }
    }
}