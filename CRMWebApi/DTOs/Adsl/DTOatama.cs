using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOatama
    {
        public int id { get; set; }
        public int? closedtasktype { get; set; }
        public int? formedtasktype { get; set; }
        public int? closedtask { get; set; }
        public int? formedtask { get; set; }
        public int? offpersonel { get; set; }
        public int? appointedpersonel { get; set; }
    }
}