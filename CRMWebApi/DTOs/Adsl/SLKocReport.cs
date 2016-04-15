using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class SLKocReport : SLBayiReport
    {
        public DateTime? KocSLStart { get; set; }

        public DateTime? kocSLEnd = DateTime.Now;
        public DateTime? KocSLEnd
        {
            get { return KocSLStart.HasValue ? kocSLEnd : null; }
            set { if (value != null) kocSLEnd = value; }
        }
        public TimeSpan? KocSLTime { get { return (KocSLStart == null) ? null : (TimeSpan?)(KocSLEnd.Value - KocSLStart.Value); } } //SLEnd - SLStart
        public string KocSLTimeString
        {
            get
            {
                if (KocSLTime == null) return null;
                var formatter = new List<string>();
                if (KocSLTime.Value.Days > 0) formatter.Add("d' gün");
                if (KocSLTime.Value.Hours > 0) formatter.Add("hh' saat");
                if (KocSLTime.Value.Minutes > 0) formatter.Add("mm' dakika");
                try
                {
                    return formatter.Count == 0 ? KocSLTime.Value.ToString(@"' Waaaaws! 'ss' saniye'") :
                (KocSLTime.Value.ToString($@"{string.Join(" '", formatter) + " '"}"));
                }
                catch (Exception)
                {

                    throw;
                }
            }
            set { }
        }
        public int KocSLMaxTime { get; set; } //bitirilmesi gereken azami süre
    }
}