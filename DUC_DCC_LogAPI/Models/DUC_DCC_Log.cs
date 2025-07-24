using System.ComponentModel.DataAnnotations;

namespace DUC_DCC_LogAPI.Models
{
    public class DUC_DCC_Log
    {
      [Key]
      public int Id { get; set; }
      public string? Group_name  { get; set; }
      public string? Username { get; set; }
      public string? Action { get; set; }
      public DateTime? Action_date_time { get; set; }
      public string? Detail { get; set; }
      public string? Bu { get; set; }
      public string? Position { get; set; }
      public DateTime? Resigned_date { get; set; }
      public int? Days_after_action { get; set; }
      public string? Event_type { get; set; }
      public string? Unauthorized { get;   set; }
      public string? Download_more_10_files_day { get; set; }
      public string? Employee_resigning_within_one_month { get; set; }
      public string? Users_action {  get; set; }
      public DateTime? User_action_date { get; set; }
      public string? dcc_duc {  get; set; }
    }
}
