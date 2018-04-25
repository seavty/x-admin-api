using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Helper
{
    public class MappingHelper
    {
        //-> DBClass To DTO
        public static DTO MapDBClassToDTO<DBclass, DTO>(Object record)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<DBclass, DTO>());
            var mapper = config.CreateMapper();
            DTO dto = mapper.Map<DTO>(record);
            return dto;
        }

        //-> DTO To DBClass
        public static Object MapDTOToDBClass<DTO, DBclass>(Object dto, Object record)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<DTO, DBclass>());
            var mapper = config.CreateMapper();
            record = mapper.Map(dto, record);
            return record;
        }
    }
}