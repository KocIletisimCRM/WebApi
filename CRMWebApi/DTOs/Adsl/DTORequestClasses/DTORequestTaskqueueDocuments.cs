using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTORequestTaskqueueDocuments:DTORequestTaskqueueStockMovements
    {
        public bool isSalesTask { get; set; }
        public int? campaignid { get; set; }
        public List<int> customerproducts { get; set; }
    }
}
