using System;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOcustomer_status
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}