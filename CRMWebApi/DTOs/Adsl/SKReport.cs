using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKReport
    {
        [Key]
        public int? MusteriId { get; set; }  // customerid
        public string SuperOnlineNo { get; set; } // süperonline müşteri numarası
        public string Musteri { get; set; }
        public string Telefon { get; set; }
        public string gsm { get; set; } // customer gsm
        public string il { get; set; } // il for customer
        public string ilce { get; set; }
        public string MusteriAdres { get; set; }
        public int? IlkTaskPerId { get; set; } // satış personelid
        public string IlkTaskPersonel { get; set; }
        public string IlkTaskPerKanalYon { get; set; } // satış personeli kanal yöneticisi
        public int IlkTaskNo { get; set; } // satış taskoerder no
        public string IlkTask { get; set; } // satış task name
        public string Kampanya { get; set; } // customer campaign
        public string kaynak { get; set; }
        public string IlkTaskAciklama { get; set; } // satış description
        public int IlkTaskOlusmaYil { get; set; } // satışın girildiği yıl
        public int IlkTaskOlusmaAy { get; set; } // satışın girildiği ay
        public int IlkTaskOlusmaGun { get; set; } // satışın girildiği gün
        public int IlkTaskNetflowYil { get; set; } // satışın netflow yıl
        public int IlkTaskNetflowAy { get; set; } // satışın netflow ay
        public int IlkTaskNetflowGun { get; set; } // satışın netflow gün
        public string IlkTaskNetflowSaat { get; set; } // satışın netflow saat
        public DateTime? IlkTaskKapama { get; set; }
        public int? IlkTaskKapamaYil { get; set; }
        public int? IlkTaskKapamaAy { get; set; }
        public int? IlkTaskKapamaGun { get; set; }
        public string IlkTaskDurum { get; set; }
        public string IlkTaskDurumTuru { get; set; }
        public int? KrandTaskNo { get; set; }
        public string KrandTask { get; set; }
        public int? KrandPerId { get; set; }
        public string KRandPersonel { get; set; }
        public int? KRandOlusmaYil { get; set; }
        public int? KRandOlusmaAy { get; set; }
        public int? KRandOlusmaGun { get; set; }
        public int? KRandNetflowYil { get; set; }
        public int? KRandNetflowAy { get; set; }
        public int? KRandNetflowGun { get; set; }
        public string KRandNetflowSaat { get; set; }
        public DateTime? KRandKapama { get; set; }
        public int? KRandKapamaYil { get; set; }
        public int? KRandKapamaAy { get; set; }
        public int? KRandKapamaGun { get; set; }
        public string KrandDurum { get; set; }
        public string KRandDurumTuru { get; set; }
        public string KRandAciklama { get; set; }
        public int? KurulumTaskNo { get; set; }
        public string KurulumTask { get; set; }
        public int? KurulumPerId { get; set; }
        public string KurulumPersonel { get; set; }
        public string KPerKanalYon { get; set; } // kurulum personeli kanal yöneticisi
        public DateTime? KurulumKapama { get; set; }
        public int? KurulumKapamaYil { get; set; }
        public int? KurulumKapamaAy { get; set; }
        public int? KurulumKapamaGun { get; set; }
        public string KurulumDurum { get; set; }
        public string KurulumDurumTuru { get; set; }
        public string KurulumAciklama { get; set; }
        public int? KtkTaskNo { get; set; }
        public string KtkTask { get; set; }
        public int? KtkPerId { get; set; }
        public string KtkPersonel { get; set; }
        public DateTime? KtkKapama { get; set; }
        public int? KtkKapamaYil { get; set; }
        public int? KtkKapamaAy { get; set; }
        public int? KtkKapamaGun { get; set; }
        public string KtkKapamaSaat { get; set; }
        public string KtkDurum { get; set; }
        public string KtkDurumTuru { get; set; }
        public string KtkAciklama { get; set; }
        public int SonTaskOlusmaYil { get; set; }
        public int SonTaskOlusmaAy { get; set; }
        public int SonTaskOlusmaGun { get; set; }
        public int? SonTaskKapamaYil { get; set; }
        public int? SonTaskKapamaAy { get; set; }
        public int? SonTaskKapamaGun { get; set; }
        public string SonTaskKapamaSaat { get; set; }
        public string SonDurum { get; set; }
        public string SonTaskDurum { get; set; }
        public string SonTaskTuru { get; set; }
        public string SonTask { get; set; }
        public string SonTaskAciklama { get; set; }

        public static string GetHeadhers()
        {
            return (new SKReport()).GetType().GetProperties().Select(p => p.Name).Aggregate((p, c) => $"{p};{c}");
        }

        public override string ToString()
        {
            return this.GetType().GetProperties().Select(p => (p.GetValue(this) ?? "").ToString().Replace("\r\n", " ")).Aggregate((p, c) => $"{p};{c}");
        }
    }

}