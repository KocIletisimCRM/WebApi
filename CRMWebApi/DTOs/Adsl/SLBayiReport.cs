using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SLBayiReport
    {
        [Key]
        public string SLName { get; set; }
        public int? BayiId { get; set; }
        public string BayiName { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        [Key]
        public int CustomerId { get; set; }
        public string superonlineNo { get; set; }
        public string CustomerName { get; set; }
        public DateTime? BayiSLTaskStart { get; set; }
        private DateTime? bayiSLEnd = DateTime.Now;
        public DateTime? BayiSLEnd
        {
            get { return BayiSLTaskStart.HasValue ? bayiSLEnd : null; }
            set { if (value != null) bayiSLEnd = value; }
        }
        public DateTime? BayiSLStart
        { // 17:00 dan sora atanan sl'ler sabah başlatılacak
            get
            {
                return BayiSLTaskStart == null ? null :
                ( // Bayiye atama pazar günü yapılmışsa ve işlem pazar tamamlanmamışsa başlangıç pazartesi olacak
                    (BayiSLTaskStart.Value.DayOfWeek == DayOfWeek.Sunday && (BayiSLEnd.Value.Date - BayiSLTaskStart.Value.Date).Days > 0) ? BayiSLTaskStart.Value.AddDays(1).AddTicks(-BayiSLTaskStart.Value.TimeOfDay.Ticks).AddHours(8) : (BayiSLTaskStart.Value.TimeOfDay.Hours >= 17 && (BayiSLEnd.Value.Date - BayiSLTaskStart.Value.Date).Days > 0) ? (BayiSLTaskStart.Value.DayOfWeek == DayOfWeek.Saturday) ?
                        BayiSLTaskStart.Value.AddDays(2).AddTicks(-BayiSLTaskStart.Value.TimeOfDay.Ticks).AddHours(8) : // bir sonraki gün 8'de başlayacak sl'ler için atama c.tesi yapılmışsa başlangıç p.tesi başlaması için yeni kural eklendi (Hüseyin KOZ)
                        BayiSLTaskStart.Value.AddDays(1).AddTicks(-BayiSLTaskStart.Value.TimeOfDay.Ticks).AddHours(8) :
                        BayiSLTaskStart
                );
            }
            set { }
        }
        public TimeSpan? BayiSLTime
        {
            get
            {
                return BayiSLTaskStart == null ? null : (TimeSpan?)(BayiSLEnd.Value - BayiSLStart.Value);
            }
        } // SLEnd - SLStart ()
        public int BayiSLMaxTime { get; set; } //tamamlanması gereken azami süre
        public double? BayiSLEtkisi { get; set; } // Yasin Bey; bayi varolan bilgilerin yanında sadece saat olarak işlem süresini görmesini istediği için oluşturuldu
        public string BayiSLTimeSting
        {
            get
            {
                if (BayiSLTaskStart == null) return null;
                var formatter = new List<string>();
                if (BayiSLTime.Value.Days > 0) formatter.Add("d' gün");
                if (BayiSLTime.Value.Hours > 0) formatter.Add("hh' saat");
                if (BayiSLTime.Value.Minutes > 0) formatter.Add("mm' dakika");
                try
                {
                    return formatter.Count == 0 ? BayiSLTime.Value.ToString(@"' Waaaaws! 'ss' saniye'") :
                (BayiSLTime.Value.ToString($@"{string.Join(" '", formatter) + " '"}"));
                }
                catch (Exception)
                {

                    throw;
                }
            }
            set { }
        }
        public string slstatus { get; set; } // anlık sl durumu
        public string status { get; set; } // process durumu

        public static string GetHeadhers()
        {
            return (new SLBayiReport()).GetType().GetProperties().Select(p => p.Name).Aggregate((p, c) => $"{p};{c}");
        }

        public override string ToString()
        {
            return this.GetType().GetProperties().Select(p => (p.GetValue(this) ?? "").ToString().Replace("\r\n", " ")).Aggregate((p, c) => $"{p};{c}");
        }
    }
}