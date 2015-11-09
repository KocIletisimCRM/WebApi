using System;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOtaskstatematches
    {
        public int id { get; set; }

        public string automandatorytasks { get; set; }
        public string autooptionaltasks { get; set; }
        public string stockcards { get; set; }
        public string documents { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }

        public DTOtask  task { get; set; }
        public DTOtaskstatepool  taskstatepool { get; set; }
    }
}
