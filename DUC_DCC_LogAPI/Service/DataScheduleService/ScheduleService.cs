using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.ApiSetting;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Options;
using System.Text.Json;
namespace DUC_DCC_LogAPI.Service.DccService
{
    public class ScheduleService : IScheduleService
    {

        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly string _dccApiUrl;

        public ScheduleService(HttpClient httpClient, AppDbContext dbContext, IConfiguration configuration, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _dccApiUrl = apiSettings.Value.DccApiBaseUrl;
        }
        public async Task ImportDccAsync()
        {
            try
            {
                var id = Guid.NewGuid();
                var response = await _httpClient.GetAsync($"{_dccApiUrl}?requestId={id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var todos = JsonSerializer.Deserialize<List<Dcc_crud>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (todos != null && todos.Any())
                {
                    const int batchSize = 1000;

                    foreach (var batch in todos.Chunk(batchSize))
                    {
                        var entities = batch.Select(t => new DUC_DCC_Log
                        {
                            Action = t.Action,
                            Action_date_time = t.Action_date_time,
                            Bu = t.Bu,
                            Detail = t.Detail,
                            Days_after_action = t.Days_after_action,
                            Download_more_10_files_day = t.Download_more_10_files_day,
                            dcc_duc = t.dcc_duc,
                            Employee_resigning_within_one_month = t.Employee_resigning_within_one_month,
                            Event_type = t.Event_type,
                            Group_name = t.Group_name,
                            Position = t.Position,
                            Resigned_date = t.Resigned_date,
                            Unauthorized = t.Unauthorized,
                            Username = t.Username,
                            Users_action = t.Users_action,
                            User_action_date = t.User_action_date
                        }).ToList();

                        await _dbContext.BulkInsertAsync(entities); // ถ้าต้องการใส่ options หรือ token สามารถเพิ่มได้
                        Console.WriteLine($"Inserted batch of {entities.Count} items.");
                    }

                    // ไม่มี need แล้วที่จะ SaveChangesAsync หลัง BulkInsert
                      //await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }



    }
}
 