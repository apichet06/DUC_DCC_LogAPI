using AutoMapper;
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

        public async Task<ResponseDto> GetChartDataAsync()
        {
            try
            {

                var query = from log in _db.Application_Log
                            where log.Admin_confirm != ""
                            group log by log.App_log into logGroup
                            select new ChartDto
                            {
                                Name = logGroup.Key,    // logGroup.Key คือค่าที่ใช้ group (ในที่นี้คือ app_log)
                                Count = logGroup.Count() // logGroup.Count() คือจำนวนรายการในแต่ละกลุ่ม
                            };

                // สั่งให้ query ทำงานและดึงข้อมูลจากฐานข้อมูล
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
