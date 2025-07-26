using DUC_DCC_LogAPI.Models.Dto;

namespace DUC_DCC_LogAPI.Service.DccService
{
    public interface IScheduleService
    {
        Task <ResponseDto> ImportDccAsync();
        Task<ResponseDto> ImportDucAsync();
    }
}
