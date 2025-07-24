using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Authen;

namespace DUC_DCC_LogAPI.Service.AuthenService
{
    public interface IAuthenService
    {
        Task<ResponseAuthen> Authen(AuthenDto authen);
        Task<ResponseAuthen> AuthenticateUserAsync(AuthenDto authen);
    }
}
