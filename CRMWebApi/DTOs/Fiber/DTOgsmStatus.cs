using System;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOgsmStatus
    {
        public int id { get; set; }
        public string gsmText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}