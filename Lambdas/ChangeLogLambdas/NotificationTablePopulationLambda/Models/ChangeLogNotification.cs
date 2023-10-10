using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NotificationTablePopulationLambda.Models;

public class ChangeLogNotification
{
    [Key]
    public Guid ChangeId { get; set; }
}
