namespace DUC_DCC_LogAPI.Models.Dto.History
{
    public class HistoryDto
    { 
        public string? emp_no { get; set; }
        public string? fullname { get; set; }
        public string? action { get; set; } 
        public string? details { get; set; }
        public DateTime action_datetime { get; set; }
    }
}
