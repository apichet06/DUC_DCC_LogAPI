namespace DUC_DCC_LogAPI.Models
{
    public class dcc_crud_log
    {
     
        public string? Group_name { get; set; }
        public string? Username { get; set; }
        public string? Action { get; set; }
        public DateTime  Action_datetime { get; set; }
        public string? Detail { get; set; }
        public string? Bu { get; set; }
        public string? Position { get; set; }
        public DateTime? Resigned_date { get; set; }
        public int? Resign_after_action { get; set; }
        public string? Event_type { get; set; }
        public string? Unauthorized { get; set; }
        public string? Is_over_10_file_per_day { get; set; }
        public string? Is_resigned_within_1_month { get; set; }
        public string? Is_not_dcc { get; set; }
    }
}
