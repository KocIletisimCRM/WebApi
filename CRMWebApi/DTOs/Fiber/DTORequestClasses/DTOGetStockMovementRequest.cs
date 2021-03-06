﻿using System.Globalization;

namespace CRMWebApi.DTOs.Fiber.DTORequestClasses
{
    public class DTOGetStockMovementRequest : DTORequestPagination
    {
        public DTOFieldFilter fromobject { get; set; }
        public DTOFieldFilter toobject { get; set; }
        public DTOFieldFilter product { get; set; }
        public DTOFieldFilter movementdate { get; set; }
        public DTOFieldFilter serialno { get; set; }
        public DTOFieldFilter movement { get; set; }
        public DTOFieldFilter isconfirmed { get; set; }

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
            if (serialno != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "serialno", op = 2, value = serialno.value });
            if (isconfirmed != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "confirmationdate", op = 8, value = null });
            if (movement != null) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "movementid", op = 2, value = movement.value });
            if (fromobject != null && fromobject.value != null)
            {
                var seachKey = (fromobject.value ?? string.Empty).ToString().ToUpper(CultureInfo.CurrentCulture);
                //Satınalma
                if (!string.IsNullOrWhiteSpace(seachKey))
                {
                    if (specialFilters[0].Contains(seachKey)) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "fromobjecttype", op = 2, value = (int)KOCAuthorization.KOCUserTypes.ADSLProcurementAssosiation });
                    // Depo
                    else if (specialFilters[1].Contains(seachKey)) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "fromobjecttype", op = 2, value = (int)KOCAuthorization.KOCUserTypes.ADSLStockRoomAssosiation });
                }
                // Diğer
                var subFilterPersonel = new DTOFilter("personel", "personelid");
                subFilterPersonel.fieldFilters.Add(new DTOFieldFilter { fieldName = "personelname", op = 6, value = fromobject.value });
                filter.subTables.Add("fromobject", subFilterPersonel);

                var subFilterCustomer = new DTOFilter("customer", "customerid");
                // Ad soyad kontrolü yapılacak
                subFilterCustomer.fieldFilters.Add(new DTOFieldFilter { fieldName = "customername", op = 6, value = fromobject.value });
                filter.subTables.Add("fromobject1", subFilterCustomer);

            }

            if (toobject != null && !string.IsNullOrWhiteSpace((string)toobject.value))
            {
                var seachKey = (toobject.value ?? string.Empty).ToString().ToUpper(CultureInfo.CurrentCulture);
                // Depo
                if (specialFilters[1].Contains(seachKey)) filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "toobjecttype", op = 2, value = (int)KOCAuthorization.KOCUserTypes.ADSLStockRoomAssosiation });
                // Diğer
                var subFilterPersonel = new DTOFilter("personel", "personelid");
                subFilterPersonel.fieldFilters.Add(new DTOFieldFilter { fieldName = "personelname", op = 6, value = toobject.value });
                filter.subTables.Add("toobject", subFilterPersonel);

                var subFilterCustomer = new DTOFilter("customer", "customerid");
                // Ad soyad kontrolü yapılacak
                subFilterCustomer.fieldFilters.Add(new DTOFieldFilter { fieldName = "customername", op = 6, value = toobject.value });
                filter.subTables.Add("toobject1", subFilterCustomer);

            }


            return filter;
        }


    }
}
