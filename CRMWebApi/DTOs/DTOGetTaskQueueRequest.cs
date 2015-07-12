using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class DTOGetTaskQueueRequest
    {
        public int pageNo { get; set; }
        public int rowsPerPage { get; set; }
        public DTOFilter filter { get; set; }
    }
}
