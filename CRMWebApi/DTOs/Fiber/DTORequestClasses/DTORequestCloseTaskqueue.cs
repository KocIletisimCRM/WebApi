namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public  class DTORequestCloseTaskqueue
    {
        public int campaignid { get; set; }
        public int [] selectedProductsIds { get; set; }
        public int taskorderno { get; set; }
    }
}
