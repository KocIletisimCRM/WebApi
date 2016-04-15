using CRMWebApi.DTOs.Adsl;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class SKStandbyTaskReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<SKStandbyTaskReport>> get()
        {
            var report = (await AdslReportsController.getSKStandbyTaskReport());
            return report.AsQueryable();
        }
    }
}
