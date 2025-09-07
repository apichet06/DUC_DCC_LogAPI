namespace DUC_DCC_LogAPI.Models.Dto.History
{
    public class HistoryDto
    {
        public string? Search { get; set; }
        public string? App_log { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
    }
}
