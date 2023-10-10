using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using NotificationTablePopulationLambda.Data;
using NotificationTablePopulationLambda.Models;
using System.Data;

namespace NotificationTablePopulationLambda.Services;

public interface INotificationService
{
    void PopulateNotificationTable();
}

public class NotificationService : INotificationService
{
    private readonly IRepository<ChangeLogNotification> _changeLogNotificationRepository;
    private readonly IRepository<ChangeLog> _changeLogRepository;
    private readonly IRepository<CompaniesUsers> _companiesUsersRepository;
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<AspNetUserRoles> _userRolesRepository;

    private static readonly IEnumerable<Guid> RolesToNotifyIds = QueryHelpers.UserRolesToNotify
        .Select(x => x.Guid);

    public NotificationService(
        IRepository<ChangeLogNotification> changeLogNotificationRepository,
        IRepository<ChangeLog> changeLogRepository,
        IRepository<CompaniesUsers> companiesUsersRepository,
        IRepository<Notification> notificationRepository,
        IRepository<AspNetUserRoles> userRolesRepository)
    {
        _changeLogNotificationRepository = changeLogNotificationRepository;
        _changeLogRepository = changeLogRepository;
        _companiesUsersRepository = companiesUsersRepository;
        _notificationRepository = notificationRepository;
        _userRolesRepository = userRolesRepository;
    }

    /// <summary>
    /// Populates <c>Notification</c> table with new entries
    /// for appeared changes in <c>ChangeLogNotification</c> table.
    /// </summary>
    public void PopulateNotificationTable()
    {
        var changeLogNotifications = _changeLogNotificationRepository
            .GetAll();

        PublishNotificationsCountMetric(changeLogNotifications.Count());

        bool newChangesHappened = changeLogNotifications.Any();

        if (!newChangesHappened)
        {
            LambdaLogger.Log("No entries in ChangeLogNotification table");
            return;
        }

        List<Guid> newChangesIds = changeLogNotifications
            .Select(x => x.ChangeId)
            .ToList();

        var changeLogs = _changeLogRepository
            .GetAll();

        // TODO: Any limit per lambda call? (to avoid timeout)
        foreach (var newChangeId in newChangesIds)
        {
            NotifyUsers(newChangeId);
            PublishNotificationsLatencyMetric(newChangeId, changeLogs);
            DeleteChangeLogNotificationEntry(newChangeId, changeLogNotifications);
        }

        LambdaLogger.Log($"Successfully created required notifications for {newChangesIds.Count} change events.");
    }

    private void NotifyUsers(Guid newChangeId)
    {
        int? companyToNotifyId = _changeLogRepository
            .GetAll()
            .Where(x => x.ChangeId == newChangeId && x.CompanyId != null)
            .FirstOrDefault()
            ?.CompanyId;

        if (companyToNotifyId == null)
        {
            return;
        }

        // From all users in the company select only those IDs that have specific roles
        IQueryable<Guid> usersToNotifyIds = (from cu in _companiesUsersRepository.GetAll()
                                             join ur in _userRolesRepository.GetAll() on cu.UserId equals ur.UserId
                                             where cu.CompanyId == companyToNotifyId.Value && RolesToNotifyIds.Contains(ur.RoleId)
                                             select cu.UserId).Distinct();

        // Select users IDs that don't need notification (additional query for code readability)
        IQueryable<Guid> usersThatAlreadyNotifiedIds = from n in _notificationRepository.GetAll()
                                                       where usersToNotifyIds.Contains(n.UserId) && n.CompanyId == companyToNotifyId.Value
                                                       select n.UserId;

        IEnumerable<Guid> usersThatAreNotNotifiedIds = usersToNotifyIds
            .Except(usersThatAlreadyNotifiedIds);

        List<Notification> newNotifications = new();
        var notificationTime = DateTime.UtcNow;

        foreach (var userId in usersThatAreNotNotifiedIds)
        {
            Notification newNotification = new()
            {
                TimeStamp = notificationTime,
                UserId = userId,
                CompanyId = companyToNotifyId.Value
            };
            newNotifications.Add(newNotification);
        }

        if (newNotifications.Count > 0)
        {
            _notificationRepository.InsertRange(newNotifications);
            LambdaLogger.Log($"Notified {newNotifications.Count} users about change event {newChangeId}");
        }
    }

    private void DeleteChangeLogNotificationEntry(Guid changeId, IQueryable<ChangeLogNotification> notifications)
    {
        var changeLogNotificationItemToDelete = notifications
            .Where(x => x.ChangeId == changeId)
            .Single();

        _changeLogNotificationRepository
            .Delete(changeLogNotificationItemToDelete);
    }

    /// <summary>
    /// Number of records in the changelog notification table at the start of the Lambda
    /// </summary>
    private static void PublishNotificationsCountMetric(int notificationsCount)
    {
        using var logger = new MetricsLogger();
        logger.SetNamespace("NotificationLambda");

        var dimensionSet = new DimensionSet();
        dimensionSet.AddDimension("Service", "aggregator");
        dimensionSet.AddDimension("Source", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
        dimensionSet.AddDimension("Version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString());
        logger.SetDimensions(dimensionSet);

        logger.PutMetric("NotificationsCount",
              notificationsCount, 
              Unit.COUNT, 
              StorageResolution.STANDARD
            );

        // This value is not submitted to CloudWatch Metrics but is searchable by CloudWatch Logs Insights. This is useful for contextual and potentially high-cardinality data that is not appropriate for CloudWatch Metrics dimensions.
        logger.PutProperty("RequestId", Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Individually measure the time spent between timestamp of the changelog event and the timestamp of removing the record from. the changelog notification table
    /// </summary>
    /// <param name="latencySeconds">Seconds between notification generation and deletion</param>
    /// <param name="changeId">Related Change Id</param>
    private static void PublishNotificationsLatencyMetric(Guid changeId, IQueryable<ChangeLog> changeLogs)
    {
        var changeLog = changeLogs
            .Where(x => x.ChangeId == changeId);

        if (!changeLog.Any())
        {
            LambdaLogger.Log($"No changelog entry found for changeId {changeId} could not publish latency metric");
            return;
        }

        using var logger = new MetricsLogger();
        logger.SetNamespace("NotificationLambda");

        var dimensionSet = new DimensionSet();
        dimensionSet.AddDimension("ChangeId", changeLog.First().ChangeId.ToString());
        dimensionSet.AddDimension("Service", "aggregator");
        dimensionSet.AddDimension("Source", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
        dimensionSet.AddDimension("Version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString());
        logger.SetDimensions(dimensionSet);

        logger.PutMetric("NotificationsCount",
              (DateTime.UtcNow - changeLog.First().Timestamp).TotalSeconds,
              Unit.SECONDS,
              StorageResolution.STANDARD
            );

        // This value is not submitted to CloudWatch Metrics but is searchable by CloudWatch Logs Insights.
        // This is useful for contextual and potentially high-cardinality data that is not appropriate for CloudWatch Metrics dimensions.
        logger.PutProperty("RequestId", changeLog.First().ChangeId.ToString());
    }
}
