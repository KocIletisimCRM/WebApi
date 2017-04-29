using System;
using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKRate
    {
        public int year
        {
            get
            {
                return date.Year;
            }
            set { }
        }
        public int month
        {
            get
            {
                return date.Month;
            }
            set { }
        }
        public int day
        {
            get
            {
                return date.Day;
            }
            set { }
        }
        public double k_SuccesRate { get { return k_total > 0 ? Math.Round(100 * ((double)k_completed / (double)k_total), 2) : 0; } set { } }
        public double k_SLOrt { get; set; }
        public int k_completed { get; set; }
        public int k_total { get; set; }
        public int e_completed { get; set; }
        public int e_total { get; set; }
        public double e_SuccesRate { get { return e_total > 0 ? Math.Round(100 * ((double)e_completed / (double)e_total), 2) : 0; } set { } }
        public double e_SLOrt { get; set; }
        public int d_completed { get; set; }
        public int d_total { get; set; }
        public double d_SuccesRate { get { return d_total > 0 ? Math.Round(100 * ((double)d_completed / (double)d_total), 2) : 0; } set { } }
        public double d_SLOrt { get; set; }
        public int a_completed { get; set; }
        public int a_total { get; set; }
        public double a_SuccesRate { get { return a_total > 0 ? Math.Round(100 * ((double)a_completed / (double)a_total), 2) : 0; } set { } }
        public double a_SLOrt { get; set; }
        [Key]
        public DateTime date { get; set; }
    }
}