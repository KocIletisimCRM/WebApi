namespace CRMWebApi.DTOs.Adsl
{
    public class SKClosedTasksReport : SKStandbyTaskReport
    {
        public int? consummationdateyear { get; set; }
        public int? consummationdatemonth { get; set; }
        public int? consummationdateday { get; set; }
        public string taskstatus { get; set; }
    }
}