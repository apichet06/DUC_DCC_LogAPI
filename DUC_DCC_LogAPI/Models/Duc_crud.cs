namespace DUC_DCC_LogAPI.Models
{
    public class Duc_crud
    {
        public int Id { get; set; }
        public string? Group_name { get; set; }
        public string? Username { get; set; }
        public string? Action { get; set; }
        public DateTime? Action_datetime { get; set; }
        public string? Detail { get; set; }
        public string? Bu { get; set; }
        public string? Position { get; set; }
        public DateTime? Resigned_date { get; set; }
        public int? Days_after_action { get; set; }
        public string? Event_type { get; set; }
        public string? Unauthorized { get; set; }
        public string? Download_more_10_files_per_day { get; set; }
        public string? Employee_resigning_within_one_month { get; set; }
 
    }
}

 