using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models; 
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Models.Dto.SaveDuc_DccLog; 
using DUC_DCC_LogAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;
 

using Microsoft.Extensions.Options; 
using System.Net; 
using System.Net.Mail;
using System.Text;
using static DUC_DCC_LogAPI.Constant.Constants;

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



        #region || ExportExcelAccept || 
        public async Task<byte[]> ExportExcelAccept(SearchDto request)
        {
            IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Admin_confirm != null && x.App_log == request.tapData);

            if (query.Count() > 0) {
               
                _response.Message = _message.Not_found;

            }
          
            

            if (request != null && request.Search != null && request.Search.Any())
            {
                string searchTerm = request.Search.ToLower();

                query = query.Where(x =>
                    x.Group_name!.ToLower().Contains(searchTerm) ||
                    x.Username!.ToLower().Contains(searchTerm) ||
                    x.Action!.ToLower().Contains(searchTerm) ||
                    x.Detail!.ToLower().Contains(searchTerm) ||
                    x.Bu!.ToLower().Contains(searchTerm) ||
                    x.Position!.ToLower().Contains(searchTerm) ||
                    x.Event_type!.ToLower().Contains(searchTerm) ||
                    x.Unauthorized!.ToLower().Contains(searchTerm) ||
                    x.Download_more_10_files_day!.ToLower().Contains(searchTerm) ||
                    x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm) ||
                    x.Admin_confirm!.ToLower().Contains(searchTerm));
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
            var fileName = request.tapData == "DUC" ? "reportDUC_Accept.xlsx" : "reportDCC_Accept.xlsx";
            var filePath = Path.Combine(_env.ContentRootPath, "Files", fileName );

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
                }
                else
                {
                    worksheet.Cell($"O{row}").Value = item.Admin_confirm;
                    worksheet.Cell($"P{row}").Value = item.Admin_confirm_date;
                }


                    row++;
            }

            // 3. Apply adjustments and styling only if new rows were added.
            var newlastRow = row - 1;
            var rowtapData = request.tapData == "DCC" ? "Q" : "P";
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

        #region || ExportExcelLog ||

        public async Task<byte[]> ExportExcelLog(SearchDto request)
        {
            IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Admin_confirm == null && x.App_log == request.tapData);

            //if (query == null)
            //      _response.Message = _message.Not_found;
           

            if (request != null && request.Search != null && request.Search.Any())
            {
                string searchTerm = request.Search.ToLower();

                query = query.Where(x =>
                    x.Group_name!.ToLower().Contains(searchTerm) ||
                    x.Username!.ToLower().Contains(searchTerm) ||
                    x.Action!.ToLower().Contains(searchTerm) ||
                    x.Detail!.ToLower().Contains(searchTerm) ||
                    x.Bu!.ToLower().Contains(searchTerm) ||
                    x.Position!.ToLower().Contains(searchTerm) ||
                    x.Event_type!.ToLower().Contains(searchTerm) ||
                    x.Unauthorized!.ToLower().Contains(searchTerm) ||
                    x.Download_more_10_files_day!.ToLower().Contains(searchTerm) ||
                    x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm) ||
                    x.Admin_confirm!.ToLower().Contains(searchTerm));
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

        #region || GetList ||
        public async Task<ResponseDto>GetList(SearchDto request)
        {
            try
            {

                IQueryable<Application_log> query = _db.Application_Log.Where(x=>x.Admin_confirm == null && x.App_log == request.tapData).OrderByDescending(a=>a.Action_date_time);

                if (request != null && request.Search != null && request.Search.Any())
                {
                    string searchTerm = request.Search.ToLower();

                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchTerm) ||
                        x.Group_name!.ToLower().Contains(searchTerm) ||
                        x.Username!.ToLower().Contains(searchTerm) ||
                        x.Action!.ToLower().Contains(searchTerm) ||
                        x.Detail!.ToLower().Contains(searchTerm) ||
                        x.Bu!.ToLower().Contains(searchTerm) ||
                        x.Position!.ToLower().Contains(searchTerm) ||
                        x.Event_type!.ToLower().Contains(searchTerm) ||
                        x.Unauthorized!.ToLower().Contains(searchTerm) ||
                        x.Download_more_10_files_day!.ToLower().Contains(searchTerm) ||
                        x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm) ||
                        x.Admin_confirm!.ToLower().Contains(searchTerm));
                       
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
         
        #region || GetSaveLog confirm ||

        public async Task<ResponseDto> GetSaveLogList(SearchDto request)
        {
            try
            {

                IQueryable<Application_log> applog = _db.Application_Log.Where(x => x.Admin_confirm != null && x.App_log == request.tapData);

                var query = from a in applog
                            join b in _db.Users_Permission on a.Admin_confirm equals b.emp_no into abgroup
                            from ab in abgroup.DefaultIfEmpty()
                            join c in _db.Users_Permission on a.Admin_confirm_edit equals c.emp_no into acgroup
                            from ac in acgroup.DefaultIfEmpty()
                          select new SaveDUC_DCC_logDto
                          {
                              Id = a.Id,
                              Group_name = a.Group_name,
                              Username = a.Username,
                              Action = a.Action,
                              Action_date_time = a.Action_date_time,
                              Detail=a.Detail,
                              Bu =a.Bu,
                              Position = a.Position,
                              Resigned_date = a.Resigned_date,
                              Days_after_action = a.Days_after_action,
                              Event_type = a.Event_type,
                              Unauthorized = a.Unauthorized,
                              Download_more_10_files_day =a.Download_more_10_files_day,
                              Employee_resigning_within_one_month = a.Employee_resigning_within_one_month,
                              Is_bu_dcc = a.Is_bu_dcc,
                              Admin_confirm =  $"{ab.firstname} {ab.lastname}",
                              Admin_confirm_date = a.Admin_confirm_date,
                              Admin_confirm_edit = $"{ac.firstname} {ac.lastname}",
                              Admin_edit_confirm_date = a.Admin_edit_confirm_date,
                              Admin_confirm_comment = a.Admin_confirm_comment,
                              Admin_confirm_event = a.Admin_confirm_event,
                          };

                if (request != null && request.Search != null && request.Search.Any())
                {
                    string searchTerm = request.Search.ToLower();

                    query = query.Where(x =>
                        x.Group_name!.ToLower().Contains(searchTerm) ||
                        x.Username!.ToLower().Contains(searchTerm) ||
                        x.Detail!.ToLower().Contains(searchTerm) ||
                        x.Action!.ToLower().Contains(searchTerm) ||
                        x.Bu!.ToLower().Contains(searchTerm) ||
                        x.Position!.ToLower().Contains(searchTerm) ||
                        x.Event_type!.ToLower().Contains(searchTerm) ||
                        x.Unauthorized!.ToLower().Contains(searchTerm) ||
                        x.Download_more_10_files_day!.ToLower().Contains(searchTerm) ||
                        x.Employee_resigning_within_one_month!.ToLower().Contains(searchTerm) ||
                        x.Admin_confirm!.ToLower().Contains(searchTerm)
                 );
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

                 

                IEnumerable<SaveDUC_DCC_logDto> obj = await query.OrderByDescending(a=>a.Action_date_time).ToListAsync();
               
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

        #region || SendMail ||
        public async Task<ResponseDto> SendMail()
        {
            try
            {
                 
                DateTime startDate = new DateTime(2025, 8, 8);
                //DateTime startDate = DateTime.Today;
                DateTime endDate = startDate.AddDays(1);

                List<Application_log> objDucList = await _db.Application_Log
                    .Where(x => x.Action_date_time >= startDate && x.Action_date_time < endDate && x.App_log == "DUC")
                    .ToListAsync();
                var mappDucList = _mapper.Map<List<Application_logDto>>(objDucList);

                ExportExcleDucSendMail(mappDucList);

                List<Application_log> objDccList = await _db.Application_Log
                 .Where(x => x.Action_date_time >= startDate && x.Action_date_time < endDate && x.App_log == "DCC")
                 .ToListAsync();
                var mappDccList = _mapper.Map<List<Application_logDto>>(objDccList);

                ExportExcleDccSendMail(mappDccList);


                var sb = new StringBuilder();

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
                sb.AppendLine("<h4>DUC Report</h4>");
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
                      <th>Link Web</th>  
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
                          <td><a href=""https://fits/CRUDLogs/applog/report-logId/{log.Id}/{log.App_log}"">Click</a></td>
                        </tr>");
                }

                sb.AppendLine("</table>");
                }
                else
                {
                    sb.AppendLine("<h5>&nbsp;&nbsp;The report log is currently empty.</h5>");
                }

                    sb.AppendLine("<h4>DCC Report</h4>");
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
                      <th>Link Web</th>  
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
                                  <td><a href=""https://fits/CRUDLogs/applog/report-logId/{log.Id}/{log.App_log}"">Click</a></td>
                                </tr>");

                } 
                sb.AppendLine("</table>");
                }
                else
                {
                    sb.AppendLine("<h5>&nbsp;&nbsp; The report log is currently empty. </h5>");
                }
                    sb.AppendLine("</body></html>");

                string body = sb.ToString();

                string filePathDuc = Path.Combine(_env.ContentRootPath, "Files", "reportDUCSendMail.xlsx");
                string filePathDcc = Path.Combine(_env.ContentRootPath, "Files", "reportDCCSendMail.xlsx");
                var message = new MailMessage();
                message.From = new MailAddress(_smtpSettings.SenderEmail!, _smtpSettings.SenderName);
                message.To.Add("apichets@fabrinet.co.th");
                message.Subject = "AutoMail";
                message.Body = body;
                message.IsBodyHtml = true;
                message.Attachments.Add(new Attachment(filePathDuc));
                message.Attachments.Add(new Attachment(filePathDcc));


                using (var client = new SmtpClient(_smtpSettings.SmtpServer))
                {
                    client.EnableSsl = false; // ถ้าเปิดใช้ Port เช่น 587 ,25 ให้ EnableSsl เป็น true
                    //client.UseDefaultCredentials = true; // สำคัญ
                    client.UseDefaultCredentials = false;
                    //client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password); //ถ้าเป็นระบบภายในไม่ต้องมีการยืนยันตัวตน รหัสผ่าน
                    await client.SendMailAsync(message);
                }

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

        #region || EditDataAccept แก้ไขหลังจากบันทึกไปแล้ว || 
        public async Task<ResponseDto> EditDataAccept(EditDataAcceptDto request, int id)
        {
            try
            {
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


        #region || UpdateList ||
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

                }
                 
                
                await _db.SaveChangesAsync();

                _response.IsSuccess = true;
                _response.Message = $"{logsToUpdate.Count} records updated successfully.";


            }
            catch (Exception ex) {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;
            }
            return _response;
        }

        #endregion

        #region || Export Excel send Mail ||
        private  void  ExportExcleDucSendMail(List<Application_logDto> dataList)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Files", "reportDUCSendMail.xlsx");

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet("Report");
            int row = 3;
            int autoId = 1;

            // remove Data
            int lastRow = worksheet.LastRowUsed()!.RowNumber(); 
            if(lastRow>= row)
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
            workbook.Save();
        }

        private void ExportExcleDccSendMail(List<Application_logDto> dataList)
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
            workbook.Save();
        }

        #endregion
    }
}
