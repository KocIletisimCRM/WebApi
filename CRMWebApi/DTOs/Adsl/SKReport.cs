﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKReport
    {
        [Key]
        public int s_ton { get; set; } // satış taskoerder no
        public int? custid { get; set; }  // customerid
        public string custname { get; set; }
        public string custphone { get; set; }
        public string il { get; set; } // il for customer
        public string ilce { get; set; }
        public string gsm { get; set; } // customer gsm
        public int? s_perid { get; set; } // satış personelid
        public string s_pername { get; set; }
        public string s_perky { get; set; } // satış personeli kanal yöneticisi
        public string s_tqname { get; set; } // satış task name
        public string campaign { get; set; } // customer campaign
        public string kaynak { get; set; }
        public string s_desc { get; set; } // satış description
        public int s_year { get; set; } // satışın yapıldığı yıl
        public int s_month { get; set; } // satışın yapıldığı ay
        public int s_day { get; set; } // satışın yapıldığı gün
        public DateTime? s_consummationdate { get; set; }
        public int? s_consummationdateyear { get; set; }
        public int? s_consummationdatemonth { get; set; }
        public int? s_consummationdateday { get; set; }
        public string s_tqstate { get; set; }
        public string s_tqstatetype { get; set; }
        public int? kr_ton { get; set; }
        public string kr_tqname { get; set; }
        public int? kr_perid { get; set; }
        public string kr_pername { get; set; }
        public DateTime? kr_consummationdate { get; set; }
        public int? kr_consummationdateyear { get; set; }
        public int? kr_consummationdatemonth { get; set; }
        public int? kr_consummationdateday { get; set; }
        public string kr_tqstate { get; set; }
        public string kr_tqstatetype { get; set; }
        public string kr_desc { get; set; }
        public int? k_ton { get; set; }
        public string k_tqname { get; set; }
        public int? k_perid { get; set; }
        public string k_pername { get; set; }
        public DateTime? k_consummationdate { get; set; }
        public int? k_consummationdateyear { get; set; }
        public int? k_consummationdatemonth { get; set; }
        public int? k_consummationdateday { get; set; }
        public string k_tqstate { get; set; }
        public string k_tqstatetype { get; set; }
        public string k_desc { get; set; }
        public int? ktk_ton { get; set; }
        public string ktk_tqname { get; set; }
        public int? ktk_perid { get; set; }
        public string ktk_pername { get; set; }
        public DateTime? ktk_consummationdate { get; set; }
        public int? ktk_consummationdateyear { get; set; }
        public int? ktk_consummationdatemonth { get; set; }
        public int? ktk_consummationdateday { get; set; }
        public string ktk_tqstate { get; set; }
        public string ktk_tqstatetype { get; set; }
        public string ktk_desc { get; set; }
        public int lasttaskcretiondateyear { get; set; }
        public int lasttaskcretiondatemonth { get; set; }
        public int lasttaskcretiondateday { get; set; }
        public string lastTaskStatus { get; set; }
        public string lastTaskTypeName { get; set; }
        public string lastTaskName { get; set; }

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