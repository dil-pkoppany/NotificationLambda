using System.ComponentModel.DataAnnotations;

namespace NotificationTablePopulationLambda.Models;

public class CompaniesUsers
{
    [Key]
    public int CompanyUsersId { get; set; }

    public Guid UserId { get; set; }

    public int CompanyId { get; set; }
}
