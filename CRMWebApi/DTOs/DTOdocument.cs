using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class DTOdocument
    {
        public int documentid { get; set; }
        public string documentname { get; set; }
        public string documentdescription { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
    }
}
