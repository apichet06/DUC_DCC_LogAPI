using System.ComponentModel.DataAnnotations;

namespace DUC_DCC_LogAPI.Models
{
    public class Application_log
    { 
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
      public string? Is_bu_dcc {  get; set; }
      public string? Users_accept {  get; set; }
      public DateTime? User_accept_date { get; set; }
      public string? App_log {  get; set; }
      public DateTime? Upload_datetime { get; set; }
      public string? User_accept_edit { get; set; }
      public DateTime? User_accept_edit_date {  get; set; }
      public string? User_accept_comment { get; set; }
      public string? Status_accept { get; set; }
    }
}
