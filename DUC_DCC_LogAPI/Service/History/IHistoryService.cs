using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.History;

namespace DUC_DCC_LogAPI.Service.History
{
    public interface IHistoryService
    {
        public Task<ResponseDto> GetHistoryList(HistoryDto request);
    }
}
