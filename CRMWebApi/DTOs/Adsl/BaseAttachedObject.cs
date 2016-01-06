namespace CRMWebApi.DTOs.Adsl
{

    public abstract class BaseAttachedObject
    {
        public int taskid { get; set; }
        public int? salespersonel { get; set; }
        public string taskdescription { get; set; }
        public int[] productids { get; set; }
        public int campaignid { get; set; }
        public string fault { get; set; }
    }
}