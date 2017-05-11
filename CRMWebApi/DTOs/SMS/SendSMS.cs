using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CRMWebApi.DTOs.SMS
{
    public class SendSMS
    {
        //public 
    }

    public class QueryResponse
    {
        public int MessageId { get; set; }
        public Status Status { get; set; }
    }

    public class Status
    {
        public int Code { get; set; }
        public string Description { get; set; }
    }
}