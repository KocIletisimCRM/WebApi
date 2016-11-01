using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class EvrakBasari
    {
        [Key]
        public int s_ton { get; set; }
        public string s_tname { get; set; }
        public string s_personel { get; set; }
        public string kaynak { get; set; }
        public int s_create_day { get; set; }
        public int s_create_month { get; set; }
        public int s_create_year { get; set; }
        public int? s_netflow_day { get; set; }
        public int? s_netflow_month { get; set; }
        public int? s_netflow_year { get; set; }
        public int? s_close_day { get; set; }
        public int? s_close_month { get; set; }
        public int? s_close_year { get; set; }
        public string s_status { get; set; }
        public string s_statustype { get; set; }
        public string custname { get; set; }
        public int custid { get; set; }
        public string custsolid { get; set; }
        public string il { get; set; }
        public string ilce { get; set; }
        public int? kapamatask_ton { get; set; }
        public string kapamatask_name { get; set; }
        public string kapamatask_personel { get; set; }
        public int? kapamatask_create_day { get; set; }
        public int? kapamatask_create_month { get; set; }
        public int? kapamatask_create_year { get; set; }
        public int? kapamatask_close_day { get; set; }
        public int? kapamatask_close_month { get; set; }
        public int? kapamatask_close_year { get; set; }
        public string kapamatask_status { get; set; }
        public string kapamatask_statustype { get; set; }
        public string process_status { get; set; }
        public string kapamatask_desc { get; set; }
        public string stask_desc { get; set; }
    }
}