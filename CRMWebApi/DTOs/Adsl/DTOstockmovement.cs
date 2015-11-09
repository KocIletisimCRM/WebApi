using System;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOstockmovement
    {
        public int movementid { get; set; }
        public Nullable<int> relatedtaskqueue { get; set; }
        public Nullable<int> fromobjecttype { get; set; }
        public Nullable<int> fromobject { get; set; }
        public Nullable<int> toobjecttype { get; set; }
        public Nullable<int> toobject { get; set; }
        public Nullable<int> stockcardid { get; set; }
        public Nullable<int> amount { get; set; }
        public string serialno { get; set; }
        public Nullable<System.DateTime> movementdate { get; set; }
        public Nullable<System.DateTime> confirmationdate { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }

        public DTOpersonel frompersonel { get; set; }
        public DTOstockcard stockcard { get; set; }
        public DTOpersonel topersonel { get; set; }
        public DTOcustomer tocustomer { get; set; }
        public DTOcustomer fromcustomer { get; set; }
    }
}
