using CRMWebApi.DTOs.Cari;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class CariHareketReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<CariHareketReport>> get()
        {
            var report = (await AdslReportsController.getCariHareketler());
            return report.AsQueryable();
        }
    }
}
