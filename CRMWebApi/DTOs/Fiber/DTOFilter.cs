using System.Collections.Generic;

namespace CRMWebApi.DTOs.Fiber
{
    public class DTOFieldFilter
    {
        public string fieldName { get; set; }
        public int op { get; set; }
        public object value { get; set; }
    }

    public class DTOFilter
    {
        public string tableName { get; set; }
        public string keyField { get; set; }
        private Dictionary<string, DTOFilter> _subTables = new Dictionary<string, DTOFilter>();
        public Dictionary<string, DTOFilter> subTables { 
            get { return _subTables; }
            set { _subTables = new Dictionary<string, DTOFilter>(value); }
        }

        private List<DTOFieldFilter> _fieldFilters = new List<DTOFieldFilter>();
        public List<DTOFieldFilter> fieldFilters {
            get { return _fieldFilters; }
            set { _fieldFilters = new List<DTOFieldFilter>(value); }
        }

        ~DTOFilter()
        {
            _subTables.Clear();
            _subTables = null;
            _fieldFilters.Clear();
            _fieldFilters = null;
        } 

        private lookupFilterSQL getLookupFilter() 
        {
            var lookupSql = new lookupFilterSQL(tableName,keyField);
            foreach (var filter in fieldFilters)
            {
                lookupSql.addFieldFilter(new fieldFilter(filter.fieldName,filter.value,(filterOperators)filter.op));
                
            }
            foreach (var subTable in subTables)
            {
                lookupSql.Details.Add(subTable.Key,subTable.Value.getLookupFilter());
            }
            return lookupSql;
        }

        public DTOFilter() { }
        public DTOFilter(string tn, string kf)
        {
            tableName = tn;
            keyField = kf;
        }

        public string getFilterSQL()
        {
            return getLookupFilter().getSQL();
        }

        public string getPagingSQL(int pageNo, int rowsPerPage)
        {
            return getLookupFilter().getPagingSQL(pageNo, rowsPerPage);
        }

        public string getCountSQL()
        {
            return getLookupFilter().getCountSQL();
        }

    }

}
