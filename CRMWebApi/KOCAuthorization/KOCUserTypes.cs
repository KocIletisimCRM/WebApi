using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.KOCAuthorization
{
    public enum KOCUserTypes
    {
        CallCenterStuff = 1,
        StockRoomStuff = 1 << 1,
        BackOfficeStuff = 1 << 2,
        TechnicalStuff = 1 << 3,
        Reserved1 = 1 << 4,
        SalesStuff = 1 << 5,
        ProtocolStuff = 1 << 6,
        updateStaff = 1 << 7,
        AgentStuff = 1 << 8, // Uzaktan satış yapan bayiler için (06.02.2017) hüseyin koz
        Customer = 1 << 24,
        ADSLCustomer = Customer | 1,
        Assosiation = 1 << 25,
        ADSLProcurementAssosiation = Assosiation | 1,
        ADSLStockRoomAssosiation = Assosiation | 2,
        TeamLeader = 1 << 26,
        BackOfficeTeamLeader = TeamLeader | BackOfficeStuff,
        TechnicalTeamLeader = TeamLeader | TechnicalStuff,
        SalesTeamLeader = TeamLeader | SalesStuff,
        Manager = 1 << 30,
        HRManager = Manager,
        SalesSupportManager = Manager | TeamLeader | CallCenterStuff | BackOfficeStuff,
        SalesManager = Manager | TeamLeader | SalesStuff,
        TechnicalManager = Manager | TeamLeader | TechnicalStuff | Reserved1 | ProtocolStuff | StockRoomStuff,
        Admin = 0xff
    }

    public enum FiberKocUserTypes
    {
        CallCenterStuff = 1,
        StockRoomStuff = 1 << 1,
        BackOfficeStuff = 1 << 2,
        TechnicalStuff = 1 << 3,
        Reserved1 = 1 << 4,
        SalesStuff = 1 << 5,
        ProtocolStuff = 1 << 6,
        Customer = 1 << 24,
        Site = Customer | 1,
        Block = Customer | 2,
        HP= Customer | 4,
        SiteManager = Customer | 8,
        Assosiation = 1 << 25,
        ADSLProcurementAssosiation = Assosiation | 1,
        ADSLStockRoomAssosiation = Assosiation | 2,
        TeamLeader = 1 << 26,
        BackOfficeTeamLeader = TeamLeader | BackOfficeStuff,
        TechnicalTeamLeader = TeamLeader | TechnicalStuff,
        SalesTeamLeader = TeamLeader | SalesStuff,
        Manager = 1 << 30,
        HRManager = Manager,
        SalesSupportManager = Manager | TeamLeader | CallCenterStuff | BackOfficeStuff,
        SalesManager = Manager | TeamLeader | SalesStuff,
        TechnicalManager = Manager | TeamLeader | TechnicalStuff | Reserved1 | ProtocolStuff | StockRoomStuff,
        Admin = 0xff
    }
}