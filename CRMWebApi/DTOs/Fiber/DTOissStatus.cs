using System;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOissStatus
    {
        public int id { get; set; }
        public string issText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}