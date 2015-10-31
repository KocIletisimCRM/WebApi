using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs.DTORequestClasses
{
   public class DTOGetDocumentFilter :DTORequestPagination
    {

        public DTOFieldFilter documentname { get; set; }
        public DTOFieldFilter documentdescription { get; set; }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("document", "documentid");

            if (documentname != null)
            {
                filter.fieldFilters.Add(documentname);
            }
            if (documentdescription != null)
            {
                filter.fieldFilters.Add(documentdescription);
            }

            return filter;
        }
    }
}
