using System;

namespace CRMWebApi.DTOs.Adsl
{
    public class DTOcustomer :BaseAttachedObject
    {
        public int customerid { get; set; }
        public string customername { get; set; }
        public string tc { get; set; }
        public string gsm { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string superonlineCustNo { get; set; }
        public Nullable<int> ilKimlikNo { get; set; }
        public Nullable<int> ilceKimlikNo { get; set; }
        public Nullable<int> bucakKimlikNo { get; set; }
        public Nullable<int> mahalleKimlikNo { get; set; }
        public Nullable<int> yolKimlikNo { get; set; }
        public Nullable<int> binaKimlikNo { get; set; }
        public Nullable<int> daire { get; set; }
        public int updatedby { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public string description { get; set; }
        public Nullable<bool> deleted { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> appointmentdate { get; set; }
        public Nullable<System.DateTime> birthdate { get; set; }

        public DTOil il { get; set; }
        public DTOilce ilce { get; set; }
        public DTOmahalle mahalle { get; set; } // yasin bey istedi mahalle ismi görünsün 26.10.2016 Hüseyin KOZ
    }
}