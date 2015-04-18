using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOsite
    {
        public int siteid { get; set; }
        public string sitename { get; set; }
        public string siteaddress { get; set; }
        public string region { get; set; }
        public string sitedistrict { get; set; }
        public string siteregioncode { get; set; }
        public string description { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public DTOpersonel updatedpersonel  { get; set; }
        public Nullable<bool> deleted { get; set; }
    
    }
}