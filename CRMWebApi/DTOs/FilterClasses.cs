using CRMWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class BaseFilterList
    {
        protected List<int> ids = new List<int>();

        public BaseFilterList() { }

        public BaseFilterList(IEnumerable<int> idArray)
        {
            ApplyFilter(idArray);
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

        public BaseFilterList ApplyFilter(IEnumerable<int> idArray)
        {
            if (idArray == null || idArray.Count() == 0) return this;
            setIds((ids.Count > 0) ? ids.Intersect(idArray) : idArray);
            return this;
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
            using (var db = new CRMEntities())
            {
                return base.ApplyFilter(db.tasktypes.Where(tt => tt.TaskTypeName.Contains(filterText)).Select(tt => tt.TaskTypeId).ToList()).getFilterXML();
            }
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
            using (var db = new CRMEntities())
            {
                base.ApplyFilter(db.sf_task(0, 0, taskTypeFilter.getFilterXML()).Select(t => t.taskid).ToList());
                return this;
            }
        }

        public TaskFilter ApplyFilterByTaskTypeName(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return this;
            var taskTypeFilter = new TaskTypeFilter(null);
            using (var db = new CRMEntities())
            {
                base.ApplyFilter(db.sf_task(0, 0, taskTypeFilter.getFilterXML(filterText)).Select(t => t.taskid).ToList());
                return this;
            }
        }

        public TaskFilter ApplyFilterByTaskName(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return this;
            var taskTypeFilter = new TaskTypeFilter(null);
            using (var db = new CRMEntities())
            {
                base.ApplyFilter(db.task.Where(t => t.taskname.Contains(filterText)).Select(t => t.taskid).ToList());
                return this;
            }
        }

    }
}
