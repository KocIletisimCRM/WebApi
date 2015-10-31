using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    class DTOstockstatus
    {
        public int toobject { get; set; }
        public int toobjecttype { get; set; }
        public int stockcardid { get; set; }
        public int INPUT { get; set; }
        public int OUTPUT { get; set; }
        public Nullable<int> amount { get; set; }

        public List<string> serials { get; set; }
    }
}
