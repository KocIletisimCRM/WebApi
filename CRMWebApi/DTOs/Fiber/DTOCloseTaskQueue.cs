namespace CRMWebApi.DTOs.Fiber
{
    public class DTOCloseTaskQueue :DTOtaskqueue
    {
      
            public int tqid { get; set; }
            public int campaignid { get; set; }
            public int[] selectedProducts { get; set; }
  
    }
}