using CRMWebApi.DTOs.Adsl;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class SKStandbyTasksHoursReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<SKStandbyTasksHours>> get()
        {
            var report = (await AdslReportsController.getSKStandbyTasksHours());
            return report.AsQueryable();
        }
    }
}
