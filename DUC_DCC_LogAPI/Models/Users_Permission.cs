namespace DUC_DCC_LogAPI.Models
{
    public class Users_Permission
    {
        public int Id { get; set; }
        public string? emp_no { get; set; }
        public string? emp_email { get; set; }
        public string? username { get; set; }
        public string? fristname { get; set; }
        public string? lastname { get; set; }
        public int is_active { get; set; }
        public int is_accept {  get; set; }
        public int is_review { get; set; }
        public string? created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public string? updated_by { get;set; }
        public DateTime? updated_date { get; set; }

    }
}

 