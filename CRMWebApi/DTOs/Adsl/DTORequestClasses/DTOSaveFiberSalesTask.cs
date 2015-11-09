using System;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOSaveFiberSalesTask
    {
        public int ? customerid {get;set;}
        public int ? attachedpersonelid {get;set;}
        public int assistant_personelid {get;set;}
        public DateTime appointmentdate {get;set;}
    }
}
