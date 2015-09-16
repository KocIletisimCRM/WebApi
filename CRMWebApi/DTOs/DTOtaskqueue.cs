using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOtaskqueue
    {
        public int taskorderno { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> attachmentdate { get; set; }
        public Nullable<System.DateTime> appointmentdate { get; set; }
        public Nullable<System.DateTime> consummationdate { get; set; }
        public string description { get; set; }
        public int ? previoustaskorderid {get;set;}
        public int? relatedtaskorderid { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<bool> deleted { get; set; }
        public string fault { get; set; }
        public DTOtask task { get; set; }
        public DTOpersonel asistanPersonel { get; set; }
        public DTOpersonel Updatedpersonel { get; set; }
        public DTOpersonel attachedpersonel { get; set; }
       // public DTOsite site { get; set; }
      //  public DTOblock block { get; set; }
        public BaseAttachedObject attachedobject { get; set; }
        public DTOtaskstatepool taskstatepool { get; set; }

       

    }
}