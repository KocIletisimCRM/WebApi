using System;
using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class MemnuniyetRapor
    {
        [Key]
        public int MusteriNo { get; set; }
        public string Musteri { get; set; }
        public string Gsm { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string Adres { get; set; }
        public string KurulumPersonel { get; set; }
        public DateTime? KurulumTarihi { get; set; }
        public int? KurulumTarihYil { get; set; }
        public int? KurulumTarihAy { get; set; }
        public int? KurulumTarihGun { get; set; }
        public int MemnuniyetTaskId { get; set; }
        public int MemnuniyetTaskNo { get; set; }
        public string MemnuniyetTask { get; set; }
        public int MemnuniyetOlusmaYil { get; set; }
        public int MemnuniyetOlusmaAy { get; set; }
        public int MemnuniyetOlusmaGun { get; set; }
        public DateTime? MemnuniyetAramaTarihi { get; set; }
        public int? MemnuniyetAramaYil { get; set; }
        public int? MemnuniyetAramaAy { get; set; }
        public int? MemnuniyetAramaGun { get; set; }
        public string MemnuniyetPersonel { get; set; }
        public string SonDurum { get; set; }
        public string MemnuniyetAciklama { get; set; }
    }
}