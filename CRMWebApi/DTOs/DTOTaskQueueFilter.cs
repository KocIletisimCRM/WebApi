using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class DTOTaskQueueFilter
    {
        public int[] taskIds { get; set; }
        public string taskName { get; set; }
        public int[] taskTypeIds { get; set; }
        public string taskTypeName { get; set; }
        public int[] attachedObjectIds { get; set; }
        public int[] personelIds { get; set; }
        public string personelName { get; set; }
        public int[] statusIds { get; set; }
        public string status { get; set; }
        public DateTime attachmentDate { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime consummationDate { get; set; }
    }
}
