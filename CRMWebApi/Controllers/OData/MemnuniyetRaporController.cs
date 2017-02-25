using CRMWebApi.DTOs.Adsl;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class MemnuniyetRaporController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<MemnuniyetRapor>> get()
        {
            var report = (await AdslReportsController.getMemnuniyetRapor());
            return report.AsQueryable();
        }
    }
}
