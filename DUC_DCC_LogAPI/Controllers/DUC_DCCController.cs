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

        [HttpGet("SaveReportLog")]
        public async Task<IActionResult> GetSavelog([FromQuery] SearchDto request)
        {
            var results = await _DCC_Log.GetSaveLogList(request);
            return Ok(results);
        }


        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CheckedDataDto request)
        {
            var results = await _DCC_Log.UpdateList(request);
            return Ok(results);
        }

        [HttpGet("SendMail")]
        public async Task<IActionResult> GetSendMail()
        {
            var results = await _DCC_Log.SendMail();
            return Ok(results);
        }

        [HttpGet("ExportExcel")]
        public async Task<IActionResult> GetExportExcel([FromQuery] SearchDto request)
        {
            var fileData = await _DCC_Log.ExportExcel(request); 

            return Ok(fileData);

        }
        [HttpGet("ExportExcelLog")]
        public async Task<IActionResult> GetExportExcelLog([FromQuery] SearchDto request)
        {
            try
            {
                var excelBytes = await _DCC_Log.ExportExcelLog(request);  
                return File(
                    excelBytes,
                    ContentTypeConfig.Xlsx,
                    "reportLog.xlsx"
                );
            }
            catch (Exception ex) {
                return StatusCode(500, $"Failed to export Excel file: {ex.Message}");
            }
             
        }

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

    }

}
