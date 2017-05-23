using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOMultiEditBlockDate
    {
        public List<int> BIDS { get; set; } // Block ids
        public DateTime FSD { get; set; } // Fiber Start Date
        public DateTime? SOSR { get; set; } // SO Sale Ready
        public DateTime? KSR { get; set; } // Koç Sale Ready
        public bool SSC { get; set; } // SO Sale Date Check
        public bool KSC { get; set; } // Koc Sale Date Check
    }
}