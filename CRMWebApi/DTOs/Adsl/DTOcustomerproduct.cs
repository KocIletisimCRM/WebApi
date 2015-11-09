using System;

namespace CRMWebApi.DTOs.Adsl
{
    public  class DTOcustomerproduct
    {
        public int id { get; set; }
        public Nullable<int> taskid { get; set; }
        public Nullable<int> customerid { get; set; }
        public Nullable<int> productid { get; set; }
        public Nullable<int> campaignid { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }

        public DTOcampaigns campaigns { get; set; }
    }
}
