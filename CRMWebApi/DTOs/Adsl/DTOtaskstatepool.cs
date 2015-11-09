using System;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOtaskstatepool
    {

        public int taskstateid { get; set; }
        public string taskstate { get; set; }
        public Nullable<int> statetype { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public DTOpersonel updatedpersonel { get; set; }
        public Nullable<bool> deleted { get; set; }
     
    }
}