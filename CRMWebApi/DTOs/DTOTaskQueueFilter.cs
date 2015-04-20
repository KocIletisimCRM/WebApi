using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class DTOTaskQueueFilter
    {
        public int[] TaskIds { get; set; }
        public string TaskName { get; set; }
        public int[] TaskTypeIds { get; set; }
        public string TaskTypeName { get; set; }
    }
}
