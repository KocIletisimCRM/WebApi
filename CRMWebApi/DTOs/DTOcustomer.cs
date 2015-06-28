using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class DTOcustomer :BaseAttachedObject
    {

        public int customerid { get; set; }
        public DTOblock block { get; set; }
        public string tckimlikno { get; set; }
        public string customername { get; set; }
        public string customersurname { get; set; }
        public string flat { get; set; }
        public string gsm { get; set; }
        public string phone { get; set; }
        public Nullable<System.DateTime> birthdate { get; set; }
        public Nullable<System.DateTime> creationdate { get; set; }
        public Nullable<System.DateTime> lastupdated { get; set; }
        public DTOpersonel updatedpersonel { get; set; }
        public Nullable<bool> deleted { get; set; }
        public DTOcustomer_status customer_status { get; set; }
        public Nullable<int> telstatu { get; set; }
        public string turkcellTv { get; set; }
        public Nullable<int> netstatu { get; set; }
        public string description { get; set; }
        public Nullable<int> iss { get; set; }
        public Nullable<int> emptorcustomernum { get; set; }
        public Nullable<int> tvstatu { get; set; }
        public Nullable<int> gsmstatu { get; set; }
    }
}