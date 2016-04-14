using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class SKStandbyTaskReport
    {
        [Key]
        public int taskorderno { get; set; }
        public int taskid { get; set; }
        public string taskname { get; set; }
        public int creationdateyear { get; set; }
        public int creationdatemonth { get; set; }
        public int creationdateday { get; set; }
        public int customerid { get; set; }
        public string customername { get; set; }
        public int? personelid { get; set; }
        public string personelname { get; set; }
        public int? attachmentdateyear { get; set; }
        public int? attachmentdatemonth { get; set; }
        public int? attachmentdateday { get; set; }
        public string description { get; set; }
        public string fault { get; set; }
    }
}