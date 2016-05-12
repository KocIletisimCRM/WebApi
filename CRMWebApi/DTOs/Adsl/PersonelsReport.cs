using System.ComponentModel.DataAnnotations;

namespace CRMWebApi.DTOs.Adsl
{
    public class PersonelsReport
    {
        [Key]
        public int personelid { get; set; }
        public string personelname { get; set; }
        public string email { get; set; }
        public string gorev { get; set; }
        public string kanalyoneticisi { get; set; }
        public string il { get; set; }
        public string ilce { get; set; }
    }
}