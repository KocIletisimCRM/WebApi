using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOgetPersonelStockFiber_Result
    {

        public int stockid { get; set; }
        public string productname { get; set; }
        public string category { get; set; }
        public Nullable<bool> hasserial { get; set; }
        public string unit { get; set; }
        public string description { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
        public Nullable<int> amount { get; set; }
        public List<string> serials { get; set; }
    }
}
