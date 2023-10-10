using System.ComponentModel.DataAnnotations;

namespace NotificationTablePopulationLambda.Models;

public class ChangeLog
{
    [Key]
    public int ChangeLogId { get; set; }

    public Guid ChangeId { get; set; }

    public int? CompanyId { get; set; }
    
    public DateTime Timestamp { get; set; }
}
