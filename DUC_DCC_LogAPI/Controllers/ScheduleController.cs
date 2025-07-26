using DUC_DCC_LogAPI.Service.DccService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController(IScheduleService scheduleService) : ControllerBase
    {
        private readonly IScheduleService _scheduleService  = scheduleService;
        [HttpGet("SchedulDuc")]
        public async Task<IActionResult> GetScheduleDUC() {
            return Ok(await _scheduleService.ImportDccAsync());
        }

        [HttpGet("SchedulDcc")]
        public async Task<IActionResult> GetSchedulDCC()
        {
            return Ok(await _scheduleService.ImportDucAsync());
        }

    }
}
