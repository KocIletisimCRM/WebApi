using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CRMWebApi.DTOs
{
    public class attachedObjectTypeConverter: TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string) 
            {
                var obj = JObject.Parse((string)value);
                var jsonDic = (IDictionary<string, JToken>)obj;
                if (jsonDic.ContainsKey("customerid")) return JsonConvert.DeserializeObject<DTOcustomer>((string)value);
                else if (jsonDic.ContainsKey("blockid")) return obj.ToObject<DTOblock>();
                else if (jsonDic.ContainsKey("siteid")) return obj.ToObject<DTOsite>();
                else return null;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}