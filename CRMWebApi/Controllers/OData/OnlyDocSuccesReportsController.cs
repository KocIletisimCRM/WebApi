using CRMWebApi.DTOs.Adsl;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class OnlyDocSuccesReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<EvrakBasari>> get()
        {
            var report = (await AdslReportsController.getEvrakBasari());
            return report.AsQueryable();
        }
    }
}