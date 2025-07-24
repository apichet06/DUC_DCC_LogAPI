using DUC_DCC_LogAPI.Models.Dto.User;

namespace DUC_DCC_LogAPI.Models.Dto.Authen
{
    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserResposeDto? User { get; set; }
        public string? Token { get; set; }
    }
}
