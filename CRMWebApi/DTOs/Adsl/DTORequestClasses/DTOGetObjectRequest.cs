﻿namespace CRMWebApi.DTOs.Adsl.DTORequestClasses
{
    public class DTOGetObjectRequest
    {
        public DTOFieldFilter objecttype {get;set;}
        public DTOFieldFilter _object {get;set;}


        public DTOFilter getFilter()
        {

            if (objecttype.value.ToString()== "16777217")
            {
                var filter = new DTOFilter("site","siteid");
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "category", op = 2, value = objecttype.value });
                return filter;
            }
            else if (objecttype.value.ToString() == "16777218")
            {
                var filter = new DTOFilter("block", "blockid");
                filter.fieldFilters.Add(objecttype);
                return filter;
            }
            else if (objecttype.value.ToString() == "16777220")
            {
                var filter = new DTOFilter("customer", "customerid");
                filter.fieldFilters.Add(objecttype);
                return filter;
            }
            else
            {
                var filter = new DTOFilter("personel", "personelid");
                filter.fieldFilters.Add(new DTOFieldFilter { fieldName = "category", op = 2, value = objecttype.value });
                return filter;
            }
        }
    }
}
