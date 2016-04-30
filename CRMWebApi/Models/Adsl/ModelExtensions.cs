using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using CRMWebApi.DTOs.Adsl;

namespace CRMWebApi.Models.Adsl
{
    public partial class KOCSAMADLSEntities
    {
        public KOCSAMADLSEntities(bool lazy) : this()
        {
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }
    }
    public abstract class EntityBase
    {
        public T toDTO<T>()
        {
            T dto = Activator.CreateInstance<T>();
            foreach (var p in dto.GetType().GetProperties())
            {
                PropertyInfo pInfo = GetType().GetProperty(p.Name);
                object propertyValue = pInfo != null ? pInfo.GetValue(this) : null;
                if (pInfo == null || propertyValue == null) continue;
                if (
                    pInfo.GetGetMethod().IsVirtual &&
                    ((propertyValue is EntityBase) || (propertyValue.GetType().IsGenericType && propertyValue.GetType().GenericTypeArguments[0].IsSubclassOf(typeof(EntityBase))))
                )
                {
                    if (propertyValue.GetType().IsGenericType)
                        p.SetValue(dto, (propertyValue as IEnumerable).Cast<EntityBase>().Select(s => s.toDTO()).ToList());
                    else
                        p.SetValue(dto, (propertyValue as EntityBase).toDTO());
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

    public partial class adsl_taskqueue : EntityBase
    {
        public virtual bool editable { get; set; }
        public override object toDTO()
        {
            var Dto = toDTO<DTOtaskqueue>();
            return Dto;
        }
    }

    public partial class adsl_task : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOtask>();
        }
    }

    public partial class customer : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOcustomer>();
        }
    }

    public partial class adsl_personel : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOpersonel>();
        }
    }

    public partial class adsl_customer_status : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOcustomer_status>();
        }
    }


    public partial class adsl_taskstatepool : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOtaskstatepool>();
        }
    }

    public partial class adsl_tasktypes : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOTaskTypes>();
        }
    }
    public partial class adsl_issStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOissStatus>();
        }
    }

    public partial class adsl_netStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOnetStatus>();
        }
    }
    public partial class adsl_gsmKullanımıStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOgsmStatus>();
        }
    }
    public partial class adsl_telStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOtelStatus>();
        }
    }
    public partial class adsl_TvKullanımıStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOtvStatus>();
        }
    }

    public partial class adsl_product_service : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOProduct_service>();
        }
    }

    public partial class adsl_customerproduct : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOcustomerproduct>();
        }
    }

    public partial class adsl_campaigns : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOcampaigns>();
        }
    }
    public partial class adsl_stockcard : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOstockcard>();
        }
    }
    public partial class adsl_stockmovement : EntityBase
    {
        public virtual getPersonelStockAdsl_Result stockStatus { get; set; }
        public override object toDTO()
        {
            return toDTO<DTOstockmovement>();
        }
    }
    public partial class adsl_stockstatus : EntityBase
    {
        public virtual List<string> serials { get; set; }
        public override object toDTO()
        {
            return toDTO<DTOstockstatus>();
        }
    }
    public partial class adsl_TurkcellTVStatus : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOturkcelltvstatus>();
        }
    }

    public partial class adsl_objecttypes : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOobjecttypes>();
        }
    }
    public partial class adsl_taskstatematches : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOtaskstatematches>();
        }
    }
    public partial class adsl_document : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOdocument>();
        }
    }
    public partial class il : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOil>();
        }
    }
    public partial class ilce : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOilce>();
        }
    }
    public partial class bucak : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTObucak>();
        }
    }
    public partial class mahalleKoy : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOmahalle>();
        }
    }
    public partial class adsl_customerdocument : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOcustomerdocument>();
        }
    }
    public partial class getPersonelStockAdsl_Result : EntityBase
    {
        public virtual List<string> serials { get; set; }
        public override object toDTO()
        {
            return toDTO<DTOgetPersonelStockAdsl_Result>();
        }
    }
    public partial class atama : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOatama>();
        }
    }
    public partial class SL : EntityBase
    {
        public override object toDTO()
        {
            return toDTO<DTOSL>();
        }
    }
}