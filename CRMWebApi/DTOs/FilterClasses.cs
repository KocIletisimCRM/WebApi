using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class BaseFilterList
    {
        protected string xmlToTable = @"
            Select x.t.value('data(.)', 'int') id 
            From (Select Cast(@filterXML as XML) as ids) t
            Cross Apply t.ids.nodes('/id') x(t)
        "; 
        protected string baseSQL = @"
            with
                {0}
	            w0 as (
		            select {1} from {2}
		            {3}
	            )
	        SELECT {1}
	        FROM w0 ";
        protected List<int> ids = new List<int>();

        public BaseFilterList() { }

        public BaseFilterList(IEnumerable<int> idArray)
        {
            applyFilter(idArray);
        }

        ~BaseFilterList()
        {
            ids.Clear();
            ids = null;
        }

        public string getFilterXML()
        {
            return (ids.Count == 0) ? null : string.Join(string.Empty, ids.Select(i => string.Format("<id>{0}</id>", i)));
        }

        protected void setIds(IEnumerable<int> idArray)
        {
            ids.Clear();
            ids.AddRange(idArray);
        }

        public BaseFilterList applyFilter(IEnumerable<int> idArray)
        {
            if (idArray == null || idArray.Count() == 0) return this;
            setIds((ids.Count > 0) ? ids.Intersect(idArray) : idArray);
            return this;
        }

        protected IEnumerable<int> getFilterIds(string withClauses, string idFieldName, string tableName, string whereClauses, object[] sqlParams)
        {
            using (var db = new CRMEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                string preperedSQL = string.Format(baseSQL, withClauses, idFieldName, tableName, whereClauses);
                return db.Database.SqlQuery<int>(preperedSQL, sqlParams);
            }
        }
    }

    public class TaskTypeFilter : BaseFilterList
    {
        public TaskTypeFilter(IEnumerable<int> idArray)
            : base(idArray)
        {

        }

        public string getFilterXML(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return getFilterXML();
            string whereClauses = string.Format("Where TaskTypeName like '%{0}%'", filterText);
            var filteredIds = getFilterIds(null, "TaskTypeId", "tasktypes", whereClauses, new object[] { });
            return applyFilter(filteredIds).getFilterXML();
        }
    }

    public class TaskFilter : BaseFilterList
    {
        public TaskFilter(int[] idArray)
            : base(idArray)
        {
        }

        public TaskFilter ApplyFilterByTaskTypes(IEnumerable<int> idArray)
        {
            if (idArray == null || idArray.Count() == 0) return this;
            var taskTypeFilter = new TaskTypeFilter(idArray);
            string preparedWithSQL = string.Format("taskTypeIds as ({0}),\r\n", xmlToTable);
            string whereClause = "Where Exists (Select * from taskTypeIds tti Where tti.id=task.tasktype)";
            SqlParameter param = new SqlParameter("filterXML", taskTypeFilter.getFilterXML());
            var filteredIds = getFilterIds(preparedWithSQL, "taskid", "task", whereClause, new object[] { param });
            applyFilter(filteredIds);
            return this;
        }

        public TaskFilter ApplyFilterByTaskTypeName(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return this;
            var taskTypeFilter = new TaskTypeFilter(null);
            string preparedWithSQL = string.Format("taskTypeIds as ({0}),\r\n", xmlToTable);
            string whereClause = "Where Exists (Select * from taskTypeIds tti Where tti.id=task.tasktype)";
            SqlParameter param = new SqlParameter("filterXML", taskTypeFilter.getFilterXML(filterText));
            var filteredIds = getFilterIds(preparedWithSQL, "taskid", "task", whereClause, new object[] { param });
            applyFilter(filteredIds);
            return this;
        }

        public TaskFilter ApplyFilterByTaskName(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return this;
            var taskTypeFilter = new TaskTypeFilter(null);
            string whereClause = string.Format("Where taskname like '%{0}%'", filterText);
            var filteredIds = getFilterIds(null, "taskid", "task", whereClause, new object[] { });
            applyFilter(filteredIds);
            return this;
        }
    }
}
