using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class DTOcampaigns
    {
        public int id { get; set; }
        public string category { get; set; }
        public string subcategory { get; set; }
        public string name { get; set; }
        public string products { get; set; }
        public string documents { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<bool> deleted { get; set; }
    }
}
