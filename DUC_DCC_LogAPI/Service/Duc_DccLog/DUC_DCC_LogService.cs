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

        //        IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Users_accept == null);

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
        //                x.Users_accept!.ToLower().Contains(searchTerm));
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
            IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Users_accept != null && x.App_log == request.tapData);

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
                    x.Users_accept!.ToLower().Contains(searchTerm));
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
                    worksheet.Cell($"P{row}").Value = item.Users_accept;
                    worksheet.Cell($"Q{row}").Value = item.User_accept_date;
                }
                else
                {
                    worksheet.Cell($"O{row}").Value = item.Users_accept;
                    worksheet.Cell($"P{row}").Value = item.User_accept_date;
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
            IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Users_accept == null && x.App_log == request.tapData);

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
                    x.Users_accept!.ToLower().Contains(searchTerm));
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
        //    IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Users_accept == null && x.App_log == request.tapData);

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
        //            x.Users_accept!.ToLower().Contains(searchTerm));
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

                IQueryable<Application_log> query = _db.Application_Log.Where(x=>x.Users_accept == null && x.App_log == request.tapData);

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
                        x.Users_accept!.ToLower().Contains(searchTerm));
                       
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

                 //ExportExcle(mappList);
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
         
        #region || GetSaveLog Accept ||

        public async Task<ResponseDto> GetSaveLogList(SearchDto request)
        {
            try
            {

                IQueryable<Application_log> query = _db.Application_Log.Where(x => x.Users_accept != null && x.App_log == request.tapData);

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
                        x.Users_accept!.ToLower().Contains(searchTerm)
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


                IEnumerable<Application_log> obj = await query.ToListAsync();
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
      

                DateTime startDate = new DateTime(2025, 7, 13);
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
                                  font-size: 10px;
                                }

                                #customers td, #customers th {
                                  border: 1px solid #ddd;
                                  padding: 8px;
                                }

                                #customers tr:nth-child(even) {background-color: #f2f2f2;}

                                #customers tr:hover {background-color: #ddd;}

                                #customers th {
                                  padding-top: 12px;
                                  padding-bottom: 12px;
                                  text-align: left;
                                  background-color: #04AA6D;
                                  color: white;
                                }
                                </style>
                                </head>
                                <body>");

                sb.AppendLine("<h1>DUC Report</h1>");
                sb.AppendLine(@"<table id=""customers"">
                    <tr>
                      <th>NO</th>
                      <th>GROUP NAME</th>
                      <th>USERNAME</th> 
                      <th>ACTION DATE/TIME</th>
                      <th>DETAIL</th>
                      <th>BU</th>
                      <th>POSITION</th>
                      <th>RESIGNED DATE</th>
                      <th>DAYS AFTER ACTION</th>
                      <th>Event type</th> 
                    </tr>");

                int ducIndex = 1;
                foreach (var log in objDucList)
                {
                    sb.AppendLine($@"
                        <tr>
                          <td>{ducIndex++}</td>
                          <td>{log.Group_name}</td>
                          <td>{log.Username}</td>
                          <td>{log.Action_date_time:yyyy-MM-dd HH:mm:ss}</td>
                          <td>{log.Detail}</td>
                          <td>{log.Bu}</td>
                          <td>{log.Position}</td>
                          <td>{(log.Resigned_date.HasValue ? log.Resigned_date.Value.ToString("yyyy-MM-dd") : "")}</td>
                          <td>{(log.Days_after_action)}</td>
                          <td>{log.Event_type}</td>
                        </tr>");
                }

                sb.AppendLine("</table>");

                sb.AppendLine("<h1>DCC Report</h1>");
                sb.AppendLine(@"<table id=""customers"">
                    <tr>
                      <th>NO</th>
                      <th>GROUP NAME</th>
                      <th>USERNAME</th> 
                      <th>ACTION DATE/TIME</th>
                      <th>DETAIL</th>
                      <th>BU</th>
                      <th>POSITION</th>
                      <th>RESIGNED DATE</th>
                      <th>DAYS AFTER ACTION</th>
                      <th>Event type</th> 
                    </tr>");

                                int dccIndex = 1;
                        foreach (var log in objDccList)
                        { 
                                sb.AppendLine($@"
                                <tr>
                                  <td>{dccIndex++}</td>
                                  <td>{log.Group_name}</td>
                                  <td>{log.Username}</td>
                                  <td>{log.Action_date_time:yyyy-MM-dd HH:mm:ss}</td>
                                  <td>{log.Detail}</td>
                                  <td>{log.Bu}</td>
                                  <td>{log.Position}</td>
                                  <td>{(log.Resigned_date.HasValue ? log.Resigned_date.Value.ToString("yyyy-MM-dd") : "")}</td>
                                  <td>{log.Days_after_action}</td>
                                  <td>{log.Event_type}</td>
                                </tr>");
                         
                }
            

                sb.AppendLine("</table>");
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

        #region || UpdateList ||
        public async Task<ResponseDto> UpdateList(CheckedDataDto request)
        {
            try
            {
                 

                if (request?.Id == null || !request.Id.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "No items to update.";
                    return _response;
                }

            
                var logsToUpdate = await _db.Application_Log
                                            .Where(log => request.Id.Contains(log.Id))
                                            .ToListAsync();

                if (!logsToUpdate.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "No matching records found.";
                    return _response;
                }
                 
                 
                foreach (var log in logsToUpdate)
                {
           
                    log.Users_accept = "Apichet";
                    log.User_accept_date = DateTime.Now;
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
            range.Style.Border.InsideBorder =XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorderColor = XLColor.Black;
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
            workbook.Save();
        }

        #endregion
    }
}
