namespace DUC_DCC_LogAPI.Models
{
    public class Historys
    {
        public int Id { get; set; }
        public string? emp_no { get; set; }
        public string? fullname { get; set; }
        public string? action { get; set; }
        public int app_logId { get; set; }
        public string? app_log {  get; set; }
        public string? details { get; set; }
        public DateTime action_datetime { get; set; }
        public string? comment { get; set; }
        public string? processType { get; set; }
    }
}
 