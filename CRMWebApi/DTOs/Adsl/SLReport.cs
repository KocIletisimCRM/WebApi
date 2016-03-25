using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs.Adsl
{
    public class SLReport
    {
        public int? custid;  // customerid
        public string custname;
        public string custphone;
        public string il; // il for customer
        public string ilce;
        public string gsm; // customer gsm
        public int? s_perid; // satış personelid
        public string s_pername;
        public string s_perky; // satış personeli kanal yöneticisi
        public int s_ton; // satış taskoerder no
        public string s_tqname; // satış task name
        public string campaign; // customer campaign
        public string kaynak;
        public string s_desc; // satış description
        public int s_year; // satışın yapıldığı yıl
        public int s_month; // satışın yapıldığı ay
        public int s_day; // satışın yapıldığı gün
        public DateTime? s_consummationdate;
        public int? s_consummationdateyear;
        public int? s_consummationdatemonth;
        public int? s_consummationdateday;
        public string s_tqstate;
        public string s_tqstatetype;
        public int? kr_ton;
        public string kr_tqname;
        public int? kr_perid;
        public string kr_pername;
        public DateTime? kr_consummationdate;
        public int? kr_consummationdateyear;
        public int? kr_consummationdatemonth;
        public int? kr_consummationdateday;
        public string kr_tqstate;
        public string kr_tqstatetype;
        public string kr_desc;
        public int? k_ton;
        public string k_tqname;
        public int? k_perid;
        public string k_pername;
        public DateTime? k_consummationdate;
        public int? k_consummationdateyear;
        public int? k_consummationdatemonth;
        public int? k_consummationdateday;
        public string k_tqstate;
        public string k_tqstatetype;
        public string k_desc;
        public int? ktk_ton;
        public string ktk_tqname;
        public int? ktk_perid;
        public string ktk_pername;
        public DateTime? ktk_consummationdate;
        public int? ktk_consummationdateyear;
        public int? ktk_consummationdatemonth;
        public int? ktk_consummationdateday;
        public string ktk_tqstate;
        public string ktk_tqstatetype;
        public string ktk_desc;
        public string lastTaskStatus;
        public string lastTaskTypeName;
        public string lastTaskName;
        public int? sltime; // koç sl süresi
        public int? bayisltime; //bayi sl süresi
        public DateTime? kocslstartdate;
        public DateTime? kocslenddate;
        public DateTime? bayislstartdate;
        public DateTime? bayislenddate;
        public DateTime? bayislstartdateadd;
        public DateTime? bayislenddateadd;
        public int? fark; // bayislenddate - bayislstartdate
    }
}