using CRMWebApi.DTOs.Adsl;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class PersonelsReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<PersonelsReport>> get()
        {
            var report = (await AdslReportsController.getPersonelsReport());
            return report.AsQueryable();
        }
    }
}
