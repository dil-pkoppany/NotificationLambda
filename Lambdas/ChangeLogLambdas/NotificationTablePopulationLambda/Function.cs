using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using NotificationTablePopulationLambda.Data;
using NotificationTablePopulationLambda.Models;
using NotificationTablePopulationLambda.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace NotificationTablePopulationLambda;

public class Function
{
    private IManageCO2DbContext _context;

    private INotificationService _notificationService;
    private IChangeLogNotificationService _changeLogNotificationService;

    private async Task ConfigureServices()
    {
        // Initialize DB Context
        var connectionString = await DbConnectionStringProvider.Get();
        var options = new DbContextOptionsBuilder<ManageCO2DbContext>()
            .UseSqlServer(connectionString)
            .Options;

        _context = new ManageCO2DbContext(options);

        // Initialize repositories
        var changeLogNotificationRepository = new Repository<ChangeLogNotification>(_context);
        var changeLogRepository = new Repository<ChangeLog>(_context);
        var companiesUsersRepository = new Repository<CompaniesUsers>(_context);
        var notificationRepository = new Repository<Notification>(_context);
        var userRolesRepository = new Repository<AspNetUserRoles>(_context);

        // Initialize services (if much more services will be added then Dependency Injection should be implemented)
        _notificationService = new NotificationService(
            changeLogNotificationRepository,
            changeLogRepository,
            companiesUsersRepository,
            notificationRepository,
            userRolesRepository);

        _changeLogNotificationService = new ChangeLogNotificationService(
            changeLogNotificationRepository,
            changeLogRepository);
    }

    public async Task FunctionHandler()
    {

        await ConfigureServices();

        LambdaLogger.Log($"Routine started on {DateTime.UtcNow} UTC");

        try
        {
            _changeLogNotificationService.RemoveWithNullCompanyId();
            _notificationService.PopulateNotificationTable();
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Routine ended with an unhandled exception. Message: {ex.Message}");
            throw;
        }
        finally
        {
            _context.Dispose();
        }
    }
}