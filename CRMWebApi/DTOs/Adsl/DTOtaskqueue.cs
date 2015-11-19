using System;
using System.Collections.Generic;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOtaskqueue
    {
        public int taskorderno { get; set; }
        public Nullable<DateTime> creationdate { get; set; }
        public Nullable<DateTime> attachmentdate { get; set; }
        public Nullable<DateTime> appointmentdate { get; set; }
        public Nullable<DateTime> consummationdate { get; set; }
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

        public DTOcustomer attachedcustomer { get; set; }
        public DTOtaskstatepool taskstatepool { get; set; }
        public List<object> customerproduct { get; set; }
        public List<object> stockmovement { get; set; }
        public List<object> stockcardlist { get; set; }
        public List<object> documentlist { get; set; }
    }
}