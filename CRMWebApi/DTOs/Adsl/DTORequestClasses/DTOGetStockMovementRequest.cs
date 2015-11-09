using System.Globalization;

namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public  class DTOGetStockMovementRequest:DTORequestPagination
    {
        public DTOFieldFilter fromobject { get; set; }
        public DTOFieldFilter toobject { get; set; }
        public DTOFieldFilter product { get; set; }
        public DTOFieldFilter movementdate { get; set; }
        public DTOFieldFilter serialno { get; set; }
        public DTOFieldFilter movement { get; set; }

        public DTOFilter getFilter()
        {
            var specialFilters = new string[] { "SATINALMA", "DEPO" };
            var filter = new DTOFilter("stockmovement", "movementid");

            if (product != null)
            {
                var subFilter = new DTOFilter("stockcard", "stockid");
                subFilter.fieldFilters.Add(product);
                filter.subTables.Add("stockcardid", subFilter);
            }
            if (movementdate != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "movementdate", op = 5, value = movementdate.value });
            if (serialno != null) filter.fieldFilters.Add(new DTOFieldFilter {fieldName="serialno",op=2,value=serialno.value });
            if (movement != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "movementid", op = 2, value = movement.value });
            if (fromobject != null && !string.IsNullOrWhiteSpace((string)fromobject.value))
            {
                var seachKey = (fromobject.value??string.Empty).ToString().ToUpper(CultureInfo.CurrentCulture);
                //Satınalma
                if (specialFilters[0].Contains(seachKey)) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "fromobjecttype", op = 2, value = 4000 });
                // Depo
                else if(specialFilters[1].Contains(seachKey)) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "fromobjecttype", op = 2, value = 5000 });
                // Diğer
                var subFilterPersonel = new DTOFilter("personel", "personelid");
                subFilterPersonel.fieldFilters.Add(new DTOFieldFilter { fieldName = "personelname", op = 6, value = fromobject.value });
                filter.subTables.Add("fromobject", subFilterPersonel);

                var subFilterCustomer = new DTOFilter("customer","customerid");
                // Ad soyad kontrolü yapılacak
                subFilterCustomer.fieldFilters.Add(new DTOFieldFilter { fieldName = "customername", op = 6, value = fromobject.value });
                filter.subTables.Add("fromobject1", subFilterCustomer);

                var subFilterBlock = new DTOFilter("block","blockid");
                subFilterBlock.fieldFilters.Add(new DTOFieldFilter { fieldName = "blockname", op = 6, value = fromobject.value });
                filter.subTables.Add("fromobject2",subFilterBlock);

                var subFilterSite = new DTOFilter("site","siteid");
                subFilterSite.fieldFilters.Add(new DTOFieldFilter { fieldName = "sitename", op = 6, value = fromobject.value });
                filter.subTables.Add("fromobject3",subFilterSite);
            }

            if (toobject != null && !string.IsNullOrWhiteSpace((string)toobject.value))
            {
                var seachKey = (toobject.value ?? string.Empty).ToString().ToUpper(CultureInfo.CurrentCulture);
                // Depo
                if (specialFilters[1].Contains(seachKey)) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "toobjecttype", op = 2, value = 5000 });
                // Diğer
                var subFilterPersonel = new DTOFilter("personel", "personelid");
                subFilterPersonel.fieldFilters.Add(new DTOFieldFilter { fieldName = "personelname", op = 6, value = toobject.value });
                filter.subTables.Add("toobject", subFilterPersonel);

                var subFilterCustomer = new DTOFilter("customer", "customerid");
                // Ad soyad kontrolü yapılacak
                subFilterCustomer.fieldFilters.Add(new DTOFieldFilter { fieldName = "customername", op = 6, value = toobject.value });
                filter.subTables.Add("toobject1", subFilterCustomer);

                var subFilterBlock = new DTOFilter("block", "blockid");
                subFilterBlock.fieldFilters.Add(new DTOFieldFilter { fieldName = "blockname", op = 6, value = toobject.value });
                filter.subTables.Add("toobject2", subFilterBlock);

                var subFilterSite = new DTOFilter("site", "siteid");
                subFilterSite.fieldFilters.Add(new DTOFieldFilter { fieldName = "sitename", op = 6, value = toobject.value });
                filter.subTables.Add("toobject3", subFilterSite);
            }


            return filter;
        }


    }
}
