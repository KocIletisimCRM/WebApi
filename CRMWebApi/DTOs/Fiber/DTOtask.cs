using System;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOtask
    {
        public int taskid { get; set; }
        public string taskname { get; set; }
        public Nullable<double> performancescore { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public int? attachableobjecttype { get; set; }
        public int ? attachablepersoneltype { get; set; }
        public DTOpersonel updatedpersonel { get; set; }
        public Nullable<bool> deleted { get; set; }
        public string description { get; set; }

        public DTOTaskTypes tasktypes { get; set; }
        public DTOobjecttypes objecttypes { get; set; }
        public DTOobjecttypes personeltypes { get; set; }
    }
}