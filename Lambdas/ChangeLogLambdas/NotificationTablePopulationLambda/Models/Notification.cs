using System.ComponentModel.DataAnnotations;

namespace NotificationTablePopulationLambda.Models;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public string? NotificationMessage { get; set; }

    public DateTime TimeStamp {  get; set; }

    public Guid UserId { get; set; }

    public int CompanyId { get; set; }
}
