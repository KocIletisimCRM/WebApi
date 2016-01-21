using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.Fiber
{
  public  class DTOcustomerdocument
    {
        public int id { get; set; }
        public Nullable<int> customerid { get; set; }
        public Nullable<int> attachedobjecttype { get; set; }
        public Nullable<int> taskqueueid { get; set; }
        public Nullable<int> documentid { get; set; }
        public string documenturl { get; set; }
        public Nullable<System.DateTime> receiptdate { get; set; }
        public Nullable<System.DateTime> deliverydate { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
        public DTOdocument fiber_document { get; set; }
    }
}
