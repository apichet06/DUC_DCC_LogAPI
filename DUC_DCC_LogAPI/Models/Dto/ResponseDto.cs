using DUC_DCC_LogAPI.Models.Dto.User;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;

namespace DUC_DCC_LogAPI.Models.Dto
{
    public partial class ResponseDto
    {
        public object? Result { get; set; }
        public bool? IsSuccess { get; set; } = true;
        public string Message { get; set; } = ""; 
    }




    public partial class ResponseDto<T>
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        [JsonIgnore]
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    }

   

    public class ResponseAuthen
    {
        public bool? IsSuccess { get; set; } = true;
        public object? Result { get; set; }
        public string? token { get; set; }
        public string? Message { get; set; }
    
    }


    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserResposeDto? User { get; set; }
        public string? Token { get; set; }
    }



    public class MessageDto
    {
        public string InsertMessage { get; set; } = "บันทึกข้อมูลสำเร็จ!";
        public string UpdateMessage { get; set; } = "อับเดทข้อมูลสำเร็จ!";
        public string DeleteMessage { get; set; } = "ลบข้อมูลสำเร็จ!";
        public string Not_found { get; set; } = "ไม่พบข้อมูล!";
        public string already_exists { get; set; } = "Already exists. : ";
        public string an_error_occurred { get; set; } = "เกิดข้อผิดพลาด 500 : ";
        public string Approved_status { get; set; } = "ดำเนินการสำเร็จ!";
        public string changPassword { get; set; } = "เปลี่ยนรหัสผ่านสำเร็จ!";
        public string Active { get; set; } = "ข้อมูลถูกใช้งานอยู่ : ";
        public string DistplaySuccess { get; set; } = "แสดงข้อมูลสำเร็จ";
        public string LoginNotFound { get; set; } = "Username หรือ Password ไม่ถูกต้อง";
        public string LoginSuccess { get; set; } = "เข้าสู่ระบบสำเร็จ";
        public string SendmailSuccess { get; set; } = "SendMail Success";
        
    }
}
