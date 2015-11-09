using System;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOgsmStatus
    {
        public int id { get; set; }
        public string gsmText { get; set; }
        public Nullable<int> deleted { get; set; }
    }
}