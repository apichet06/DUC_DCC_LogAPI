using DUC_DCC_LogAPI.Models.Dto.Authen;
using DUC_DCC_LogAPI.Service.AuthenService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUC_DCC_LogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthenController(IAuthenService authen) : ControllerBase
    {
        private readonly IAuthenService _authen = authen;
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AuthenDto request)
        {
            return Ok(await _authen.Authen(request));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Logins([FromForm] AuthenDto authen)
        {

            var authResult = await _authen.AuthenticateUserAsync(authen);
            // นำ Token ที่ได้จาก Service มาใส่ใน Cookie
            Response.Cookies.Append("authToken", authResult.token!, new CookieOptions
            {
                HttpOnly = false,
                Secure = true, // ควรเป็น true เมื่อ Deploy จริง (ใช้ HTTPS)
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(6)
            });
            //return Redirect("http://localhost:5173/reportlog");
            //return Redirect("http://dccduclog/reportlog");

            #region || backup ||
            //var userJson = System.Text.Json.JsonSerializer.Serialize(authResult.User);
            //Response.Cookies.Append("user", userJson, new CookieOptions
            //{
            //    HttpOnly = true, // JavaScript อ่านได้
            //    Secure = true,
            //    SameSite = SameSiteMode.Strict,
            //    Expires = DateTime.UtcNow.AddMinutes(60)
            //}); 
            #endregion


            #region || backup ||
            return Ok(new
            {
                IsSuccess = true,
                Token = authResult.token,
                url = "http://dccduclog/reportlog",
                Message = authResult.Message
            });
            #endregion
        }



    }
}
