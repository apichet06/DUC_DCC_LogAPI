using DUC_DCC_LogAPI.Models.Dto.History;
using DUC_DCC_LogAPI.Service.History;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController(IHistoryService history) : ControllerBase
    {
        private readonly IHistoryService _history = history;

        [HttpGet]
        public async Task<IActionResult> Gethistory([FromQuery] HistoryDto request) {

            return Ok(await _history.GetHistoryList(request));
        
        }
     

    }
}
