using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
  public   class DTOProduct_service
    {
        public int productid { get; set; }
        public string productname { get; set; }
        public string category { get; set; }
        public string automandatorytasks { get; set; }
        public Nullable<int> maxduration { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
    }
}
