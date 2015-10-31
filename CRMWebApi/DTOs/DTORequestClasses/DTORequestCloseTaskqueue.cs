using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.DTORequestClasses
{
   public  class DTORequestCloseTaskqueue
    {
        public int campaignid { get; set; }
        public int [] selectedProductsIds { get; set; }
        public int taskorderno { get; set; }
    }
}
