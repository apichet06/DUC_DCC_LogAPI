using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dtos;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;

namespace DUC_DCC_LogAPI.Service.Duc_DccLog
{
    public interface IDUC_DCC_Log
    {
        Task<ResponseDto> GetList(SearchDto request);
        Task<ResponseDto> GetSaveLogList(SearchDto request);
        Task<ResponseDto> UpdateList(CheckedDataDto request); 
        Task<ResponseDto> SendMail(); 
        Task<ResponseDto<FileDownloadDto>> ExportExcel(SearchDto request); 
        Task<byte[]> ExportExcelLog(SearchDto request);
        Task<byte[]> ExportExcelDccLog(SearchDto request);
        Task<byte[]> ExportExcelAccept(SearchDto request);
        Task<ResponseDto> GetDataDUC(Application_log request);
        Task<ResponseDto> GetDataDCC();
    }
}
