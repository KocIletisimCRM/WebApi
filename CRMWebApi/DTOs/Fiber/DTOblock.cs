using System;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOblock :BaseAttachedObject
    {
        public int blockid { get; set; }
        public string blockname { get; set; }
        public DTOsite site { get; set; }
        public string groupid { get; set; }
        public Nullable<int> hp { get; set; }
        public string telocadia { get; set; }
        public string projectno { get; set; }
        public Nullable<System.DateTime> readytosaledate { get; set; }
        public Nullable<System.DateTime> sosaledate { get; set; }
        public Nullable<System.DateTime> kocsaledate { get; set; }
        public DTOpersonel salespersonel { get; set; }
        public string superintendent { get; set; }
        public string superintendentcontact { get; set; }
        public string cocierge { get; set; }
        public string cociergecontact { get; set; }
        public string verticalproductionline { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public DTOpersonel updatedpersonel { get; set; }
        public Nullable<bool> deleted { get; set; }
        public string binakodu { get; set; }
        public string locationid { get; set; }
        public string objid { get; set; }
    
    }
}