using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKReport
    {
        [Key]
        public int s_ton { get; set; } // satış taskoerder no
        public int? custid { get; set; }  // customerid
        public string superonlinecustno { get; set; } // süperonline müşteri numarası
        public string custname { get; set; }
        public string custphone { get; set; }
        public string gsm { get; set; } // customer gsm
        public string il { get; set; } // il for customer
        public string ilce { get; set; }
        public string customeradres { get; set; }
        public int? s_perid { get; set; } // satış personelid
        public string s_pername { get; set; }
        public string s_perky { get; set; } // satış personeli kanal yöneticisi
        public string s_tqname { get; set; } // satış task name
        public string campaign { get; set; } // customer campaign
        public string kaynak { get; set; }
        public string s_desc { get; set; } // satış description
        public int s_createyear { get; set; } // satışın girildiği yıl
        public int s_createmonth { get; set; } // satışın girildiği ay
        public int s_createday { get; set; } // satışın girildiği gün
        public int s_netflowyear { get; set; } // satışın netflow yıl
        public int s_netflowmonth { get; set; } // satışın netflow ay
        public int s_netflowday { get; set; } // satışın netflow gün
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
        public int? kr_creationdateyear { get; set; }
        public int? kr_creationdatemonth { get; set; }
        public int? kr_creationdateday { get; set; }
        public int? kr_netflowdateyear { get; set; }
        public int? kr_netflowdatemonth { get; set; }
        public int? kr_netflowdateday { get; set; }
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
        public string k_perky { get; set; } // kurulum personeli kanal yöneticisi
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
        public int lasttaskcreationdateyear { get; set; }
        public int lasttaskcreationdatemonth { get; set; }
        public int lasttaskcreationdateday { get; set; }
        public int? lasttaskconsummationdateyear { get; set; }
        public int? lasttaskconsummationdatemonth { get; set; }
        public int? lasttaskconsummationdateday { get; set; }
        public string lastTaskStatus { get; set; }
        public string lastTaskStatusName { get; set; }
        public string lastTaskTypeName { get; set; }
        public string lastTaskName { get; set; }
        public string lastTaskDescription { get; set; }

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