namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOGetAdressFilter
    {
        public DTOFieldFilter adres { get; set; }
        public DTOFieldFilter subAdres { get; set; }

        public DTOFilter getFilter()
        {
            var filter = new DTOFilter("il","kimlikNo");

            if (adres.fieldName.ToString() == "ilkimlikNo")
            {
                filter = new DTOFilter("ilce","kimlikNo");      
                filter.fieldFilters.Add(adres);
                return filter;
            }
            else if (adres.fieldName.ToString() == "ilceKimlikNo") // ilçe kimlik no seçilince mahalleler listelenmeli
            {
                filter = new DTOFilter("mahalleKoy", "kimlikno");
                filter.fieldFilters.Add(adres);
                return filter;
            }
            else if (adres.fieldName.ToString() == "mahalleKoyBaglisiKimlikNo")
            {
                if (subAdres.fieldName.ToString() == "yolKimlikNo") //mahalle ve cadde seçildiğinde binaların kapı numaraları listelenmeli
                {
                    filter = new DTOFilter("bina", "kimlikNo");
                    filter.fieldFilters.Add(adres);
                    filter.fieldFilters.Add(subAdres);
                    return filter;
                }
                else if (subAdres.fieldName.ToString()== "binaKimlikNo")//mahalle ve binakimlik numarası seçince daireler listelenmeli(bağımsız bölümler)
                {

                    filter = new DTOFilter("daire", "kimlikNo");
                    filter.fieldFilters.Add(adres);
                    filter.fieldFilters.Add(subAdres);
                    return filter;
                }
                else // sadece mahalle seçilince mahalledeki caddeler listelenmeli
                {
                    filter = new DTOFilter("cadde", "kimlikNo");
                    filter.fieldFilters.Add(adres);
                    return filter;
                }
            }
            else
            {
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName="ad",op=6,value=""});
                return filter;
            }

         
        }
    }
}
