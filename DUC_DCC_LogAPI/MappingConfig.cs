using AutoMapper;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Models.Dto.SaveDuc_DccLog;
using DUC_DCC_LogAPI.Models.Dto.User;

namespace DUC_DCC_LogAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
                {
                    config.CreateMap<Application_log, Application_logDto>();
                    config.CreateMap<Application_log, SaveDUC_DCC_logDto>();
                    config.CreateMap<Users_Permission,UserResposeDto>();

                });
            return mappingConfig;
        }
    }
}
 