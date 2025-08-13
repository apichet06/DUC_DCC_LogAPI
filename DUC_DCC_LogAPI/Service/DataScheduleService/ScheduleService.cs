using AutoMapper;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.ApiSetting;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Utilities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
namespace DUC_DCC_LogAPI.Service.DccService
{
    public class ScheduleService : IScheduleService
    {

        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly string _dccApiUrl;
        private readonly ResponseDto _response;
        private const string Format = "yyyy-MM-dd HH:mm:ss";
        private readonly MessageDto _message;
        private IMapper _mapper;

        public ScheduleService(HttpClient httpClient, AppDbContext dbContext, IConfiguration configuration, IOptions<ApiSettings> apiSettings, IMapper mapper)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _dccApiUrl = apiSettings.Value.DccApiBaseUrl;
            _response = new ResponseDto();
            _message = new MessageDto();
            _mapper = mapper;
        }

        #region || DCC Import Data ||
        public async Task<ResponseDto>  ImportDucAsync()
        {
            try
            {
                DateTime formattedDate = new DateTime(2025, 8, 8);
                //DateTime endDate = startDate.AddDays(1);

                //DateTime formattedDate = DateTime.Today.AddDays(-1);
                string formattedDate1 = formattedDate.ToString("yyyy-MM-dd");
                var response = await _httpClient.GetAsync($"{_dccApiUrl}&action_datetime={formattedDate1}");
                response.EnsureSuccessStatusCode();

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                serializerOptions.Converters.Add(new CustomDateTimeConverter());

                var json = await response.Content.ReadAsStringAsync();
                var todos = JsonSerializer.Deserialize<List<Duc_crud>>(json, serializerOptions);
                if (todos != null && todos.Any())
                {
                    const int batchSize = 1000;

                    foreach (var batch in todos.Chunk(batchSize))
                    {
                        
                        var entities = batch.Select(t => new Application_log
                        {
                            
                            Group_name = t.Group_name,
                            Username = t.Username,
                            Action = t.Action,
                            Action_date_time = (DateTime)t.Action_datetime!,
                            Bu = t.Bu,
                            Detail = t.Detail,
                            Days_after_action = t.Days_after_action,
                            Download_more_10_files_day = t.Download_more_10_files_per_day,
                            App_log = "DUC",
                            Employee_resigning_within_one_month = t.Employee_resigning_within_one_month,
                            Event_type = t.Event_type,
                            Position = t.Position,
                            Resigned_date = t.Resigned_date,
                            Unauthorized = t.Unauthorized,
                            Upload_datetime = DateTime.Now
                        }).ToList();
                         
                    var  dataapp = await _dbContext.Application_Log.FirstOrDefaultAsync(
                        a=>a.Group_name == entities[0].Group_name && a.Username == entities[0].Username && a.Action == entities[0].Action && 
                        a.Action_date_time == entities[0].Action_date_time && a.Detail == entities[0].Detail);
                        if(dataapp == null)
                        await _dbContext.AddRangeAsync(entities); // ถ้าต้องการใส่ options หรือ token สามารถเพิ่มได้
                        //Console.WriteLine($"Inserted batch of {entities.Count} items.");
                    }

                    // ไม่มี need แล้วที่จะ SaveChangesAsync หลัง BulkInsert
                      await _dbContext.SaveChangesAsync();
                    _response.Result = formattedDate1;
                    _response.Message = _message.InsertMessage;

                }
            }
            catch (Exception ex)
            {
              _response.IsSuccess = false;
              _response.Message = ex.Message;
            }
            return _response;
        }
        #endregion

        public async Task<ResponseDto> ImportDccAsync() 
        {
            try
            {

                //DateTime startDate = DateTime.Today.AddDays(-1);
                //DateTime endDate = DateTime.Today;

                DateTime startDate = new DateTime(2025, 6, 29);
                DateTime endDate = startDate.AddDays(1);

                List<dcc_crud_log> objList = await _dbContext.dcc_crud_log
                    .Where(x => x.Action_datetime >= startDate && x.Action_datetime < endDate)
                    .ToListAsync(); 
              

                if (objList.Count > 0) {

                    const int batchSize = 1000;

                        foreach (var batch in objList.Chunk(batchSize))
                        {
                        var entities = batch.Select(t => new Application_log
                            {

                            Group_name = t.Group_name,
                            Username = t.Username,
                            Action = t.Action,
                            Action_date_time = t.Action_datetime,
                            Bu = t.Bu,
                            Detail = t.Detail,
                            Days_after_action = t.Resign_after_action,
                            Download_more_10_files_day = t.Is_over_10_file_per_day,
                            Is_bu_dcc = t.Is_not_dcc,
                            Employee_resigning_within_one_month = t.Is_resigned_within_1_month,
                            Event_type = t.Event_type,
                            Position = t.Position,
                            Resigned_date = t.Resigned_date,
                            Unauthorized = t.Unauthorized,
                            App_log = "DCC",
                            Upload_datetime = DateTime.Now 
                        }).ToList();

                        var dataapp = await _dbContext.Application_Log.FirstOrDefaultAsync(
                            a => a.Group_name == entities[0].Group_name && a.Username == entities[0].Username && a.Action == entities[0].Action && 
                            a.Action_date_time == entities[0].Action_date_time && a.Detail == entities[0].Detail );
                        if (dataapp == null)
                            await _dbContext.AddRangeAsync(entities);
                    }
                    await _dbContext.SaveChangesAsync();
                    _response.Result = $"{startDate} TO {endDate}"; 
                    _response.Message = _message.InsertMessage;
                }
              
            }
            catch (Exception ex) {

                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }


    }
}
 