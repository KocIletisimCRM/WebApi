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
        public string acikAdres { get; set; }
        public Nullable<int> adresTip { get; set; }
        public string bagimsizBolumDurum { get; set; }
        public Nullable<int> bagimsizBolumDurumKod { get; set; }
        public string bagimsizBolumIcKapiNo { get; set; }
        public Nullable<int> bagimsizBolumKimlikNo { get; set; }
        public string bagimsizBolumTipiAciklama { get; set; }
        public Nullable<int> bagimsizBolumTipiKod { get; set; }
        public string binaDisKapiNo { get; set; }
        public Nullable<int> binaKimlikNo { get; set; }
        public string blokAd { get; set; }
        public string bucakAciklama { get; set; }
        public string bucakAd { get; set; }
        public Nullable<int> bucakKimlikNo { get; set; }
        public string csbmAciklama { get; set; }
        public string csbmAd { get; set; }
        public Nullable<int> csbmKimlikNo { get; set; }
        public string ilAd { get; set; }
        public Nullable<int> ilKimlikNo { get; set; }
        public string ilceAd { get; set; }
        public Nullable<int> ilceKimlikNo { get; set; }
        public string koyAciklama { get; set; }
        public string koyAd { get; set; }
        public Nullable<int> koyKimlikNo { get; set; }
        public string mahalleAciklama { get; set; }
        public string mahalleAd { get; set; }
        public Nullable<int> mahalleKimlikNo { get; set; }
        public Nullable<bool> maksIntegrated { get; set; }
        public string postaKod { get; set; }
        public string siteAd { get; set; }
        public string yapiKullanimAmacAciklama { get; set; }
        public Nullable<int> yapiKullanimAmacKod { get; set; }
        public Nullable<int> yolKimlikNo { get; set; }
        public int updatedby { get; set; }
        public DateTime lastupdated { get; set; }
        public string description { get; set; }

        public DTOil il { get; set; }
        public DTOilce ilce { get; set; }
        //public int customerid { get; set; }
        //public DTOblock block { get; set; }
        //public string tckimlikno { get; set; }
        //public string customername { get; set; }
        //public string customersurname { get; set; }
        //public string flat { get; set; }
        //public string gsm { get; set; }
        //public string phone { get; set; }
        //public Nullable<System.DateTime> birthdate { get; set; }
        //public Nullable<System.DateTime> creationdate { get; set; }
        //public Nullable<System.DateTime> lastupdated { get; set; }
        //public DTOpersonel updatedpersonel { get; set; }
        //public Nullable<bool> deleted { get; set; }
        //public DTOcustomer_status customer_status { get; set; }
        //public DTOtelStatus telStatus { get; set; }
        //public string turkcellTv { get; set; }
        //public DTOnetStatus netStatus { get; set; }
        //public string description { get; set; }
        //public DTOissStatus issStatus { get; set; }
        //public Nullable<int> emptorcustomernum { get; set; }
        //public DTOtvStatus TvKullanımıStatus { get; set; }
        //public DTOgsmStatus gsmKullanımıStatus { get; set; }

    }
}