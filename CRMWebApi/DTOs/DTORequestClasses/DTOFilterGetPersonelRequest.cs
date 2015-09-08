using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.DTORequestClasses
{
    public interface IPersonelRequest
    {
        DTOFieldFilter personel { get; set; }
        DTOFilter getFilter();
    }

    public interface IAssistantPersonelRequest : IPersonelRequest { new DTOFilter getFilter(); }
    public interface IUpdatedByRequest : IPersonelRequest { new DTOFilter getFilter(); }

    public class DTOFilterGetPersonelRequest : IPersonelRequest, IAssistantPersonelRequest, IUpdatedByRequest
    {
        private DTOFieldFilter _personel;
        public DTOFieldFilter personel
        {
            get
            {
                return _personel;
            }
            set
            {
                _personel = value;
            }
        }

        public DTOFilter getFilter()
        {
            DTOFilter filter = new DTOFilter("personel", "personelid");
            if (personel != null) filter.fieldFilters.Add(personel);
            return filter;
        }
    }
}