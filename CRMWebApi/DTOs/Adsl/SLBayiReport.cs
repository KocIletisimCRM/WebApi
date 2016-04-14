﻿using System;
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
        [Key]
        public int CustomerId { get; set; }
        public int? BayiId { get; set; }
        public string BayiName { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string CustomerName { get; set; }
        public DateTime? BayiSLTaskStart { get; set; }
        private DateTime? bayiSLEnd = DateTime.Now;
        public DateTime? BayiSLEnd
        {
            get { return BayiSLTaskStart.HasValue ? bayiSLEnd : null; }
            set { if (value != null) bayiSLEnd = value; }
        }
        public int BayiSLMaxTime { get; set; } //tamamlanması gereken azami süre
        public DateTime? BayiSLStart
        { // 17:00 dan sora atanan sl'ler sabah başlatılacak
            get
            {
                return BayiSLTaskStart == null ? null :
                (
                    (BayiSLTaskStart.Value.TimeOfDay.Hours >= 17 && (BayiSLEnd.Value.Date - BayiSLTaskStart.Value.Date).Days > 0) ?
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
        public string status { get; set; }
    }
}