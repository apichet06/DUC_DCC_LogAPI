namespace DUC_DCC_LogAPI.Models.Dto.Chart
{
    public class BarCharDto
    {
        public int month { get; set; }
        public string? Name { get; set; }
        public int CountData { get; set; } = 0;
        public string? App_log {  get; set; }
        public int Year { get; set; }
    }
}
