using AutoMapper;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dtos;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models; 
using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Models.Dto.SaveDuc_DccLog; 
using Microsoft.EntityFrameworkCore;
 

using Microsoft.Extensions.Options; 
using System.Net; 
using System.Net.Mail;
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


        public DUC_DCC_LogService(AppDbContext dbContext, IMapper mapper, IOptions<EmailSettings> emailSettings, IWebHostEnvironment env , HttpClient httpClient )
        {
            _db = dbContext;
            _mapper = mapper;
            _message = new MessageDto();
            _response = new ResponseDto();
            _smtpSettings = emailSettings.Value;
            _env = env;
            _httpClient = httpClient;
        }

        public async Task<ResponseDto> GetDataDUC(DUC_DCC_Log request)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<DUC_DCC_Log>>(_url);
                var dataInsert = data!.Select(a => new DUC_DCC_Log
                {
                    Group_name = a.Group_name,


                });
                await _db.DUC_DCC_Log.AddRangeAsync(dataInsert);
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
                var data = await _httpClient.GetFromJsonAsync<List<DUC_DCC_Log>>(_url);
                var dataInsert = data!.Select(a => new DUC_DCC_Log
                {
                    Group_name = a.Group_name,


                });
                await _db.DUC_DCC_Log.AddRangeAsync(dataInsert);
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
        public async Task<ResponseDto<FileDownloadDto>> ExportExcel(SearchDto request)
        {
            try
            {

                IQueryable<DUC_DCC_Log> query = _db.DUC_DCC_Log.Where(x => x.Users_action == null);

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
                        x.Users_action!.ToLower().Contains(searchTerm));
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
                var mappList = _mapper.Map<List<DUC_DCC_logDto>>(obj);

                var filePath = Path.Combine(_env.ContentRootPath, "Files", "DUC_DCC_LogService.xlsx");

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Report");
                int row = 3;
                int autoId = 1;
                 

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

                    row++;
                }
                worksheet.Columns().AdjustToContents();
                var newlastRow = row - 1;
                var range = worksheet.Range($"A3:N{newlastRow}");

                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.OutsideBorderColor = XLColor.Black;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorderColor = XLColor.Black;

        
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    byte[] content = stream.ToArray();

                    return new ResponseDto<FileDownloadDto>()
                    {
                        IsSuccess = true,
                        Message = "Success",
                        Data = new FileDownloadDto()
                        {
                            Content = content,
                            FileName = $"รายการงาน.xlsx",
                            ContentType = ContentTypeConfig.Xlsx,
                        },
                        Status = HttpStatusCode.OK
                    };
                }


            }
            catch (Exception ex)
            {
                // 5. แก้ไขการ Return ค่าใน Catch Block
                return new ResponseDto<FileDownloadDto>()
                {
                    IsSuccess = false,
                    Message = "An error occurred: " + ex.Message,
                    Data = null,
                    Status = HttpStatusCode.InternalServerError
                };
            }
        }
        #endregion

        #region || ExportExcelAccept || 
        public async Task<byte[]> ExportExcelAccept(SearchDto request)
        {
            IQueryable<DUC_DCC_Log> query = _db.DUC_DCC_Log.Where(x => x.Users_action != null);

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
                    x.Users_action!.ToLower().Contains(searchTerm));
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
            var mappList = _mapper.Map<List<DUC_DCC_logDto>>(obj);

            var filePath = Path.Combine(_env.ContentRootPath, "Files", "reportDUC_DCC_Accept.xlsx");

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
                worksheet.Cell($"O{row}").Value = item.Users_action;
                worksheet.Cell($"P{row}").Value = item.User_action_date;

                row++;
            }

            // 3. Apply adjustments and styling only if new rows were added.
            var newlastRow = row - 1;
            var range = worksheet.Range($"A3:P{newlastRow}");

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
            IQueryable<DUC_DCC_Log> query = _db.DUC_DCC_Log.Where(x => x.Users_action == null);

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
                    x.Users_action!.ToLower().Contains(searchTerm));
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
            var mappList = _mapper.Map<List<DUC_DCC_logDto>>(obj);

            var filePath = Path.Combine(_env.ContentRootPath, "Files", "reportDUC_DCC.xlsx");

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
            //return Task.FromResult(stream.ToArray());
            return stream.ToArray();
        }
        #endregion

        #region || ExportReportLog ||

        private void ExportReportLog(List<DUC_DCC_logDto> dataList)
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

                IQueryable<DUC_DCC_Log> query = _db.DUC_DCC_Log.Where(x=>x.Users_action == null);

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
                        x.Users_action!.ToLower().Contains(searchTerm));
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
                var mappList = _mapper.Map<List<DUC_DCC_logDto>>(obj);

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
         
        #region || GetSaveLogList ||

        public async Task<ResponseDto> GetSaveLogList(SearchDto request)
        {
            try
            {

                IQueryable<DUC_DCC_Log> query = _db.DUC_DCC_Log.Where(x => x.Users_action != null);

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
                        x.Users_action!.ToLower().Contains(searchTerm)
                 );
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


                IEnumerable<DUC_DCC_Log> obj = await query.ToListAsync();
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
  
                var body = $@"

                                                <!DOCTYPE html>
                                                <html>
                                                <head>
                                                <style>
                                                #customers {{
                                                  font-family: Arial, Helvetica, sans-serif;
                                                  border-collapse: collapse;
                                                  width: 100%;
                                                }}

                                                #customers td, #customers th {{
                                                  border: 1px solid #ddd;
                                                  padding: 8px;
                                                }}

                                                #customers tr:nth-child(even){{background-color: #f2f2f2;}}

                                                #customers tr:hover {{background-color: #ddd;}}

                                                #customers th {{
                                                  padding-top: 12px;
                                                  padding-bottom: 12px;
                                                  text-align: left;
                                                  background-color: #04AA6D;
                                                  color: white;
                                                }}
                                                </style>
                                                </head>
                                                <body>

                                                <h1>DCC & DUC Report</h1>

                                                <table id=""customers"">
                                                  <tr>
                                                    <th>GROUP NAME</th>
                                                    <th>USERNAME</th>
                                                    <th>ACTION</th>
                                                    <th>ACTION DATE/TIME</th>
                                                    <th>DETAIL</th>
                                                    <th>BU</th>
                                                    <th>POSITION</th>
                                                    <th>RESIGNED DATE</th>
                                                    <th>DAYS AFTER ACTION</th>
                                                    <th>Event type</th>
                                                    <th>Unauthorized</th>
                                                    <th>Download more 10 files per day</th>
                                                    <th>Employee resigning within one month</th> 
                                                  </tr>
                                                  <tr>
                                                    <td>CB_CISCO_OTBU</td>
                                                    <td>Maria Anders</td>
                                                    <td>tw@twengspecialist.com</td>
                                                    <td>Download</td>
                                                    <td>2025-01-04 08:51:56</td>   <td>CB_CISCO_OTBU/Supplier_Cisco_OTBU/TW_Engineering/ACE packaging.zip</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Usual Event</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                  </tr>
                                                  <tr>
                                                    <td>CB_CISCO_OTBU</td>
                                                    <td>Maria Anders</td>
                                                    <td>tw@twengspecialist.com</td>
                                                    <td>Download</td>
                                                    <td>2025-01-04 08:51:56</td>   <td>CB_CISCO_OTBU/Supplier_Cisco_OTBU/TW_Engineering/ACE packaging.zip</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Usual Event</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                  </tr>
                                                  <tr>
                                                    <td>CB_CISCO_OTBU</td>
                                                    <td>Maria Anders</td>
                                                    <td>tw@twengspecialist.com</td>
                                                    <td>Download</td>
                                                    <td>2025-01-04 08:51:56</td>   <td>CB_CISCO_OTBU/Supplier_Cisco_OTBU/TW_Engineering/ACE packaging.zip</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Usual Event</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                    <td>Germany</td>
                                                  </tr> 
                                                </table> 
                                                </body>
                                                </html> 

                                                ";
                string filePath = Path.Combine(_env.ContentRootPath, "Files", "report.xlsx");
                var message = new MailMessage();
                message.From = new MailAddress(_smtpSettings.SenderEmail!, _smtpSettings.SenderName);
                message.To.Add("apichets06@fabrinet.co.th");
                message.Subject = "AutoMail";
                message.Body = body;
                message.IsBodyHtml = true;
                Attachment attachment = new Attachment(filePath);
                message.Attachments.Add(attachment);

                using (var client = new SmtpClient(_smtpSettings.SmtpServer, _smtpSettings.SmtpPort))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false; // สำคัญ
                    client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendMailAsync(message);
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

            
                var logsToUpdate = await _db.DUC_DCC_Log
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
           
                    log.Users_action = "Apichet";
                    log.User_action_date = DateTime.Now;
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

        #region || Export Excel ||
        private void ExportExcle(List<DUC_DCC_logDto> dataList)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Files", "report.xlsx");

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

       

        #endregion
    }
}
