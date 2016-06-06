using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class ISSSuccessRate
    {
        [Key]
        public int staskoerderno { get; set; }
        public string stask { get; set; }
        public int customerid { get; set; }
        public string customername { get; set; }
        public int screatedateyear { get; set; }
        public int screatedatemonth { get; set; }
        public int screatedateday { get; set; }
        public int? snetflowdateyear { get; set; }
        public int? snetflowdatemonth { get; set; }
        public int? snetflowdateday { get; set; }
        public string staskstatus { get; set; }
        public string staskstatetype { get; set; }
        public int? sconsummationdateyear { get; set; }
        public int? sconsummationdatemonth { get; set; }
        public int? sconsummationdateday { get; set; }
        public int? endtaskorderno { get; set; }
        public string endtask { get; set; }
        public int? endtaskcreatedateyear { get; set; }
        public int? endtaskcreatedatemonth { get; set; }
        public int? endtaskcreatedateday { get; set; }
        public int? endconsummationdateyear { get; set; }
        public int? endconsummationdatemonth { get; set; }
        public int? endconsummationdateday { get; set; }
        public string endtaskstatus { get; set; }
        public string endtaskstatetype { get; set; }
    }
}