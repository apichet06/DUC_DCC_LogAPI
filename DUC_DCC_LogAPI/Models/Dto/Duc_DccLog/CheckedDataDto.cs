namespace DUC_DCC_LogAPI.Models.Dto.Duc_DccLog
{
    public class CheckedDataDto
    {
        public List<int>? Id { get; set; } 
        public  string? Admin_confirm { get; set; }
        public string? Admin_confirm_comment { get; set; }
        public string? Admin_confirm_event { get; set; }
    }

    public class EditDataAcceptDto
    { 
        public string? Admin_confirm_edit { get; set; }
        public string? Admin_confirm_comment { get; set; } 
        public string? Admin_confirm_event { get; set; }
    }

    public class DataAcceptByIdDto
    { 
        public string? Admin_confirm { get; set; }
        public string? Admin_confirm_comment { get; set; }
        public string? Admin_confirm_event { get; set; }
    }
}

 