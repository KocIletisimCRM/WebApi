using System;

namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public class DTOSaveFiberSalesTask
    {
        public int ? customerid {get;set;}
        public int ? attachedpersonelid {get;set;}
        public int assistant_personelid {get;set;}
        public DateTime appointmentdate {get;set;}
    }
}
