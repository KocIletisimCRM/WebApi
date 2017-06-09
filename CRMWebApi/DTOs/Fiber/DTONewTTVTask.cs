using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRMWebApi.Models.Fiber;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTONewTTVTask
    {
        public customer customer { get; set; }
        public int customerid { get; set; } // yerine yeni müşteri oluşacak cid
        public DateTime? creationdate { get; set; }
        public int? attachedpersonelid { get; set; }
        public int taskid { get; set; }
        public string description { get; set; } // task açıklaması
    }
}