using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Chart;

namespace DUC_DCC_LogAPI.Service.Chart
{
    public interface IChart
    {
        Task<ResponseDto> GetChartDataAsync(int Year, string plant);
        Task<ResponseDto> GetChartBarAsync(int Year, string plant);
    }
}
