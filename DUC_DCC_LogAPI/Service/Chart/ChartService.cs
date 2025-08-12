using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Chart;
using Microsoft.EntityFrameworkCore;

namespace DUC_DCC_LogAPI.Service.Chart
{
    public class ChartService : IChart
    {
        private readonly AppDbContext _db;
        private readonly ResponseDto _response;
        private readonly MessageDto _message;
        //private readonly IMapper _mapper;

        public ChartService(AppDbContext db)
        {
            _db = db;
            _response = new ResponseDto();
            _message = new MessageDto();
          
        }

        public async Task<ResponseDto> GetChartBarAsync(int Year)
        {
            try
            {

                #region || backUp ||
                // var query = from a in _db.Month
                //             join b in _db.Application_Log on a.Id equals  b.Action_date_time.Month into ab
                //             from b in ab.DefaultIfEmpty()
                //             select new
                //             {
                //                 Id = a.Id,
                //                 Month = b.Action_date_time.Month,
                //                 App_log = b.App_log,
                //                 Name = a.Name,

                //             };

                // var result = await query
                //     .GroupBy(a => new { a.Id, a.Month,a.Name,a.App_log })
                //     .Select(g => new BarCharDto
                //     {
                //         month = g.Key.Id,
                //         Name = g.Key.Name,
                //         App_log = g.Key.App_log,
                //         CountData = g.Count(log => log != null)
                //     }).ToListAsync();

                //_response.Result = result;
                #endregion

                var query = from month in _db.Month
                            join log in _db.Application_Log.Where(l => l.Action_date_time.Year == Year)
                            on month.Id equals log.Action_date_time.Month into monthLogs
                            from ml in monthLogs.DefaultIfEmpty()  
                            group ml by new   
                            {
                                month.Id,
                                month.Name,
                                AppLog = ml.App_log,
                               
                            } into g
                           
                            select new BarCharDto
                            {
                                month = g.Key.Id,
                                Name = g.Key.Name,
                                App_log = g.Key.AppLog, 
                                CountData = g.Count(log => log != null),
                                Year = Year
                            };

                var result = await query.OrderBy(a=> a.month).ToListAsync();
                _response.Result = result;

            }
            catch (Exception ex) { 
              _response.IsSuccess = false;
            _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        public async Task<ResponseDto> GetChartDataAsync(int Year)
        {
            try
            {

                var query = from log in _db.Application_Log
                            where log.Action_date_time.Year == Year
                            group log by log.App_log into logGroup
                            select new ChartDto
                            {
                                Name = logGroup.Key,    
                                Count = logGroup.Count(),
                                Year = Year
                            };

            
                var chartData = await query.ToListAsync();

                _response.Result = chartData;



                #region || backup ||

                //var chartData = await _db.Application_Log
                //                              .Where(log => log.Admin_confirm != "")
                //                              .GroupBy(log => log.App_log)
                //                              .Select(logGroup => new ChartDto
                //                              {
                //                                  Name = logGroup.Key,
                //                                  Count = logGroup.Count()
                //                              })
                //                              .ToListAsync();

                //_response.Result = chartData;

                #endregion
            }
            catch (Exception ex)
            { 
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }
    }
}
