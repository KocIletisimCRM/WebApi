//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRMWebApi.Models.Cari
{
    using System;
    using System.Collections.Generic;
    
    public partial class Hareketler
    {
        public int Id { get; set; }
        public int BorcluFirma { get; set; }
        public int AlacakliFirma { get; set; }
        public int GelirGiderTuru { get; set; }
        public System.DateTime IslemTarihi { get; set; }
        public System.DateTime Donem { get; set; }
        public System.DateTime VadeTarihi { get; set; }
        public decimal Tutar { get; set; }
        public decimal KdvMatrah { get; set; }
        public string Aciklama { get; set; }
        public Nullable<int> Muavin { get; set; }
        public Nullable<int> Kaynak { get; set; }
        public bool Odendi { get; set; }
    
        public virtual GelirGiderTurleri GelirGiderTurleri { get; set; }
        public virtual Firmalar Firmalar { get; set; }
        public virtual Firmalar Firmalar1 { get; set; }
    }
}
