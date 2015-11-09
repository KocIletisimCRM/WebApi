using System;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTORequestSaveFaultTaks
    {
      public int? customerid {get;set;}
      public int? attachedpersonelid {get;set;}
      public DateTime appointmentdate {get;set;}
      public string description {get;set;}
      public string fault { get; set; }
    }
}