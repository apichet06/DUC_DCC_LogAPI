using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Service.Chart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChartController(IChart chart) : ControllerBase

    {
        private readonly IChart _chart = chart;

        [HttpGet("{Year}")]
        public async Task<IActionResult> GetChart(int Year)
        {
            var results = await _chart.GetChartDataAsync(Year);
            return Ok(results);
        }
        [HttpGet("BarChart/{Year}")]
        public async Task<IActionResult> GetBarChart(int Year)
        {
            var results = await _chart.GetChartBarAsync(Year);
            return Ok(results);
        }
    }
}
