using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class Filter
    {
        public int[] Ids { get; set; }
        public string Name { get; set; }
        public int[] TypeIds { get; set; }
        public string TypeName { get; set; }
 
    }


    public class DTOTaskQueueFilter
    {
        public Filter taskFilter { get; set; }
        public Filter attachedObjectFilter { get; set; }
        //public int[] attachedObjectIds { get; set; }
        public Filter personelFilter { get; set; }
        public Filter statusFilter { get; set; }
        public DateTime attachmentDate { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime consummationDate { get; set; }
    }
}
