using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOtask
    {
        public int taskid { get; set; }
        public string taskname { get; set; }
        public Nullable<int> attachablepersoneltype { get; set; }
        public Nullable<int> attachableobjecttype { get; set; }
        public Nullable<double> performancescore { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public DTOpersonel updatedpersonel { get; set; }
        public Nullable<bool> deleted { get; set; }

        public DTOTaskTypes tasktypes { get; set; }
    }
}