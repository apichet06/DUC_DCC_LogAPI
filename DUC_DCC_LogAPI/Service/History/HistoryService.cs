using AutoMapper;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.History;
using MailKit.Search;
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
                IQueryable<Historys> query = _db.history.OrderByDescending(a=>a.action_datetime);
                if (request != null && request.Search != null && request.Search.Any())
                {
                    string searchTerm = request.Search!.ToLower();
                      query = query.Where(x =>
                    x.emp_no!.ToLower().Contains(searchTerm.Trim()) ||
                    x.fullname!.ToLower().Contains(searchTerm.Trim()) ||
                    x.action!.ToLower().Contains(searchTerm.Trim()) ||
                    x.app_log!.ToLower().Contains(searchTerm.Trim()) ||
                    x.details!.ToLower().Contains(searchTerm.Trim()) ||
                    x.comment!.ToLower().Contains(searchTerm.Trim()) ||
                    x.processType!.ToLower().Contains(searchTerm.Trim()) ||
                    x.group_name!.ToLower().Contains(searchTerm.Trim()) ||
                    x.username!.ToLower().Contains(searchTerm.Trim()) ||
                    (x.admin_confirm_event != null && x.admin_confirm_event.ToLower() == searchTerm));

                }

                if (!string.IsNullOrEmpty(request!.App_log))
                {
                    string appLogSearch = request.App_log.ToLower().Trim();
                    query = query.Where(x => x.app_log!.ToLower().Contains(appLogSearch));
                }

                if (request!.startDate.HasValue)
                {
                    DateTime startDate = request.startDate.Value.Date; // เอาเฉพาะส่วนวันที่ (เวลา 00:00:00)
                    query = query.Where(x => x.action_datetime >= startDate);
                }

                if (request.endDate.HasValue)
                {
                    DateTime endDateExclusive = request.endDate.Value.Date.AddDays(1); // เอาวันถัดไปตอน 00:00:00
                    query = query.Where(x => x.action_datetime < endDateExclusive);
                }

                var obj = await query.OrderByDescending(a => a.action_datetime).ToListAsync();
                var resule = _mapper.Map<List<Historys>>(query);

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
