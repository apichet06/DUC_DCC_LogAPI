using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Service.UserPermissionService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPermissionController(IUserPermissionService users) : ControllerBase
    {
        private readonly IUserPermissionService _userService = users;


        [HttpGet("byEmp/{emp_no}")]
        public async Task<IActionResult> GetUserById(string emp_no)
        {
            return Ok(await _userService.GetUserById(emp_no));
        }


        [HttpGet("users/")]
        public async Task<IActionResult> GetUserList([FromQuery] Users_Permission request)
        {
            return Ok(await _userService.GetUserList(request));
        }
        [HttpPost]
        public async Task<IActionResult> PostUserPremiess([FromBody] Users_Permission request)
        {
            return Ok(await _userService.CreateUserPermiss(request));
        }
        [HttpPut("{Id}")]
        public async Task<IActionResult> PutUserPermiss([FromBody] Users_Permission request,int Id)
        {
            return Ok(await _userService.UpdateUserPermiss(request,Id));
        }
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeletePermiss(int Id)
        {
            return Ok(await _userService.DeleteUserPermiss(Id));
        }

        [HttpGet("appName")]
        public async Task<IActionResult> GetAppName()
        {
            return Ok(await _userService.GetAppName());
        }

        [HttpGet("buPlant")]
        public async Task<IActionResult> GetbuPlant()
        {
            return Ok(await _userService.GetBuPlant());
        }
    }

}
