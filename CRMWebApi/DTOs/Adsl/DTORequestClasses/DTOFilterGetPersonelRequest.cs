namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public interface IPersonelRequest
    {
        DTOFieldFilter personel { get; set; }
        DTOFieldFilter il { get; set; }
        DTOFieldFilter ilce { get; set; }
        DTOFilter getFilter();
    }

    public interface IAssistantPersonelRequest : IPersonelRequest { new DTOFilter getFilter(); }
    public interface IUpdatedByRequest : IPersonelRequest { new DTOFilter getFilter(); }

    public class DTOFilterGetPersonelRequest :DTORequestPagination, IPersonelRequest, IAssistantPersonelRequest, IUpdatedByRequest
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
        private DTOFieldFilter _il;
        public DTOFieldFilter il
        {
            get
            {
                return _il;
            }
            set
            {
                _il = value;
            }
        }
        private DTOFieldFilter _ilce;
        public DTOFieldFilter ilce
        {
            get
            {
                return _ilce;
            }
            set
            {
                _ilce = value;
            }
        }

        public DTOFilter getFilter()
        {
            DTOFilter filter = new DTOFilter("personel", "personelid");
            if (personel != null) filter.fieldFilters.Add(personel);
            if (il != null) filter.fieldFilters.Add(il);
            if (ilce != null) filter.fieldFilters.Add(ilce);
            return filter;
        }
    }
}