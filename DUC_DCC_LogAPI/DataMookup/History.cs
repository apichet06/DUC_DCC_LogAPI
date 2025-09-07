using System;
using System.Collections.Generic;

namespace DUC_DCC_LogAPI.DataMookup;

public partial class History
{
    public int Id { get; set; }

    public string? emp_no { get; set; }

    public string? fullname { get; set; }

    public string? action { get; set; }

    public string? admin_confirm_evnet { get; set; }

    public int? app_logId { get; set; }

    public string? app_log { get; set; }

    public string? bu_code { get; set; }

    public string? group_name { get; set; }

    public string? username { get; set; }

    public string? details { get; set; }

    public DateTime? action_datetime { get; set; }

    public string? comment { get; set; }

    public string? event_type { get; set; }

    public string? processType { get; set; }
}
