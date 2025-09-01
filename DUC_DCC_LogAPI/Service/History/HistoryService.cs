using AutoMapper;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.History;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DUC_DCC_LogAPI.Service.History
{
    public class HistoryService : IHistoryService  
    {
        //private readonly ILogger _logger;
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ResponseDto _response;
        private readonly MessageDto _message;

        public HistoryService(AppDbContext dbContext,IMapper mapper)
        {
            _db = dbContext;
            _mapper = mapper;
            _response = new ResponseDto();
            _message = new MessageDto();
        }


        public async Task<ResponseDto> GetHistoryList(HistoryDto request)
        {
           
            try
            {
                var obj = await _db.history
                    .OrderByDescending(a=>a.action_datetime).ToListAsync();
                var resule = _mapper.Map<List<HistoryDto>>(obj);

                _response.Result = resule;
                _response.Message = _message.DistplaySuccess;
                

            }
            catch (Exception ex) { 
             _response.IsSuccess = false;
                if (ex.InnerException != null) {
                    _response.Message = _message.an_error_occurred + ex.InnerException!.Message;
                }
                else
                {
                    _response.Message = _message.an_error_occurred + ex.Message;
                }
           

            }
            return _response;
        }
    }
}
