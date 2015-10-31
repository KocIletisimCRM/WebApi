using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Reflection;
using System.Collections;

namespace CRMWebApi.Models
{
    public abstract class EntityBase
    {
        public T toDTO<T>()
        {
            T dto = Activator.CreateInstance<T>();
            Type thisType = GetType();
            Type dtoType = dto.GetType();
            foreach(var p in dto.GetType().GetProperties()){
                PropertyInfo pInfo = thisType.GetProperty(p.Name);
                object propertyValue = pInfo != null ? pInfo.GetValue(this) : null;
                if (pInfo==null || propertyValue == null) continue;
                if (
                    pInfo.GetGetMethod().IsVirtual && 
                    ((propertyValue is EntityBase) || (propertyValue.GetType().IsGenericType && propertyValue.GetType().GenericTypeArguments[0].IsSubclassOf(typeof(EntityBase))))
                ){
                    if (propertyValue.GetType().IsGenericType) p.SetValue(dto, (propertyValue as IEnumerable).Cast<EntityBase>().Select(s => s.toDTO()).ToList());
                    else p.SetValue(dto, (propertyValue as EntityBase).toDTO());
                }
                else
                    p.SetValue(dto, propertyValue);
            }
            return dto;
        }

        public abstract object toDTO();
    }

    // Çoklu tablo birleştirmesinde id ve name özelliklerini birleştirmek için
    public class idName : EntityBase
    {
        public int id;
        public string name;

        public override object toDTO()
        {
            return base.toDTO<idName>();
        }
    }

    public partial class taskqueue:EntityBase
    {
        public virtual List<stockcard> stockcardlist { get; set; }
        public override object toDTO()
        {
            var Dto= toDTO<DTOs.DTOtaskqueue>();
            if (attachedblock == null && attachedcustomer == null) return Dto;
            if (attachedblock==null)
            {
                Dto.attachedobject=attachedcustomer.toDTO<DTOs.DTOcustomer>();
              //Dto.block = attachedcustomer.block.toDTO<DTOs.DTOblock>();
              // Dto.site = attachedcustomer.block.site.toDTO<DTOs.DTOsite>();
            }
            else
            {
                Dto.attachedobject = attachedblock.toDTO<DTOs.DTOblock>();
                //Dto.site = attachedblock.site.toDTO<DTOs.DTOsite>();
            }
            return Dto;
        } 
    }

    public partial class task :EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOtask>();
        }
    }

    public partial class customer : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOcustomer>();
        }
    }

    public partial class block : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOblock>();
        }
    }
    public partial class personel : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOpersonel>();
        }
    }

    public partial class customer_status : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOcustomer_status>();
        }
    }

    public partial class site : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOsite>();
        }
    }
    public partial class taskstatepool : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOtaskstatepool>();
        }
    }

    public partial class tasktypes : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOTaskTypes>();
        }
    }
    public partial class issStatus : EntityBase 
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOissStatus>();
        }
    }

    public partial class netStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOnetStatus>();
        }
    }
    public partial class gsmKullanımıStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOgsmStatus>();
        }
    }
    public partial class telStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOtelStatus>();
        }
    }
    public partial class TvKullanımıStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOtvStatus>();
        }
    }

    public partial class product_service : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOProduct_service>();
        }
    }

    public partial class customerproduct : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOcustomerproduct>();
        }
    }

    public partial class campaigns : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOcampaigns>();
        }
    }
    public partial class stockcard : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOstockcard>();
        }
    }
    public partial class stockmovement : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOstockmovement>();
        }
    }
    public partial class stockstatus : EntityBase
    {
        public virtual List<string> serials { get; set; }
        public override object toDTO()
        {
            return toDTO<DTOs.DTOstockstatus>();
        }
    }
    public partial class TurkcellTVStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOturkcelltvstatus>();
        }
    }

    public partial class objecttypes : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOobjecttypes>();
        }
    }
    public partial class taskstatematches : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOtaskstatematches>();
        }
    }
    public partial class document : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOs.DTOdocument>();
        }
    }
}