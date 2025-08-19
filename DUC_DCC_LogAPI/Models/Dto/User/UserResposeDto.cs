namespace DUC_DCC_LogAPI.Models.Dto.User
{
    public class UserResposeDto
    {
        public int Id { get; set; }
        public int Plant_Id { get; set; } 
        public string? emp_no { get; set; }
        public string? emp_email { get; set; }
        public string? username { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public int is_active { get; set; }
        public int is_accept { get; set; }
        public int is_review { get; set; }
        public string? App_Id { get; set; }
        public string? created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public string? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
        public string? Status { get; set; }
        public string? Plant { get; set; }
        public string? Plant_Name { get; set; }

    }
}
