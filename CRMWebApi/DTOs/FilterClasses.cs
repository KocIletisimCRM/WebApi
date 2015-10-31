using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public enum filterOperators
    {
        foLess = 0,
        foLessOrEqual = 1,
        foEqual = 2,
        foGreater = 3,
        foGreaterOrEqual = 4,
        foBetween = 5,
        foLike = 6,
        foIn=7,
        foIsNull = 8
    }

    public class fieldFilter
    {
        private string fieldName { get; set; }
        private object filterValue { get; set; }
        private object filterValue2 { get; set; }
        private filterOperators filterOperator { get; set; }

        private static string[] operatorStrings = { "{0} < {1}", "{0} <= {1}", "{0} = {1}", "{0} > {1}", "{0} >= {1}", "{0} BETWEEN  {1} AND {2}", "{0} LIKE '%{1}%'", "{0} IN ({1})", "{0} is null" };

        private string getValueCompairer(object val)
        {
            var s = (val is string) || (val is DateTime) ? "'" : string.Empty;
            return string.Format("{0}{1}{0}", s, (val is Array) ? string.Join(",", val) : val.ToString().Replace("'","''"));
        }

        public fieldFilter(string name, object value, filterOperators op)
        {
            fieldName = name;
            if (name.Contains("date"))
            {
                filterValue = value.ToString().Split('-')[0];
                filterValue2 = value.ToString().Split('-')[1];
            }
            else
            filterValue = value;
            filterOperator = op;
        }

        public string combineWith(filterOperators op)
        {
            filterOperator = op;
            return combine();
        }

        public string combine()
        {
            switch (filterOperator)
            {
                case filterOperators.foLess:
                case filterOperators.foLessOrEqual:
                case filterOperators.foEqual:
                case filterOperators.foGreater:
                case filterOperators.foGreaterOrEqual:
                    return string.Format(operatorStrings[(int)filterOperator], fieldName, getValueCompairer(filterValue));
                case filterOperators.foBetween:
                        return string.Format(operatorStrings[(int)filterOperator], fieldName, getValueCompairer(filterValue), getValueCompairer(filterValue2));
                case filterOperators.foLike:
                    return string.Format(operatorStrings[(int)filterOperator], fieldName, (filterValue??string.Empty).ToString().Replace("'","''"));
                case filterOperators.foIn:
                    if (filterValue is Array)
                        return string.Format(operatorStrings[(int)filterOperator], fieldName,
                           string.Join(", ", (filterValue as Array)));
                    else
                        throw new Exception(" IN operatörü için filtre değeri Array tipinde olmalı!");
                case filterOperators.foIsNull:
                    return string.Format(operatorStrings[(int)filterOperator], fieldName);
                default:
                    throw new Exception("Bilinmeyen operatör tipi!");
            }
        }

        public static string operator &(fieldFilter f1, fieldFilter f2)
        {
            return string.Format("({0} and {1})", f1.combine(), f2.combine());
        }

        public static string operator |(fieldFilter f1, fieldFilter f2)
        {
            return string.Format("({0} or {1})", f1.combine(), f2.combine());
        }

    }

    public class filterSQL
    {
        static string sql = "SELECT * FROM {0}";
        private string tableName { get; set; }
        public string TableName { get { return tableName; } }

        private string keyFieldName { get; set; }
        public string KeyFieldName { get { return keyFieldName; } }

        public filterSQL(string tablename, string keyfieldname)
        {
            tableName = tablename;
            keyFieldName = keyfieldname;
        }

        private List<fieldFilter> _fieldFilters = new List<fieldFilter>();
        public bool hasFilterField { get { return _fieldFilters.Count > 0; } }
        public void addFieldFilter(fieldFilter f)
        {
            _fieldFilters.Add(f);
        }
        public void clearFilters()
        {
            _fieldFilters.Clear();
        }
        public virtual string get()
        {
            return _fieldFilters.Count > 0 ?
                string.Format("{0} WHERE ({1})", string.Format(sql, tableName), string.Join(" AND ", _fieldFilters.Select(f => f.combine()))) :
                string.Format(sql, tableName);
        }
    }
    
    public class lookupFilterSQL : filterSQL
    {
        private Dictionary<string, filterSQL> details = new Dictionary<string, filterSQL>();
        public Dictionary<string, filterSQL> Details { get { return details; } }

        protected Dictionary<string, string> getSubWithClaueses()
        {
            Dictionary<string, string> withClauses = new Dictionary<string, string>();
            foreach (var detail in details)
            {
                if (detail.Value is lookupFilterSQL)
                {
                    var ldetail = detail.Value as lookupFilterSQL;
                    foreach (var item in ldetail.getSubWithClaueses())
                        withClauses[item.Key] = item.Value;
                }
                withClauses[detail.Key] = string.Format("_{0} as ({1})", detail.Key, detail.Value.get());
            }
            return withClauses;
        }

        public lookupFilterSQL(string tablename, string keyfieldname) : base(tablename, keyfieldname) { }

        private string getSelectStatement()
        {
            return base.get();
        }

        public override string get()
        {
            List<string> whereClauses = new List<string>();
            foreach (var detail in Details)
            {
                whereClauses.Add(string.Format(" (EXISTS (SELECT * from _{0} WHERE _{0}.{1} = {2}.{3}))", detail.Key, detail.Value.KeyFieldName, TableName, detail.Key));
            }
            if (whereClauses.Count > 0)
                return string.Format("{0} {1} {2}", base.get(), hasFilterField ? "AND" : "WHERE", string.Join(" AND ", whereClauses));
            else return base.get();
        }

        public string getSQL()
        {
            return string.Format("{0} {1}", Details.Count > 0 ? string.Format("WITH {0}", string.Join(",", getSubWithClaueses().Values)) : "", get());
        }

        public string getPagingSQL(int pageNo, int rowsPerPage)
        {
            var selectstatement = get();
            var pos = selectstatement.IndexOf("*");
            var sqlwithpagingcolumn = selectstatement.Insert(pos, String.Format("ROW_NUMBER() OVER(ORDER BY {0} desc) ORDERNO, ", KeyFieldName));
            var withClauses = getSubWithClaueses();
            withClauses.Add("_paging", string.Format("_paging as ({0})", sqlwithpagingcolumn));
            return string.Format("{0} {1}", 
                string.Format("WITH {0}", string.Join(",", withClauses.Values)), 
                string.Format("SELECT * FROM _paging WHERE (CAST(ORDERNO/{0} as INT) = {1} - 1 OR {1} = 0)", rowsPerPage, pageNo));
        }

        public string getCountSQL() {
            var withClauses = getSubWithClaueses();
            withClauses.Add("_all", string.Format("_all as ({0})", get()));
            return string.Format("{0} {1}",
                string.Format("WITH {0}", string.Join(",", withClauses.Values)),
                "SELECT Count(*) FROM _all");
        }
    }
}