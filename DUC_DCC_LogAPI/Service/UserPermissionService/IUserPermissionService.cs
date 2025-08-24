using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto;

namespace DUC_DCC_LogAPI.Service.UserPermissionService
{
    public interface IUserPermissionService
    {
        Task<ResponseDto> GetUserById(string emp_no);
        Task<ResponseDto> GetUserList(Users_Permission request);
        Task<ResponseDto> CreateUserPermiss(Users_Permission request);
        Task<ResponseDto> UpdateUserPermiss(Users_Permission request,int Id);
        Task<ResponseDto> DeleteUserPermiss(int Id);
        Task<ResponseDto> GetAppName();
        Task<ResponseDto> GetBuPlant();
    }
}
