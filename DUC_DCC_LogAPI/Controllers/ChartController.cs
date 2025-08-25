using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Service.Chart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class ChartController(IChart chart) : ControllerBase

    {
        private readonly IChart _chart = chart;

        [HttpGet("{Year}/{plant}")]
        public async Task<IActionResult> GetChart(int Year, string plant)
        {
            var results = await _chart.GetChartDataAsync(Year,plant);
            return Ok(results);
        }
        [HttpGet("BarChart/{Year}/{plant}")]
        public async Task<IActionResult> GetBarChart(int Year, string plant)
        {
            var results = await _chart.GetChartBarAsync(Year, plant);
            return Ok(results);
        }
    }
}
