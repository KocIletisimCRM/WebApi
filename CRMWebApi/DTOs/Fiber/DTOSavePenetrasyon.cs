using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.Fiber
{
   public class DTOSavePenetrasyon
    {
      public  int blockid { get; set; }
      public int attachedpersonelid { get; set; }
      public DateTime date { get; set; }
    }
}
