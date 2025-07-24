namespace DUC_DCC_LogAPI.Models.Dto.User
{
    public class UserResposeDto
    {
        public int Id { get; set; }
        public string? emp_no { get; set; }
        public string? emp_email { get; set; }
        public string? username { get; set; }
        public string? fristname { get; set; }
        public string? lastname { get; set; }
        public int is_active { get; set; }
        public int is_accept { get; set; }
        public int is_review { get; set; } 
    }
}
