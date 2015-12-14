using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOGetDocumentIdsRequest
    {
        public int? taskid { get; set; }
        public int? taskstate { get; set; }
        public int? campaignid { get; set; }
        public int? [] productIds { get; set; }
    }
}
