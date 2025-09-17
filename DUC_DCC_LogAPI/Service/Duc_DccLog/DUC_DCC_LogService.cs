using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models; 
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Models.Dto.History;
using DUC_DCC_LogAPI.Models.Dto.SaveDuc_DccLog; 
using DUC_DCC_LogAPI.Models.Dtos;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
 

using Microsoft.Extensions.Options; 
using System.Net; 
using System.Net.Mail;
using System.Text;
using static DUC_DCC_LogAPI.Constant.Constants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DUC_DCC_LogAPI.Service.Duc_DccLog
{
    public class DUC_DCC_LogService : IDUC_DCC_Log
    {
        private readonly AppDbContext _db;
        private readonly ResponseDto _response;
        private readonly MessageDto _message;
        private readonly IMapper _mapper;
        private readonly EmailSettings _smtpSettings;
        private readonly IWebHostEnvironment _env;
  
        private readonly HttpClient _httpClient;
        private readonly string _url = "https://jsonplaceholder.typicode.com/todos";


        public DUC_DCC_LogService(AppDbContext dbContext, IMapper mapper, IOptions<EmailSettings> emailSettings, IWebHostEnvironment env , HttpClient httpClient  )
        {
            _db = dbContext;
            _mapper = mapper;
            _message = new MessageDto();
            _response = new ResponseDto();
            _smtpSettings = emailSettings.Value;
            _env = env;
            _httpClient = httpClient;
             
        }

        public async Task<ResponseDto> GetCountReportLog()
        {
            try
            {
                var today = DateTime.Now.Date;
                var yesterday = today.AddDays(-1);
                var targetAppLogs = new[] { "DUC", "DCC" };

                var query = from log in _db.Application_Log
                            where  log.Admin_confirm == null &&
                                  targetAppLogs.Contains(log.App_log)
                            group log by log.App_log into g
                            select new
                            {
                                AppLog = g.Key,
                                TotalCount = g.Count(),
                                Count_Yesterday = g.Count(l => l.Action_date_time.Date == yesterday),
                                Count_AllBeforeYesterday = g.Count(l => l.Action_date_time.Date < yesterday)
                            };

                var result = await query.ToListAsync();
                _response.Result = result;
            }
            catch (Exception ex) 
            {

                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        public async Task<ResponseDto> GetCountAuditLog()
        {
            try
            {
                var today = DateTime.Now.Date;
                var yesterday = today.AddDays(-1);
                var targetAppLogs = new[] { "DUC", "DCC" };

                var query = from log in _db.Application_Log
                            where !string.IsNullOrEmpty(log.Admin_confirm) &&
                                  targetAppLogs.Contains(log.App_log)
                            group log by log.App_log into g
                            select new
                            {
                                AppLog = g.Key,
                                TotalCount = g.Count(),
                                Count_Yesterday = g.Count(l => l.Action_date_time.Date == yesterday),
                                Count_AllBeforeYesterday = g.Count(l => l.Action_date_time.Date < yesterday)
                            };

                var result = await query.ToListAsync();
                _response.Result = result;
            }
            catch (Exception ex) 
            {

                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        public async Task<ResponseDto> GetDataDUC(Application_log request)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<Application_log>>(_url);
                var dataInsert = data!.Select(a => new Application_log
                {
                    Group_name = a.Group_name,
                     
                });
                await _db.Application_Log.AddRangeAsync(dataInsert);
                var affectRows = await _db.SaveChangesAsync();

                 
            }
            catch (Exception ex)
            { 
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        public async Task<ResponseDto> GetDataDCC()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<Application_log>>(_url);
                var dataInsert = data!.Select(a => new Application_log
                {
                    Group_name = a.Group_name, 
                });
                await _db.Application_Log.AddRangeAsync(dataInsert);
                var affectRows = await _db.SaveChangesAsync();
                 
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        #region || ExportExcel Form base64 ||
        //public async Task<ResponseDto<FileDownloadDto>> ExportExcel(SearchDto request)
        //{
        //    try
        //    {

        //        IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Admin_confirm == null);

        //        if (request != null && request.Search != null && request.Search.Any())
        //        {
        //            string searchTerm = request.Search.ToLower();

        //            query = query.Where(x =>
        //                x.Group_name!.ToLower().Contains(searchTerm) ||
        //                x.Username!.ToLower().Contains(searchTerm) ||
        //                x.Action!.ToLower().Contains(searchTerm) ||
        //                x.Detail!.ToLower().Contains(searchTerm) ||
        //                x.Bu!.ToLower().Contains(searchTerm) ||
        //                x.Position!.ToLower().Contains(searchTerm) ||
        //                x.Event_type!.ToLower().Contains(searchTerm) ||
        //                x.Unauthorized!.ToLower().Contains(searchTerm) ||
        //                x.Download_more_10_files_day!.ToLower().Contains(searchTerm) ||
        //                x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm) ||
        //                x.Admin_confirm!.ToLower().Contains(searchTerm));
        //        }

        //        if (request!.startDate.HasValue)
        //        {
        //            DateTime startDate = request.startDate.Value.Date; // เอาเฉพาะส่วนวันที่ (เวลา 00:00:00)
        //            query = query.Where(x => x.Action_date_time >= startDate);
        //        }

        //        if (request.endDate.HasValue)
        //        {
        //            DateTime endDateExclusive = request.endDate.Value.Date.AddDays(1); // เอาวันถัดไปตอน 00:00:00
        //            query = query.Where(x => x.Action_date_time < endDateExclusive);
        //        }

        //        var obj = await query.ToListAsync();
        //        var mappList = _mapper.Map<List<Application_logDto>>(obj);

        //        var filePath = Path.Combine(_env.ContentRootPath, "Files", "DUC_DCC_LogService.xlsx");

        //        using var workbook = new XLWorkbook();
        //        var worksheet = workbook.Worksheets.Add("Report");
        //        int row = 3;
        //        int autoId = 1;
                 

        //        foreach (var item in mappList)
        //        {

        //            item.Id = autoId++;

        //            worksheet.Cell($"A{row}").Value = item.Id;
        //            worksheet.Cell($"B{row}").Value = item.Group_name;
        //            worksheet.Cell($"C{row}").Value = item.Username;
        //            worksheet.Cell($"D{row}").Value = item.Action;
        //            worksheet.Cell($"E{row}").Value = item.Action_date_time;
        //            worksheet.Cell($"F{row}").Value = item.Detail;
        //            worksheet.Cell($"G{row}").Value = item.Bu;
        //            worksheet.Cell($"H{row}").Value = item.Position;
        //            worksheet.Cell($"I{row}").Value = item.Resigned_date;
        //            worksheet.Cell($"J{row}").Value = item.Days_after_action;
        //            worksheet.Cell($"K{row}").Value = item.Event_type;
        //            worksheet.Cell($"L{row}").Value = item.Unauthorized;
        //            worksheet.Cell($"M{row}").Value = item.Download_more_10_files_day;
        //            worksheet.Cell($"N{row}").Value = item.Employee_resigning_within_one_month;

        //            row++;
        //        }
        //        worksheet.Columns().AdjustToContents();
        //        var newlastRow = row - 1;
        //        var range = worksheet.Range($"A3:N{newlastRow}");

        //        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //        range.Style.Border.OutsideBorderColor = XLColor.Black;
        //        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //        range.Style.Border.InsideBorderColor = XLColor.Black;

        
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            byte[] content = stream.ToArray();

        //            return new ResponseDto<FileDownloadDto>()
        //            {
        //                IsSuccess = true,
        //                Message = "Success",
        //                Data = new FileDownloadDto()
        //                {
        //                    Content = content,
        //                    FileName = $"รายการงาน.xlsx",
        //                    ContentType = ContentTypeConfig.Xlsx,
        //                },
        //                Status = HttpStatusCode.OK
        //            };
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        // 5. แก้ไขการ Return ค่าใน Catch Block
        //        return new ResponseDto<FileDownloadDto>()
        //        {
        //            IsSuccess = false,
        //            Message = "An error occurred: " + ex.Message,
        //            Data = null,
        //            Status = HttpStatusCode.InternalServerError
        //        };
        //    }
        //}
        #endregion
          
        #region || ExportExcelLog ||

        public async Task<byte[]> ExportExcelLog(SearchDto request)
        {
            IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Admin_confirm == null && x.App_log == request.tapData && (x.Event_type == request.CheckBoxUsual || x.Event_type == request.CheckBoxUnusual));

            //if (query == null)
            //      _response.Message = _message.Not_found;
           

            if (request != null && request.Search != null && request.Search.Any())
            {
                string searchTerm = request.Search.ToLower();

                query = query.Where(x =>
                    x.Group_name!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Username!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Action!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Detail!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Bu!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Position!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Event_type!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Unauthorized!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Download_more_10_files_day!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm.Trim()) ||
                    x.Admin_confirm!.ToLower().Contains(searchTerm.Trim()));
            }

            if (request!.startDate.HasValue)
            {
                DateTime startDate = request.startDate.Value.Date; // เอาเฉพาะส่วนวันที่ (เวลา 00:00:00)
                query = query.Where(x => x.Action_date_time >= startDate);
            }

            if (request.endDate.HasValue)
            {
                DateTime endDateExclusive = request.endDate.Value.Date.AddDays(0); // เอาวันถัดไปตอน 00:00:00
                query = query.Where(x => x.Action_date_time < endDateExclusive);
            }

            var obj = await query.ToListAsync();
            var mappList = _mapper.Map<List<Application_logDto>>(obj);
            var fileName = request.tapData == "DUC" ? "reportDUC.xlsx" : "reportDCC.xlsx";
            var filePath = Path.Combine(_env.ContentRootPath, "Files", fileName);

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet("Report");
             

            int autoId = 1;
            int row = 3;
           
            foreach (var item in mappList)
            {
                item.Id = autoId++;

                worksheet.Cell($"A{row}").Value = item.Id;
                worksheet.Cell($"B{row}").Value = item.Group_name;
                worksheet.Cell($"C{row}").Value = item.Username;
                worksheet.Cell($"D{row}").Value = item.Action;
                worksheet.Cell($"E{row}").Value = item.Action_date_time;
                worksheet.Cell($"F{row}").Value = item.Detail;
                worksheet.Cell($"G{row}").Value = item.Bu;
                worksheet.Cell($"H{row}").Value = item.Position;
                worksheet.Cell($"I{row}").Value = item.Resigned_date;
                worksheet.Cell($"J{row}").Value = item.Days_after_action;
                worksheet.Cell($"K{row}").Value = item.Event_type;
                worksheet.Cell($"L{row}").Value = item.Unauthorized;
                worksheet.Cell($"M{row}").Value = item.Download_more_10_files_day;
                worksheet.Cell($"N{row}").Value = item.Employee_resigning_within_one_month;
                if(request.tapData == "DCC")
                    worksheet.Cell($"O{row}").Value = item.Is_bu_dcc; 
                row++;
            }

            // 3. Apply adjustments and styling only if new rows were added.
            var newlastRow = row - 1;
            var rowtapData = request.tapData == "DUC" ? "N" : "O"; // Determine the last column based on tapData
            var range = worksheet.Range($"A3:{rowtapData}{newlastRow}");

                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.OutsideBorderColor = XLColor.Black;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorderColor = XLColor.Black;
            
             
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            //return Task.FromResult(stream.ToArray());
            return stream.ToArray();
        }

        #endregion

        #region || ExportExcelDCCLog ||

        //public async Task<byte[]> ExportExcelDccLog(SearchDto request)
        //{
        //    IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Admin_confirm == null && x.App_log == request.tapData);

        //    if (request != null && request.Search != null && request.Search.Any())
        //    {
        //        string searchTerm = request.Search.ToLower();

        //        query = query.Where(x =>
        //            x.Group_name!.ToLower().Contains(searchTerm) ||
        //            x.Username!.ToLower().Contains(searchTerm) ||
        //            x.Action!.ToLower().Contains(searchTerm) ||
        //            x.Detail!.ToLower().Contains(searchTerm) ||
        //            x.Bu!.ToLower().Contains(searchTerm) ||
        //            x.Position!.ToLower().Contains(searchTerm) ||
        //            x.Event_type!.ToLower().Contains(searchTerm) ||
        //            x.Unauthorized!.ToLower().Contains(searchTerm) ||
        //            x.Download_more_10_files_day!.ToLower().Contains(searchTerm) ||
        //            x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm) ||
        //            x.Admin_confirm!.ToLower().Contains(searchTerm));
        //    }

        //    if (request!.startDate.HasValue)
        //    {
        //        DateTime startDate = request.startDate.Value.Date; // เอาเฉพาะส่วนวันที่ (เวลา 00:00:00)
        //        query = query.Where(x => x.Action_date_time >= startDate);
        //    }

        //    if (request.endDate.HasValue)
        //    {
        //        DateTime endDateExclusive = request.endDate.Value.Date.AddDays(0); // เอาวันถัดไปตอน 00:00:00
        //        query = query.Where(x => x.Action_date_time < endDateExclusive);
        //    }

        //    var obj = await query.ToListAsync();
        //    var mappList = _mapper.Map<List<Application_logDto>>(obj);

        //    var filePath = Path.Combine(_env.ContentRootPath, "Files", "reportDCC.xlsx");

        //    using var workbook = new XLWorkbook(filePath);
        //    var worksheet = workbook.Worksheet("Report");


        //    int autoId = 1;
        //    int row = 3;

        //    foreach (var item in mappList)
        //    {
        //        item.Id = autoId++;

        //        worksheet.Cell($"A{row}").Value = item.Id;
        //        worksheet.Cell($"B{row}").Value = item.Group_name;
        //        worksheet.Cell($"C{row}").Value = item.Username;
        //        worksheet.Cell($"D{row}").Value = item.Action;
        //        worksheet.Cell($"E{row}").Value = item.Action_date_time;
        //        worksheet.Cell($"F{row}").Value = item.Detail;
        //        worksheet.Cell($"G{row}").Value = item.Bu;
        //        worksheet.Cell($"H{row}").Value = item.Position;
        //        worksheet.Cell($"I{row}").Value = item.Resigned_date;
        //        worksheet.Cell($"J{row}").Value = item.Days_after_action;
        //        worksheet.Cell($"K{row}").Value = item.Event_type;
        //        worksheet.Cell($"L{row}").Value = item.Unauthorized;
        //        worksheet.Cell($"M{row}").Value = item.Download_more_10_files_day;
        //        worksheet.Cell($"N{row}").Value = item.Employee_resigning_within_one_month;
        //        worksheet.Cell($"O{row}").Value = item.Is_not_dcc;

        //        row++;
        //    }

        //    // 3. Apply adjustments and styling only if new rows were added.
        //    var newlastRow = row - 1;
        //    var range = worksheet.Range($"A3:N{newlastRow}");

        //    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //    range.Style.Border.OutsideBorderColor = XLColor.Black;
        //    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //    range.Style.Border.InsideBorderColor = XLColor.Black;


        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    //return Task.FromResult(stream.ToArray());
        //    return stream.ToArray();
        //}
        #endregion

        #region || ExportReportLog ||

        private void ExportReportLog(List<Application_logDto> dataList)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Files", "reportDUC_DCC.xlsx");

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet("Report");


            int autoId = 1;
            int row = 3;

            foreach (var item in dataList)
            {
                item.Id = autoId++;

                worksheet.Cell($"A{row}").Value = item.Id;
                worksheet.Cell($"B{row}").Value = item.Group_name;
                worksheet.Cell($"C{row}").Value = item.Username;
                worksheet.Cell($"D{row}").Value = item.Action;
                worksheet.Cell($"E{row}").Value = item.Action_date_time;
                worksheet.Cell($"F{row}").Value = item.Detail;
                worksheet.Cell($"G{row}").Value = item.Bu;
                worksheet.Cell($"H{row}").Value = item.Position;
                worksheet.Cell($"I{row}").Value = item.Resigned_date;
                worksheet.Cell($"J{row}").Value = item.Days_after_action;
                worksheet.Cell($"K{row}").Value = item.Event_type;
                worksheet.Cell($"L{row}").Value = item.Unauthorized;
                worksheet.Cell($"M{row}").Value = item.Download_more_10_files_day;
                worksheet.Cell($"N{row}").Value = item.Employee_resigning_within_one_month;

                row++;
            }

            // 3. Apply adjustments and styling only if new rows were added.
            var newlastRow = row - 1;
            var range = worksheet.Range($"A3:N{newlastRow}");

            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.Black;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorderColor = XLColor.Black;


            using var stream = new MemoryStream();
            workbook.SaveAs(stream); 
           

        }

        #endregion

        #region || GetById || 
        public async Task<ResponseDto> GetById(int id)
        {
            try
            {

                //IQueryable<Application_log> applog = _db.Application_Log.Where(x => x.Id == id);
                var obj =  await( from a in _db.Application_Log
                                  join b in _db.Users_Permission on a.Admin_confirm equals b.emp_no into userGroup
                                from b in userGroup.DefaultIfEmpty()  
                               where a.Id == id
                             select new Application_logDto
                                {
                                    Id = id,
                                    Bu_code =a.Bu_code,
                                    Group_name = a.Group_name,
                                    Username = a.Username,
                                    Action = a.Action,
                                    Action_date_time = a.Action_date_time,
                                    Detail = a.Detail,
                                    Bu = a.Bu == null ? "-":a.Bu,
                                    Position = a.Position == null ? "-":a.Position,
                                    Resigned_date = a.Resigned_date,
                                    Days_after_action = a.Days_after_action,
                                    Event_type = a.Event_type,
                                    Unauthorized = a.Unauthorized,
                                    Download_more_10_files_day = a.Download_more_10_files_day,
                                    Employee_resigning_within_one_month = a.Employee_resigning_within_one_month,
                                    Is_bu_dcc = a.Is_bu_dcc,
                                    Admin_confirm = $"{b.firstname} {b.lastname}",
                                    Admin_confirm_date = a.Admin_confirm_date,
                                    Admin_confirm_comment = a.Admin_confirm_comment,
                                    Admin_confirm_event = a.Admin_confirm_event, 

                                }).ToListAsync();

                if (obj == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = _message.Not_found;
                }
                else
                {
                  
                    _response.Result = obj;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }
        #endregion

        #region || GetList Application Log||
        public async Task<ResponseDto>GetList(SearchDto request)
        {
            try
            {
                string plantSuffix = "." + request.plant;
                IQueryable<Application_log> query = _db.Application_Log.Where(x=>x.Admin_confirm == null && x.App_log == request.tapData && x.Bu_code!.EndsWith(plantSuffix) && (x.Event_type == request.CheckBoxUsual || x.Event_type == request.CheckBoxUnusual))
                     .OrderByDescending(a=>a.Action_date_time);

                if (request != null && request.Search != null && request.Search.Any())
                {
                    string searchTerm = request.Search.ToLower();

                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchTerm.Trim()) ||
                        x.Group_name!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Username!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Action!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Detail!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Bu!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Position!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Event_type!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Unauthorized!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Download_more_10_files_day!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Admin_confirm!.ToLower().Contains(searchTerm.Trim()));
                       
                }

                if (request!.startDate.HasValue)
                {
                    DateTime startDate = request.startDate.Value.Date; // เอาเฉพาะส่วนวันที่ (เวลา 00:00:00)
                    query = query.Where(x => x.Action_date_time >= startDate);
                }

                if (request.endDate.HasValue)
                {
                    DateTime endDateExclusive = request.endDate.Value.Date.AddDays(1); // เอาวันถัดไปตอน 00:00:00
                    query = query.Where(x => x.Action_date_time < endDateExclusive);
                }

                var obj = await query.OrderBy(a => a.Event_type == "Unusual Event" ? 0 : 1).ThenByDescending(a => a.Action_date_time).ToListAsync();
                var mappList = _mapper.Map<List<Application_logDto>>(obj); 
                
                _response.Result = mappList;
                

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        #endregion
         
        #region || SendMail Schedule ||
        public async Task<ResponseDto> SendMail()
        {
            try
            {

                //DateTime startDate = new DateTime(2025, 8, 8);
                DateTime startDate = DateTime.Today.AddDays(-1);
                DateTime endDate = DateTime.Today;

                List<Application_log> objDucList = await _db.Application_Log
                    .Where(x => x.Action_date_time >= startDate && x.Action_date_time < endDate && x.App_log == "DUC" && x.Event_type == "Unusual Event")
                    .ToListAsync();
                var mappDucList = _mapper.Map<List<Application_logDto>>(objDucList);

           var ducStreem =  ExportExcleDucSendMail(mappDucList);

                List<Application_log> objDccList = await _db.Application_Log
                 .Where(x => x.Action_date_time >= startDate && x.Action_date_time < endDate && x.App_log == "DCC" && x.Event_type == "Unusual Event")
                 .ToListAsync();
                var mappDccList = _mapper.Map<List<Application_logDto>>(objDccList);

            var dccStreem =  ExportExcleDccSendMail(mappDccList);


                var sb = new StringBuilder();
                #region || HTML Template ||
                sb.AppendLine(@"<!DOCTYPE html>
                                <html>
                                <head>
                                <style>
                                #customers {
                                  font-family: Arial, Helvetica, sans-serif;
                                  border-collapse: collapse;
                                  width: 100%; 
                                }

                                #customers td, #customers th {
                                  border: 1px solid #ddd;
                                  padding: 5px;
                                  font-size: 11px;
                                }

                                #customers tr:nth-child(even) {background-color: #f2f2f2;}

                                #customers tr:hover {background-color: #ddd;}

                                #customers th {
                                  padding-top: 7px;
                                  padding-bottom: 7px;
                                  text-align: left;
                                  background-color: #04AA6D;
                                  color: white;
                                }
                                </style>
                                </head>
                                <body>");
                sb.AppendLine(@"<a href=""https://fits/CRUDLogs/applog/report-log"">Go to website</a>"); 
                sb.AppendLine($"<h4>DUC Report log dated {startDate.ToString("dd MMM yyyy")}</h4>");

                if(objDucList.Count > 0)
                {
                     
                sb.AppendLine(@"<table id=""customers"">
                    <tr>
                      <th>NO</th>
                      <th>GROUP NAME</th>
                      <th>USERNAME</th> 
                      <th>ACTION DATE/TIME</th>
                      <th>DETAIL</th>
                      <th>BU</th>
                      <th>POSITION</th> 
                      <th>Event type</th>
                      <th>Link</th>  
                    </tr>");

                int ducIndex = 1;
                foreach (var log in objDucList)
                {
                        var events = log.Event_type == "Usual Event" ? "black" : "red";
                        sb.AppendLine($@"
                        <tr>
                          <td>{ducIndex++}</td>
                          <td>{log.Group_name}</td>
                          <td>{log.Username}</td>
                          <td>{log.Action_date_time:yyyy-MM-dd HH:mm:ss}</td>
                          <td>{log.Detail}</td>
                          <td>{log.Bu}</td>
                          <td>{log.Position}</td> 
                          <td style=""color: {events};"">{log.Event_type}</td>
                          <td><a href=""https://fits/CRUDLogs/applog/report-log/{log.Id}/{log.App_log}"">Click</a></td>
                        </tr>");
                }

                sb.AppendLine("</table>");
                }
                else
                {
                    sb.AppendLine("<h5>&nbsp;&nbsp;The report log is currently empty.</h5>");
                }

                sb.AppendLine($"<h4>DCC Report log dated {startDate.ToString("dd MMM yyyy")}</h4>");

                if (objDccList.Count > 0)
                { 
                sb.AppendLine(@"<table id=""customers"">
                    <tr>
                      <th>NO</th>
                      <th>GROUP NAME</th>
                      <th>USERNAME</th> 
                      <th>ACTION DATE/TIME</th>
                      <th>DETAIL</th>
                      <th>BU</th>
                      <th>POSITION</th> 
                      <th>Event type</th>
                      <th>Link</th>  
                    </tr>");

                int dccIndex = 1;
                foreach (var log in objDccList)
                {
                      var events =  log.Event_type== "Usual Event" ?  "black":"red";
                    sb.AppendLine($@"
                                <tr>
                                  <td>{dccIndex++}</td>
                                  <td>{log.Group_name}</td>
                                  <td>{log.Username}</td>
                                  <td>{log.Action_date_time:yyyy-MM-dd HH:mm:ss}</td>
                                  <td>{log.Detail}</td>
                                  <td>{log.Bu}</td>
                                  <td>{log.Position}</td> 
                                  <td style=""color: {events};"">{log.Event_type}</td>
                                  <td><a href=""https://fits/CRUDLogs/applog/report-log/{log.Id}/{log.App_log}"">Click</a></td>
                                </tr>");

                } 
                sb.AppendLine("</table>");
                }
                else
                {
                    sb.AppendLine("<h5>&nbsp;&nbsp; The report log is currently empty. </h5>");
                }
                    sb.AppendLine("</body></html>");
                #endregion
                string body = sb.ToString();
                 
                var message = new MailMessage();
                message.From = new MailAddress(_smtpSettings.SenderEmail!, _smtpSettings.SenderName);
                message.To.Add("apichets@fabrinet.co.th");
                message.Subject = "AutoMail";
                message.Body = body;
                message.IsBodyHtml = true;
                message.Attachments.Add(new Attachment(ducStreem,"reportDUCSendmail.xlsx", ContentTypeConfig.Xlsx));
                message.Attachments.Add(new Attachment(dccStreem,"reportDCCSendmail.xlsx", ContentTypeConfig.Xlsx));


                using (var client = new SmtpClient(_smtpSettings.SmtpServer))
                {
                    client.EnableSsl = false; // ถ้าเปิดใช้ Port เช่น 587 ,25 ให้ EnableSsl เป็น true
                    //client.UseDefaultCredentials = true; // สำคัญ
                    client.UseDefaultCredentials = false;
                    //client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password); //ถ้าเป็นระบบภายในไม่ต้องมีการยืนยันตัวตน รหัสผ่าน
                    await client.SendMailAsync(message);
                }
                string formattedstartDate = startDate.ToString("yyyy-MM-dd");
                _response.Result = formattedstartDate;
                _response.Message = _message.SendmailSuccess;

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                Console.WriteLine(ex.ToString());
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;

        }
        #endregion

        #region || AccceptDataById ||
        public async Task<ResponseDto> DataAcceptById(DataAcceptByIdDto request, int id)
        {
            try
            {

                var history = new Historys(); 

                var applicationLog = await _db.Application_Log.FirstOrDefaultAsync(a => a.Id == id);
                if (applicationLog == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ไม่พบข้อมูล Log ที่ต้องการอนุมัติ"; // ข้อความที่เป็นมิตรกับผู้ใช้
                    return _response;
                }
                var user = await _db.Users_Permission.FirstOrDefaultAsync(u => u.emp_email == request.Admin_confirm);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ไม่พบข้อมูลผู้ใช้ที่ทำการอนุมัติ";
                    return _response;
                } 
                applicationLog.Admin_confirm = user.emp_no;
                applicationLog.Admin_confirm_comment = request.Admin_confirm_comment;
                applicationLog.Admin_confirm_date = DateTime.Now;
                applicationLog.Admin_confirm_event = request.Admin_confirm_event;

                history.emp_no = user!.emp_no;
                history.fullname = $"{user.firstname} {user.lastname}";
                history.action = applicationLog.Action;
                history.details = applicationLog.Detail;
                history.app_logId = applicationLog.Id;
                history.app_log = applicationLog.App_log;
                history.comment = applicationLog.Admin_confirm_comment;
                history.action_datetime = DateTime.Now;
                history.processType = "Confirm";
                history.bu_code = applicationLog.Bu_code;
                history.event_type = applicationLog.Event_type;
                history.admin_confirm_event = applicationLog.Admin_confirm_event;
                history.username = applicationLog.Username;
                history.group_name = applicationLog.Group_name;
                history.master_action_datetime = applicationLog.Action_date_time;




                _db.history.Add(history);
 
                await _db.SaveChangesAsync();
                _response.IsSuccess = true;
                _response.Message = _message.UpdateMessage;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        #endregion

        #region || EditDataAccept แก้ไขหลังจากบันทึกไปแล้ว || 
        public async Task<ResponseDto> EditDataAccept(EditDataAcceptDto request, int id)
        {
            try
            {
                var history = new Historys();


                var applicationLog = await _db.Application_Log.FirstOrDefaultAsync(a => a.Id == id);
                if (applicationLog == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ไม่พบข้อมูล Log ที่ต้องการแก้ไข"; // ข้อความที่เป็นมิตรกับผู้ใช้
                    return _response;
                }

                var user = await _db.Users_Permission.FirstOrDefaultAsync(u => u.emp_email == request.Admin_confirm_edit);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ไม่พบข้อมูลผู้ใช้ที่ทำการอนุมัติ";
                    return _response;
                } 
                applicationLog.Admin_confirm_comment = request.Admin_confirm_comment;
                applicationLog.Admin_edit_confirm_date = DateTime.Now;
                applicationLog.Admin_confirm_edit = user.emp_no;
                applicationLog.Admin_confirm_event = request.Admin_confirm_event;

                history.emp_no = user!.emp_no;
                history.fullname = $"{user.firstname} {user.lastname}";
                history.action = applicationLog.Action;
                history.details = applicationLog.Detail;
                history.app_logId = applicationLog.Id;
                history.app_log = applicationLog.App_log;
                history.comment = applicationLog.Admin_confirm_comment;
                history.action_datetime = DateTime.Now;
                history.processType = "Edit confirm";
                history.bu_code = applicationLog.Bu_code;
                history.event_type = applicationLog.Event_type;
                history.admin_confirm_event = applicationLog.Admin_confirm_event;
                history.username = applicationLog.Username;
                history.group_name = applicationLog.Group_name;
                history.master_action_datetime = applicationLog.Action_date_time;


                _db.history.Add(history);
                //_db.Application_Log.Update(sqlApplication_Log); 
                await _db.SaveChangesAsync();

                _response.IsSuccess = true;
                _response.Message = _message.UpdateMessage;


            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        #endregion


        #region || Update Comfirmed Reportlog ||
        public async Task<ResponseDto> UpdateList(CheckedDataDto request)
        {
            try
            {
                 var SqlUser = await _db.Users_Permission.FirstOrDefaultAsync(a=>a.emp_email == request.Admin_confirm);

                if (SqlUser?.Id == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "No items to update.";
                    return _response;
                }

             
 


                var logsToUpdate = await _db.Application_Log
                                            .Where(log => request.Id!.Contains(log.Id))
                                            .ToListAsync();

                if (!logsToUpdate.Any())  
                {
                    _response.IsSuccess = false;
                    _response.Message = "No matching records found.";
                    return _response;
                }
                 
                 
                foreach (var log in logsToUpdate)
                { 
                    log.Admin_confirm= SqlUser!.emp_no;
                    log.Admin_confirm_comment = request.Admin_confirm_comment;   
                    log.Admin_confirm_date = DateTime.Now;
                    log.Admin_confirm_event = request.Admin_confirm_event;
                     

                    var history = new Historys
                    {
                        emp_no = SqlUser.emp_no,
                        fullname = $"{SqlUser.firstname} {SqlUser.lastname}",
                        action = log.Action, 
                        details = log.Detail,
                        app_logId = log.Id,
                        app_log = log.App_log,
                        comment = log.Admin_confirm_comment,
                        action_datetime = DateTime.Now,
                        processType = "Confirm",
                         bu_code = log.Bu_code,
                        event_type = log.Event_type,
                        admin_confirm_event = log.Admin_confirm_event,
                        username = log.Username,
                        group_name = log.Group_name,
                        master_action_datetime = log.Action_date_time
                    };
                     

                    _db.history.Add(history);
                }
                 
                
                await _db.SaveChangesAsync();

                _response.IsSuccess = true;
                _response.Message = $"{logsToUpdate.Count} records updated successfully.";


            }
            catch (Exception ex) {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
                _response.Message = _message.an_error_occurred + ex.InnerException!.Message;
            }
            return _response;
        }

        #endregion

        #region || Export Excel send Mail Schedule ||
        private MemoryStream ExportExcleDucSendMail(List<Application_logDto> dataList)
        { 
            var filePath = Path.Combine(_env.ContentRootPath, "Files", "reportDUCSendMail.xlsx");

            using  var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet("Report");
                int row = 3;
                int autoId = 1;

                // remove Data
                int lastRow = worksheet.LastRowUsed()!.RowNumber();
                if (lastRow >= row)
                {
                    worksheet.Rows(row, lastRow).Delete();
                }
                if (dataList.Count > 0)
                {
                    foreach (var item in dataList)
                    {

                        item.Id = autoId++;

                        worksheet.Cell($"A{row}").Value = item.Id;
                        worksheet.Cell($"B{row}").Value = item.Group_name;
                        worksheet.Cell($"C{row}").Value = item.Username;
                        worksheet.Cell($"D{row}").Value = item.Action;
                        worksheet.Cell($"E{row}").Value = item.Action_date_time;
                        worksheet.Cell($"F{row}").Value = item.Detail;
                        worksheet.Cell($"G{row}").Value = item.Bu;
                        worksheet.Cell($"H{row}").Value = item.Position;
                        worksheet.Cell($"I{row}").Value = item.Resigned_date;
                        worksheet.Cell($"J{row}").Value = item.Days_after_action;
                        worksheet.Cell($"K{row}").Value = item.Event_type;
                        worksheet.Cell($"L{row}").Value = item.Unauthorized;
                        worksheet.Cell($"M{row}").Value = item.Download_more_10_files_day;
                        worksheet.Cell($"N{row}").Value = item.Employee_resigning_within_one_month;

                        row++;
                    }
                    var newlastRow = row - 1;
                    var range = worksheet.Range($"A3:N{newlastRow}");

                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.OutsideBorderColor = XLColor.Black;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorderColor = XLColor.Black;
                }
                var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0; // Reset the stream position to the beginning
            return ms;
           
          
        }

        private MemoryStream ExportExcleDccSendMail(List<Application_logDto> dataList)
        { 
            var filePath = Path.Combine(_env.ContentRootPath, "Files", "reportDCCSendMail.xlsx");

            using var workbook = new XLWorkbook(filePath);
           
                    var worksheet = workbook.Worksheet("Report");
                    int row = 3;
                    int autoId = 1;

                    // remove Data
                    int lastRow = worksheet.LastRowUsed()!.RowNumber();
                    if (lastRow >= row)
                    {
                        worksheet.Rows(row, lastRow).Delete();
                    }
                    if(dataList.Count > 0)
                    {
                 
                    foreach (var item in dataList)
                    {

                        item.Id = autoId++;

                        worksheet.Cell($"A{row}").Value = item.Id;
                        worksheet.Cell($"B{row}").Value = item.Group_name;
                        worksheet.Cell($"C{row}").Value = item.Username;
                        worksheet.Cell($"D{row}").Value = item.Action;
                        worksheet.Cell($"E{row}").Value = item.Action_date_time;
                        worksheet.Cell($"F{row}").Value = item.Detail;
                        worksheet.Cell($"G{row}").Value = item.Bu;
                        worksheet.Cell($"H{row}").Value = item.Position;
                        worksheet.Cell($"I{row}").Value = item.Resigned_date;
                        worksheet.Cell($"J{row}").Value = item.Days_after_action;
                        worksheet.Cell($"K{row}").Value = item.Event_type;
                        worksheet.Cell($"L{row}").Value = item.Unauthorized;
                        worksheet.Cell($"M{row}").Value = item.Download_more_10_files_day;
                        worksheet.Cell($"N{row}").Value = item.Employee_resigning_within_one_month;
                        worksheet.Cell($"O{row}").Value = item.Is_bu_dcc;
                        row++;
                    }
                    var newlastRow = row - 1;
                    var range = worksheet.Range($"A3:O{newlastRow}");

                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.OutsideBorderColor = XLColor.Black;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorderColor = XLColor.Black;

                    }
                    var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0; // Reset the stream position to the beginning
            return ms;

        }

        #endregion

        #region || ExportExcelAccept || 
        public async Task<byte[]> ExportExcelAccept(SearchDto request)
        {
            var obj = await BuildSaveLogQuery(request)
                .OrderByDescending(a => a.Action_date_time).ToListAsync();
            var mappList = _mapper.Map<List<SaveDUC_DCC_logDto>>(obj);
            var fileName = request.tapData == "DUC" ? "reportDUC_Accept.xlsx" : "reportDCC_Accept.xlsx";
            var filePath = Path.Combine(_env.ContentRootPath, "Files", fileName);

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet("Report");

            int autoId = 1;
            int row = 3;

            foreach (var item in mappList)
            {
                item.Id = autoId++;

                worksheet.Cell($"A{row}").Value = item.Id;
                worksheet.Cell($"B{row}").Value = item.Group_name;
                worksheet.Cell($"C{row}").Value = item.Username;
                worksheet.Cell($"D{row}").Value = item.Action;
                worksheet.Cell($"E{row}").Value = item.Action_date_time;
                worksheet.Cell($"F{row}").Value = item.Detail;
                worksheet.Cell($"G{row}").Value = item.Bu;
                worksheet.Cell($"H{row}").Value = item.Position;
                worksheet.Cell($"I{row}").Value = item.Resigned_date;
                worksheet.Cell($"J{row}").Value = item.Days_after_action;
                worksheet.Cell($"K{row}").Value = item.Event_type;
                worksheet.Cell($"L{row}").Value = item.Unauthorized;
                worksheet.Cell($"M{row}").Value = item.Download_more_10_files_day;
                worksheet.Cell($"N{row}").Value = item.Employee_resigning_within_one_month;
                if (request.tapData == "DCC")
                {
                    worksheet.Cell($"O{row}").Value = item.Is_bu_dcc;
                    worksheet.Cell($"P{row}").Value = item.Admin_confirm;
                    worksheet.Cell($"Q{row}").Value = item.Admin_confirm_date;
                    worksheet.Cell($"R{row}").Value = item.Admin_confirm_edit; 
                    worksheet.Cell($"S{row}").Value = item.Admin_edit_confirm_date;
                    worksheet.Cell($"T{row}").Value = item.Admin_confirm_comment;
                    worksheet.Cell($"U{row}").Value = item.Admin_confirm_event;
                    
                }
                else
                {
                    worksheet.Cell($"O{row}").Value = item.Admin_confirm;
                    worksheet.Cell($"P{row}").Value = item.Admin_confirm_date;
                    worksheet.Cell($"Q{row}").Value = item.Admin_confirm_edit;
                    worksheet.Cell($"R{row}").Value = item.Admin_edit_confirm_date;
                    worksheet.Cell($"S{row}").Value = item.Admin_confirm_comment;
                    worksheet.Cell($"T{row}").Value = item.Admin_confirm_event;
                }


                row++;
            }

            // 3. Apply adjustments and styling only if new rows were added.
            var newlastRow = row - 1;
            var rowtapData = request.tapData == "DCC" ? "U" : "T";
            var range = worksheet.Range($"A3:{rowtapData}{newlastRow}");

            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.Black;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorderColor = XLColor.Black;


            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            //return Task.FromResult(stream.ToArray());
            return stream.ToArray();
        }
        #endregion


        #region || BuildSavelog on Email ||

        private IQueryable<SaveDUC_DCC_logDto> BuildSaveLogOnEmail(SearchDto request)
        {
            string plantSuffix = "." + request.plant;
            // เตรียม applog ตามเงื่อนไขหลัก
            var applog = _db.Application_Log
                .Where(x => x.Admin_confirm != null
                    && x.App_log == request.tapData && x.Bu_code!.EndsWith(plantSuffix)
                    && (x.Event_type == request.CheckBoxUsual || x.Event_type == request.CheckBoxUnusual));

            // join + search ในฝั่ง DB
            var applogWithJoin = from a in applog
                                 join b in _db.Users_Permission on a.Admin_confirm equals b.emp_no into abgroup
                                 from ab in abgroup.DefaultIfEmpty()
                                 join c in _db.Users_Permission on a.Admin_confirm_edit equals c.emp_no into acgroup
                                 from ac in acgroup.DefaultIfEmpty()
                                 where string.IsNullOrEmpty(request.Search) || (
                                       (a.Group_name ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Username ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Detail ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Action ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Bu ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Position ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Unauthorized ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Download_more_10_files_day ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Employee_resigning_within_one_month ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       ((ab.firstname + " " + ab.lastname) ?? "").ToLower().Contains(request.Search.ToLower().Trim())
                                 )
                                 select new { a, ab, ac };

            // สร้าง DTO
            var query = from x in applogWithJoin
                        select new SaveDUC_DCC_logDto
                        {
                            Id = x.a.Id,
                            Group_name = x.a.Group_name,
                            Username = x.a.Username,
                            Action = x.a.Action,
                            Action_date_time = x.a.Action_date_time,
                            Detail = x.a.Detail,
                            Bu = x.a.Bu,
                            Position = x.a.Position,
                            Resigned_date = x.a.Resigned_date,
                            Days_after_action = x.a.Days_after_action,
                            Event_type = x.a.Event_type,
                            Unauthorized = x.a.Unauthorized,
                            Download_more_10_files_day = x.a.Download_more_10_files_day,
                            Employee_resigning_within_one_month = x.a.Employee_resigning_within_one_month,
                            Is_bu_dcc = x.a.Is_bu_dcc,
                            Admin_confirm = $"{x.ab.firstname} {x.ab.lastname}",
                            Admin_confirm_date = x.a.Admin_confirm_date,
                            Admin_confirm_edit = $"{x.ac.firstname} {x.ac.lastname}",
                            Admin_edit_confirm_date = x.a.Admin_edit_confirm_date,
                            Admin_confirm_comment = x.a.Admin_confirm_comment,
                            Admin_confirm_event = x.a.Admin_confirm_event,
                        };

            // filter วันที่
            if (request.startDate.HasValue)
            {
                var startDate = request.startDate.Value.Date;
                query = query.Where(x => x.Action_date_time >= startDate);
            }

            if (request.endDate.HasValue)
            {
                var endDateExclusive = request.endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Action_date_time < endDateExclusive);
            }

            return query;
        }

        #endregion



        #region || BuildSaveLogQuery || 

        private IQueryable<SaveDUC_DCC_logDto> BuildSaveLogQuery(SearchDto request)
        {
            string plantSuffix = "." + request.plant;
            // เตรียม applog ตามเงื่อนไขหลัก
            var applog = _db.Application_Log
                .Where(x => x.Admin_confirm != null
                    && x.App_log == request.tapData && x.Bu_code!.EndsWith(plantSuffix)
                    && (x.Admin_confirm_event == request.CheckBoxUsual || x.Admin_confirm_event == request.CheckBoxUnusual));

            // join + search ในฝั่ง DB
            var applogWithJoin = from a in applog
                                 join b in _db.Users_Permission on a.Admin_confirm equals b.emp_no into abgroup
                                 from ab in abgroup.DefaultIfEmpty()
                                 join c in _db.Users_Permission on a.Admin_confirm_edit equals c.emp_no into acgroup
                                 from ac in acgroup.DefaultIfEmpty()
                                 where string.IsNullOrEmpty(request.Search) || (
                                       (a.Group_name ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Username ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Detail ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Action ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Bu ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Position ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Unauthorized ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Download_more_10_files_day ?? "").ToLower().Contains(request.Search.ToLower().Trim()) ||
                                       (a.Employee_resigning_within_one_month ?? "").ToLower().Contains(request.Search.ToLower().Trim())  ||
                                       ((ab.firstname + " " + ab.lastname) ?? "").ToLower().Contains(request.Search.ToLower().Trim())  
                                 )
                                 select new { a, ab, ac };

            // สร้าง DTO
            var query = from x in applogWithJoin
                        select new SaveDUC_DCC_logDto
                        {
                            Id = x.a.Id,
                            Group_name = x.a.Group_name,
                            Username = x.a.Username,
                            Action = x.a.Action,
                            Action_date_time = x.a.Action_date_time,
                            Detail = x.a.Detail,
                            Bu = x.a.Bu,
                            Position = x.a.Position,
                            Resigned_date = x.a.Resigned_date,  
                            Days_after_action = x.a.Days_after_action,
                            Event_type = x.a.Event_type,
                            Unauthorized = x.a.Unauthorized,
                            Download_more_10_files_day = x.a.Download_more_10_files_day,
                            Employee_resigning_within_one_month = x.a.Employee_resigning_within_one_month,
                            Is_bu_dcc = x.a.Is_bu_dcc,
                            Admin_confirm = $"{x.ab.firstname} {x.ab.lastname}",
                            Admin_confirm_date = x.a.Admin_confirm_date,
                            Admin_confirm_edit = $"{x.ac.firstname} {x.ac.lastname}",
                            Admin_edit_confirm_date = x.a.Admin_edit_confirm_date,
                            Admin_confirm_comment = x.a.Admin_confirm_comment,
                            Admin_confirm_event = x.a.Admin_confirm_event,
                        };

            // filter วันที่
            if (request.startDate.HasValue)
            {
                var startDate = request.startDate.Value.Date;
                query = query.Where(x => x.Action_date_time >= startDate);
            }

            if (request.endDate.HasValue)
            {
                var endDateExclusive = request.endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Action_date_time < endDateExclusive);
            }

            return query;
        }

        #endregion

        #region || GetSaveLog confirm || 
        public async Task<ResponseDto> GetSaveLogList(SearchDto request)
        {
            try
            {
                IEnumerable<SaveDUC_DCC_logDto> obj = await BuildSaveLogQuery(request).OrderBy(a => a.Event_type == "Unusual Event" ? 0 : 1).ThenByDescending(a => a.Action_date_time).ToListAsync();
                 
                IEnumerable<SaveDUC_DCC_logDto> mappDataList = _mapper.Map<IEnumerable<SaveDUC_DCC_logDto>>(obj);

                _response.Result = mappDataList;

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }


        #endregion

    

        #region || SendEmailAsync 2025-08-19 ||
        public async Task<ResponseDto> SendMailByPlant()
        {
            try
            {
                //DateTime startDate = new DateTime(2025, 8, 1);
                DateTime startDate = DateTime.Today.AddDays(-1);
                DateTime endDate = DateTime.Today;
                 

                // ดึง user + plant name ที่ active และ accept
                var plantUsers = await (
                    from u in _db.Users_Permission
                    join p in _db.bu_plant on u.Plant_Id equals p.Id
                    where u.is_email
                    select new
                    {
                        u.emp_no,
                        u.emp_email,
                        u.username,
                        u.firstname,
                        u.lastname,
                        u.Plant_Id,
                        p.Plant_Name,
                        p.Plant,
                        u.App_Id
                    }
                ).ToListAsync();

                // ดึง plant ทั้งหมดจาก user
                var plants = plantUsers
                    .Select(u => new { u.Plant_Id,u.Plant, u.Plant_Name})
                    .Distinct();

                // ===== ดึง log ทั้งหมดของวันนี้ =====
                var allDucLogs = await _db.Application_Log
                    .Where(x => x.Action_date_time >= startDate && x.Action_date_time < endDate
                             && x.App_log == "DUC" && x.Event_type == "Unusual Event")
                    .ToListAsync();

                var allDccLogs = await _db.Application_Log
                    .Where(x => x.Action_date_time >= startDate && x.Action_date_time < endDate
                             && x.App_log == "DCC" && x.Event_type == "Unusual Event")
                    .ToListAsync();

                foreach (var plant in plants)
                {
                    var ducLogs = allDucLogs.Where(x => x.Bu_code!.Split('.').Last() == plant.Plant).ToList();
                    //var ducUsers = plantUsers.Where(u => u.Plant_Id == plant.Plant_Id).ToList();
                    var ducUsers = plantUsers
                        .Where(u => u.Plant_Id == plant.Plant_Id
                                 && u.App_Id.Split(',').Contains("1"))
                        .ToList();

                    if (ducUsers.Count > 0) // <-- ต่อให้ไม่มี log ก็ส่งได้
                    {
                        Stream? ducStream = null;
                        string fileName = $"reportDUC_{plant.Plant_Name} {startDate:yyyy-MM-dd}.xlsx";

                        string ducBody;
                        if (ducLogs.Count > 0)
                        {
                            var ducDtoList = _mapper.Map<List<Application_logDto>>(ducLogs);
                            ducStream = ExportExcleDucSendMail(ducDtoList);
                            ducBody = BuildEmailBody($"DUC Report - {plant.Plant_Name}", startDate, ducLogs, plant.Plant);
                        }
                        else
                        {
                            ducBody = $"<p>DUC Report - {plant.Plant_Name} dated {startDate:dd MMM yyyy}</p><p><h5>&nbsp;&nbsp; The report log is currently empty. </h5></p>";
                        }

                        string ducEmails = string.Join(";", ducUsers.Select(u => u.emp_email));
                        await SendEmailAsync($"DUC Report - {plant.Plant_Name}", ducBody, ducStream!, fileName, ducEmails);
                    }

                    // ===== DCC =====
                    var dccLogs = allDccLogs.Where(x => x.Bu_code!.Split('.').Last() == plant.Plant).ToList();
                    //var dccUsers = plantUsers.Where(u => u.Plant_Id == plant.Plant_Id).ToList(); 
                    var dccUsers = plantUsers
                    .Where(u => u.Plant_Id == plant.Plant_Id
                             && u.App_Id.Split(',').Contains("2"))
                    .ToList();

                    if (dccUsers.Count > 0)
                    {
                        Stream? dccStream = null;
                        string fileName = $"reportDCC_{plant.Plant_Name} {startDate:yyyy-MM-dd}.xlsx";

                        string dccBody;
                        if (dccLogs.Count > 0)
                        {
                            var dccDtoList = _mapper.Map<List<Application_logDto>>(dccLogs);
                            dccStream = ExportExcleDccSendMail(dccDtoList);
                            dccBody = BuildEmailBody($"DCC Report - {plant.Plant_Name}", startDate, dccLogs, plant.Plant);
                        }
                        else
                        {
                            dccBody = $"<p>DCC Report - {plant.Plant_Name} dated {startDate:dd MMM yyyy}</p><p><h5>&nbsp;&nbsp; The report log is currently empty. </h5></p>";
                        }

                        string dccEmails = string.Join(";", dccUsers.Select(u => u.emp_email));
                        await SendEmailAsync($"DCC Report - {plant.Plant_Name}", dccBody, dccStream!, fileName, dccEmails);
                    }
                }


                _response.Result = startDate.ToString("yyyy-MM-dd");
                _response.Message = _message.SendmailSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }

            return _response;
        }

         

        // ===== ฟังก์ชันสร้าง body HTML =====
        private string BuildEmailBody(string type, DateTime startDate, List<Application_log> logs,string plant)
        {

            //var firstLog = logs.FirstOrDefault();
            var sb = new StringBuilder();
            var appLog = logs.FirstOrDefault()?.App_log;
            var datetime = logs.FirstOrDefault()?.Action_date_time.Date.ToString("yyyy-MM-dd");
            sb.AppendLine("<html><body>");
       
            #region || html ||
            if (logs.Count > 0)
            {
               
                sb.AppendLine(@"<!DOCTYPE html>
                                <html>
                                <head>
                                <style>
                              #customers {
                                   font-family: Arial, Helvetica, sans-serif;
                                   border-collapse: collapse;
                                   width: 100%; 
                                 }
                                 #customers td, #customers th {
                                   border: 1px solid #ddd;
                                   padding: 5px;
                                   font-size: 11px;
                                 }
                                 #customers tr:nth-child(even) {background-color: #f2f2f2;}
                                 #customers tr:hover {background-color: #ddd;}
                                 #customers th {
                                   padding-top: 7px;
                                   padding-bottom: 7px;
                                   text-align: left;
                                   background-color: #04AA6D;
                                   color: white;
                                 }
                                 a.button {
                                  display: inline-block;
                                  padding: 7px 15px;
                                  margin: 5px 8px 3px 0;
                                  font-size: 14px;
                                  font-weight: bold;
                                  text-decoration: none;
                                  color: white;
                                  border-radius: 8px;
                                  transition: background-color 0.3s ease;
                                }

                                a.green {
                                  background-color: #04AA6D;
                                }
                                a.green:hover {
                                  background-color: #038a59;
                                }

                                a.blue {
                                  background-color: #2196F3;
                                }
                                a.blue:hover {
                                  background-color: #1976D2;
                                }

                                a.btn-action {
                                  display: inline-block;
                                  padding: 3px 8px;
                                  font-size: 11px;
                                  text-decoration: none;
                                  color: #fff;
                                  background-color: #ff9800; /* สีส้ม */
                                  border-radius: 5px;
                                  transition: background-color 0.3s ease;
                                }
                                a.btn-action:hover {
                                  background-color: #e68900;
                                }
                                </style>
                                </head>
                                <body>");
                sb.AppendLine($"<h4>{type} dated {startDate:dd MMM yyyy}</h4>");
                sb.AppendLine(@"<table border='0' cellpadding='0' cellspacing='0' role='presentation' style='margin: 5px 8px 3px 0;'>
          <tr>
            <td align='center' bgcolor='#04AA6D' style='border-radius: 8px; background: #04AA6D;'>
              <a href='https://fits/CRUDLogs/applog/reportlog' target='_blank' style='font-size: 14px; font-weight: bold; font-family: Arial, Helvetica, sans-serif; color: #ffffff; text-decoration: none; border-radius: 8px; padding: 7px 15px; border: 1px solid #04AA6D; display: inline-block;'>
                Go to website
              </a>
            </td>
          </tr>
        </table>");
                sb.AppendLine($@"
                    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='margin: 15px 0;'>
                      <tr>
                        <td style='background-color:#e8f4fd; border:1px solid #b6e0fe; border-radius:5px;'>
                          <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                            <tr>
                              <td style='padding: 15px; font-family: Arial, Helvetica, sans-serif; font-size:13px; color:#084298; vertical-align:middle;'>
                                &#9989; I have reviewed the data in the table and confirm that all records are <strong>Usual Events</strong> before saving.
                              </td>
                              <td width='20' style='width:20px;'>&nbsp;</td>
                              <td align='right' style='padding: 15px; vertical-align:middle;' width='100'>
                                
                                <table border='0' cellpadding='0' cellspacing='0' role='presentation'>
                                  <tr>
                                    <td align='center' bgcolor='#2196F3' style='border-radius: 8px; background: #2196F3;'>
                                      <a href='https://fits/CRUDLogs/applog/updateDateOnEmail/{plant}/{appLog}/{datetime}' target='_blank' style='font-size: 13px; font-weight: bold; font-family: Arial, Helvetica, sans-serif; color: #ffffff; text-decoration: none; border-radius: 8px; padding: 5px 12px; border: 1px solid #2196F3; display: inline-block;'>
                                        Confirm
                                      </a>
                                    </td>
                                  </tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                    "); 
                sb.AppendLine(@"<table id='customers'>
                        <tr>
                          <th>No</th>
                          <th>Group Name</th>
                          <th>Action</th>
                          <th>Username</th>
                          <th>Action Date/Time</th>
                          <th>Detail</th>
                          <th>BU</th>
                          <th>Position</th>
                          <th>Event Type</th>
                          <th></th>
                        </tr>");

                int index = 1;
                foreach (var log in logs)
                {
                    string color = log.Event_type == "Usual Event" ? "black" : "red";
                    sb.AppendLine($@"
                <tr>
                  <td>{index++}</td>
                  <td>{log.Group_name}</td>
                  <td>{log.Action}</td>
                  <td>{log.Username}</td>
                  <td>{log.Action_date_time:yyyy-MM-dd HH:mm:ss}</td>
                  <td>{log.Detail}</td>
                  <td>{log.Bu}</td>
                  <td>{log.Position}</td>
                  <td style='color:{color}'>{log.Event_type}</td>
                  <td><a href='https://fits/CRUDLogs/applog/reportlog/{log.Id}/{log.App_log}' class=""btn-action"">view/update</a></td>
                </tr>");
                }

                sb.AppendLine("</table>");
            }
            else
            {
                sb.AppendLine("<p>No data found.</p>");
            }
            #endregion
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        // ===== ฟังก์ชันส่งอีเมล =====
        private async Task SendEmailAsync(string subject, string body, Stream attachStream, string fileName, string toEmails)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_smtpSettings.SenderEmail!, _smtpSettings.SenderName);

            foreach (var email in toEmails.Split(';'))
                message.To.Add(email);

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            if (attachStream != null)
                message.Attachments.Add(new Attachment(attachStream, fileName, ContentTypeConfig.Xlsx));

            using var client = new SmtpClient(_smtpSettings.SmtpServer);
            client.EnableSsl = false;
            client.UseDefaultCredentials = false;
            await client.SendMailAsync(message);
        }

        #region  backup email
        //private async Task SendEmailAsync(string subject, string body, Stream attachStream, string fileName, string toEmails)
        //{
        //    using var message = new MailMessage();
        //    message.From = new MailAddress(_smtpSettings.SenderEmail!, _smtpSettings.SenderName);

        //    foreach (var email in toEmails.Split(';'))
        //    {
        //        if (!string.IsNullOrWhiteSpace(email))
        //            message.To.Add(email);
        //    }

        //    message.Subject = subject;
        //    message.Body = body;
        //    message.IsBodyHtml = true;

        //    if (attachStream != null)
        //        message.Attachments.Add(new Attachment(attachStream, fileName, ContentTypeConfig.Xlsx));

        //    using var client = new SmtpClient(_smtpSettings.SmtpServer, _smtpSettings.SmtpPort)
        //    {
        //        EnableSsl = true,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
        //    };

        //    await client.SendMailAsync(message);
        //}
        #endregion

        #endregion

        public async Task<ResponseDto> SaveAllDateReportlog(SearchAndSaveDto request, string plant, string app_log)
        {
            try
            {
                string plantSuffix = "." + request.plant;
                IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Admin_confirm == null && x.App_log == request.tapData && x.Bu_code!.EndsWith(plantSuffix)&& x.Event_type == "unusual event" && (x.Event_type == request.CheckBoxUsual || x.Event_type == request.CheckBoxUnusual))
                     .OrderByDescending(a => a.Action_date_time);

           

                var user = await _db.Users_Permission.FirstOrDefaultAsync(a => a.emp_no == request.admin_confirm);

                if (request != null && request.Search != null && request.Search.Any())
                {
                    string searchTerm = request.Search.ToLower();

                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchTerm.Trim()) ||
                        x.Group_name!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Username!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Action!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Detail!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Bu!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Position!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Event_type!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Unauthorized!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Download_more_10_files_day!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm.Trim()) ||
                        x.Admin_confirm!.ToLower().Contains(searchTerm.Trim()));

                }

                if (request!.startDate.HasValue)
                {
                    DateTime startDate = request.startDate.Value.Date; // เอาเฉพาะส่วนวันที่ (เวลา 00:00:00)
                    query = query.Where(x => x.Action_date_time >= startDate);
                }

                if (request.endDate.HasValue)
                {
                    DateTime endDateExclusive = request.endDate.Value.Date.AddDays(1); // เอาวันถัดไปตอน 00:00:00
                    query = query.Where(x => x.Action_date_time < endDateExclusive);
                }

                var obj = await query.ToListAsync();
                //var mappList = _mapper.Map<List<Application_logDto>>(obj);
                //_response.Result = mappList;
                int count = 0;
                
                foreach (var item in obj)
                {
                    if (string.IsNullOrEmpty(item.Admin_confirm))
                    {
                        item.Admin_confirm = request.admin_confirm;
                        item.Admin_confirm_comment = "Confirm on email";
                        item.Admin_confirm_date = DateTime.Now;
                        item.Admin_confirm_event = "Usual Event";
                     
                        _db.Application_Log.Update(item);
                   
                   
                        var history = new Historys
                        {
                            emp_no = user!.emp_no,
                            fullname = $"{user.firstname} {user.lastname}",
                            action = item.Action,
                            details = item.Detail,
                            app_logId = item.Id,
                            app_log = item.App_log,
                            comment = item.Admin_confirm_comment,
                            action_datetime = DateTime.Now,
                            processType = "Confirm",
                            bu_code = item.Bu_code,
                            event_type = item.Event_type,
                            admin_confirm_event = item.Admin_confirm_event,
                            username = item.Username,
                            group_name = item.Group_name,
                            master_action_datetime = item.Action_date_time

                        };
                        _db.history.Add(history);
                        count++;
                    }

  
                }

                await _db.SaveChangesAsync();
                _response.Result = $"รวม usual และ unusual ({count})";
                _response.Message = _message.UpdateMessage;

            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        #region || SaveAllDayInEmail ||
        public async Task<ResponseDto> SaveAllDayInEmail(DataAcceptDataAllEamil request,DateTime Datetime,string plant, string app_log)
        {
            try
            {
                DateTime startDate =  Datetime.Date;
                DateTime endDate = startDate.Date.AddDays(1);
                string plantSuffix = "." + plant;
                IQueryable<Application_log> query = _db.Application_Log.Where(x => x.App_log == app_log
                && x.Action_date_time >= startDate && x.Action_date_time < endDate
                && x.Bu_code!.EndsWith(plantSuffix) && x.Event_type == "Unusual Event");

                var user = await _db.Users_Permission.FirstOrDefaultAsync(a => a.emp_no == request.admin_confirm);

                var obj = await query.ToListAsync();
               
                int count = 0;
                foreach (var item in obj)
                {
                    if (string.IsNullOrEmpty(item.Admin_confirm))
                    {
                        item.Admin_confirm = request.admin_confirm;
                        item.Admin_confirm_comment = "Confirm on email";
                        item.Admin_confirm_date = DateTime.Now;
                        item.Admin_confirm_event = "Usual Event";

                        _db.Application_Log.Update(item);

                        var history = new Historys
                        {
                            emp_no = user!.emp_no,
                            fullname = $"{user.firstname} {user.lastname}",
                            action = item.Action,
                            details = item.Detail,
                            app_logId = item.Id,
                            app_log = item.App_log,
                            comment = item.Admin_confirm_comment,
                            action_datetime = DateTime.Now,
                            processType = "Confirm",
                            bu_code = item.Bu_code,
                            event_type = item.Event_type,
                            admin_confirm_event = item.Admin_confirm_event,
                            username = item.Username,
                            group_name = item.Group_name,
                            master_action_datetime = item.Action_date_time

                        };
                        _db.history.Add(history);
 
                        count++;
                    }

                }

                

                await _db.SaveChangesAsync();
                _response.Result = $"รวม usual และ unusual ({count})";
                _response.Message = _message.UpdateMessage;
                 
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }
        #endregion

        public async Task<ResponseDto> GetSaveLogOnEmail(SearchDto request)
        {
            try
            {
                IEnumerable<SaveDUC_DCC_logDto> obj = await BuildSaveLogOnEmail(request).OrderBy(a => a.Event_type == "Unusual Event" ? 0 : 1).ThenByDescending(a => a.Action_date_time).ToListAsync();

                IEnumerable<SaveDUC_DCC_logDto> mappDataList = _mapper.Map<IEnumerable<SaveDUC_DCC_logDto>>(obj);

                _response.Result = mappDataList;

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
