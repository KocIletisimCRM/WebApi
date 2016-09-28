using CRMWebApi.DTOs.Adsl;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;

namespace CRMWebApi.Controllers.OData
{
    public class StockMovementBackSeriReportsController : ODataController
    {
        [EnableQuery]
        public async Task<IQueryable<StockMovementBackSeri>> get()
        {
            var report = (await AdslReportsController.getSMBS());
            return report.AsQueryable();
        }
}
}
