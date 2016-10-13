using CRMWebApi.DTOs.Adsl;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class SKClosedTasksReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<SKClosedTasksReport>> get()
        {
            var report = (await AdslReportsController.getSKClosedTasksReport());
            return report.AsQueryable();
        }
    }
}