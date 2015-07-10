using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace CRMWebApi.Models
{
    public abstract class EntityBase
    {
        public T toDTO<T>()
        {
            T dto = Activator.CreateInstance<T>();
            foreach(var p in dto.GetType().GetProperties()){
                if (this.GetType().GetProperty(p.Name)==null || this.GetType().GetProperty(p.Name).GetValue(this) == null) continue;
                if (this.GetType().GetProperty(p.Name).GetGetMethod().IsVirtual){
                    EntityBase propertyValue = this.GetType().GetProperty(p.Name).GetValue(this) as EntityBase;
                    p.SetValue(dto, propertyValue.toDTO());
                }
                else
                    p.SetValue(dto, this.GetType().GetProperty(p.Name).GetValue(this));
            }
            return dto;
        }

        public abstract EntityBase toDTO();
    }
    public partial class taskqueue:EntityBase
    {
        public override taskqueue toDTO()
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


}