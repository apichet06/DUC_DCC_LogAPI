using AutoMapper;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Service.Duc_DccLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static DUC_DCC_LogAPI.Constant.Constants;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DUC_DCCController(IDUC_DCC_Log duclog) : ControllerBase
    {

        private readonly IDUC_DCC_Log _DCC_Log = duclog;

        [HttpGet("ReportLog")]
        public async Task<IActionResult> GetList([FromQuery] SearchDto request)
        {
            var results = await _DCC_Log.GetList(request);
            return Ok(results);

        }

        [HttpGet("ReportLog/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _DCC_Log.GetById(id); 
            return Ok(result);
        }

        [HttpGet("SaveReportLog")]
        public async Task<IActionResult> GetSavelog([FromQuery] SearchDto request)
        {
            var results = await _DCC_Log.GetSaveLogList(request);
            return Ok(results);
        }

        [HttpGet("SavelogOnEmail")]
        public async Task<IActionResult> GetSavelogOnEmail([FromQuery] SearchDto request)
        {
            var results = await _DCC_Log.GetSaveLogOnEmail(request);
            return Ok(results);
        }

        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] CheckedDataDto request)
        {
            var results = await _DCC_Log.UpdateList(request);
            return Ok(results);
        }


        [HttpPut("AcceptById/{id:int}")]
        public async Task<IActionResult> putAcceptById([FromBody] DataAcceptByIdDto request, int id)
        {
            var results = await _DCC_Log.DataAcceptById(request, id);
            return Ok(results);
        }

        [HttpPut("EditAccept/{id:int}")]
        public async Task<IActionResult> putEdit([FromBody] EditDataAcceptDto request,int id)
        {
            var results = await _DCC_Log.EditDataAccept(request,id);
            return Ok(results);
        }

        [HttpGet("SendMailSchedule")]
        public async Task<IActionResult> GetSendMail()
        {
            var results = await _DCC_Log.SendMail();
            return Ok(results);
        }

        [HttpGet("SendMailByPlant")]
        public async Task<IActionResult> SendMailByPlant()
        {
            return Ok( await _DCC_Log.SendMailByPlant());
        }

        [HttpPut("SaveAllDayInEmail/{plant}/{app_log}/{Datetime}")]
        public async Task<IActionResult> SaveAllDayInEmail([FromBody] DataAcceptDataAllEamil request, DateTime Datetime, string plant, string app_log)
        {
            return Ok(await _DCC_Log.SaveAllDayInEmail(request, Datetime, plant, app_log));
        }


        //[HttpGet("ExportExcelDUCBas64")]
        //public async Task<IActionResult> GetExportExcel([FromQuery] SearchDto request)
        //{
        //    var fileData = await _DCC_Log.ExportExcel(request); 

        //    return Ok(fileData);

        //}

        [HttpGet("ExportExcelLog")]
        public async Task<IActionResult> GetExportExcelDucLog([FromQuery] SearchDto request)
        {
            try
            {
                var excelBytes = await _DCC_Log.ExportExcelLog(request);  
                return File(
                    excelBytes,
                    ContentTypeConfig.Xlsx,
                    "DUC_reportLog.xlsx"
                );
            }
            catch (Exception ex) {
                return StatusCode(500, $"Failed to export Excel file: {ex.Message}");
            }
             
        }

        //[HttpGet("ExportExcelDCCLog")]
        //public async Task<IActionResult> GetExportExcelDccLog([FromQuery] SearchDto request)
        //{
        //    try
        //    {
        //        var excelBytes = await _DCC_Log.ExportExcelDccLog(request);
        //        return File(
        //            excelBytes,
        //            ContentTypeConfig.Xlsx,
        //            "DCC_reportLog.xlsx"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Failed to export Excel file: {ex.Message}");
        //    }

        //}


        [HttpGet("ExportExcelLogAccept")]
        public async Task<IActionResult> GetExportExcelAccept([FromQuery] SearchDto request)
        {
            try
            {
                var excelBytes = await _DCC_Log.ExportExcelAccept(request);
                return File(
                   excelBytes,
                   ContentTypeConfig.Xlsx,
                   "reportLogAccept.xlsx"
               );
            }
            catch (Exception ex) {

                return StatusCode(500, $"Failed to export Excel file: {ex.Message}");
            }
        }

        [HttpGet("CountReportLog")]
        public async Task<IActionResult> GetCountReportLog()
        {
            return Ok(await _DCC_Log.GetCountReportLog());
        }

        [HttpGet("CountAuditLog")]
        public async Task<IActionResult> GetCountAuditLog()
        {
            return Ok(await _DCC_Log.GetCountAuditLog());
        }
    }

}
