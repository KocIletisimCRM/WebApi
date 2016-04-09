using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKPaymentReport
    {
        [Key]
        public int bId { get; set; }
        public string bName { get; set; }
        public double bSLOrt { get; set; }
        public double sat { get; set; }
        public double sat_kur { get; set; }
        public double kur { get; set; }
        public double ariza { get; set; }
        public double teslimat { get; set; }
        public double evrak { get; set; }
        public double toplam
        {
            get
            {
                return sat + sat_kur + kur + ariza + teslimat + evrak;
            }
            set { }
        }
    }
}