using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.KOCAuthorization
{
    public class KOCAuthorizedUser
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public string userFullName { get; set; }
        public int userRole { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime lastActivityTime { get; set; }

        public bool hasRole(KOCUserTypes role)
        {
            return (userRole & (int)role) == (int)role;
        }
    }
}
