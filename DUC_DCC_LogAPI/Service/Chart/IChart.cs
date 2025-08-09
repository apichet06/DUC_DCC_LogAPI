using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Chart;

namespace DUC_DCC_LogAPI.Service.Chart
{
    public interface IChart
    {
        Task<ResponseDto> GetChartDataAsync();
        Task<ResponseDto> GetChartBarAsync();
    }
}
