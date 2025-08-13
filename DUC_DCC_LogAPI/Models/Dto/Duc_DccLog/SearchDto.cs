namespace DUC_DCC_LogAPI.Models.Dto.Duc_DccLog
{
    public class SearchDto
    {
        public string? Search {  get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; } 
        public string? tapData { get; set; }
        public string? CheckBoxkUsual { get; set; }
        public string? CheckBoxkUnusual { get; set; }
         
    }
}
