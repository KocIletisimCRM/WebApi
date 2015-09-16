using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOCloseTaskQueue :DTOtaskqueue
    {
      
            public int tqid { get; set; }
            public int campaignid { get; set; }
            public int[] selectedProducts { get; set; }
  
    }
}