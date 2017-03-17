using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public class DTOSaveGlobalTask
    {
        public bool newcustomer { get; set; }
        public int? customerid { get; set; }
        public string customername { get; set; }
        public string gsm { get; set; }
        public string tc { get; set; }
        public int? attachedpersonelid { get; set; }
        public int? blockid { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public int taskid { get; set; }
        public string description { get; set; }
        public string fault { get; set; }
    }
}
