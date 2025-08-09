using DUC_DCC_LogAPI.Models.Dto.Duc_DccLog;
using DUC_DCC_LogAPI.Service.Chart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController(IChart chart) : ControllerBase

    {
        private readonly IChart _chart = chart;

        [HttpGet]
        public async Task<IActionResult> GetChart()
        {
            var results = await _chart.GetChartDataAsync();
            return Ok(results);
        }
        [HttpGet("BarChart")]
        public async Task<IActionResult> GetBarChart()
        {
            var results = await _chart.GetChartBarAsync();
            return Ok(results);
        }
    }
}
