using NotificationTablePopulationLambda.Data;
using NotificationTablePopulationLambda.Models;
using NotificationTablePopulationLambda.Services;
using NotificationTablePopulationLambda.UnitTests.Helpers;
using NSubstitute;

namespace NotificationTablePopulationLambda.UnitTests;

public class NotificationServiceTests
{
    private readonly IRepository<ChangeLogNotification> changeLogNotificationRepositorySub
        = Substitute.For<IRepository<ChangeLogNotification>>();

    private readonly IRepository<ChangeLog> changeLogRepositorySub
        = Substitute.For<IRepository<ChangeLog>>();

    private readonly IRepository<CompaniesUsers> companiesUsersRepositorySub
        = Substitute.For<IRepository<CompaniesUsers>>();

    private readonly IRepository<AspNetUserRoles> userRolesRepositorySub
        = Substitute.For<IRepository<AspNetUserRoles>>();

    private readonly IRepository<Notification> notificationRepositorySub
        = Substitute.For<IRepository<Notification>>();

    [Fact]
    public void PopulateNotificationTable_ChangeLogNotificationTableNotEmpty_UsersAreNotified()
    {
        // Arrange
        var newChangeId = Guid.NewGuid();

        List<ChangeLogNotification> changeLogNotifications = new()
        {
            new ChangeLogNotification()
            {
                ChangeId = newChangeId
            }
        };

        changeLogNotificationRepositorySub
            .GetAll()
            .Returns(changeLogNotifications.AsQueryable());

        var companyToNotifyId = RandomIntGenerator.GeneratePositive();

        List<ChangeLog> changeLogs = new()
        {
            new ChangeLog()
            {
                ChangeLogId = RandomIntGenerator.GeneratePositive(),
                ChangeId = newChangeId,
                CompanyId = companyToNotifyId
            }
        };

        changeLogRepositorySub
            .GetAll()
            .Returns(changeLogs.AsQueryable());

        var userToNotifyId = Guid.NewGuid();

        List<CompaniesUsers> companiesUsers = new()
        {
            new CompaniesUsers()
            {
                CompanyId = companyToNotifyId,
                UserId = userToNotifyId
            }
        };

        companiesUsersRepositorySub
            .GetAll()
            .Returns(companiesUsers.AsQueryable());

        List<AspNetUserRoles> userRoles = new()
        {
            new AspNetUserRoles()
            {
                UserId = userToNotifyId,
                RoleId = QueryHelpers.UserRolesToNotify[0].Guid
            }
        };

        userRolesRepositorySub
            .GetAll()
            .Returns(userRoles.AsQueryable());

        List<Notification> notifications = new();

        notificationRepositorySub
            .GetAll()
            .Returns(notifications.AsQueryable());

        NotificationService service = new(
            changeLogNotificationRepositorySub,
            changeLogRepositorySub,
            companiesUsersRepositorySub,
            notificationRepositorySub,
            userRolesRepositorySub);

        // Act
        service.PopulateNotificationTable();

        // Assert
        notificationRepositorySub
            .Received()
            .InsertRange(Arg.Is<IEnumerable<Notification>>(x => x.First().CompanyId == companyToNotifyId));

        notificationRepositorySub
            .Received()
            .InsertRange(Arg.Is<IEnumerable<Notification>>(x => x.First().UserId == userToNotifyId));

        changeLogNotificationRepositorySub
            .Received()
            .Delete(changeLogNotifications[0]);
    }

    [Fact]
    public void PopulateNotificationTable_UserIsAlreadyNotified_NoNewNotificationsCreated()
    {
        // Arrange
        var newChangeId = Guid.NewGuid();

        List<ChangeLogNotification> changeLogNotifications = new()
        {
            new ChangeLogNotification()
            {
                ChangeId = newChangeId
            }
        };

        changeLogNotificationRepositorySub
            .GetAll()
            .Returns(changeLogNotifications.AsQueryable());

        var companyToNotifyId = RandomIntGenerator.GeneratePositive();

        List<ChangeLog> changeLogs = new()
        {
            new ChangeLog()
            {
                ChangeLogId = RandomIntGenerator.GeneratePositive(),
                ChangeId = newChangeId,
                CompanyId = companyToNotifyId
            }
        };

        changeLogRepositorySub
            .GetAll()
            .Returns(changeLogs.AsQueryable());

        var userToNotifyId = Guid.NewGuid();

        List<CompaniesUsers> companiesUsers = new()
        {
            new CompaniesUsers()
            {
                CompanyId = companyToNotifyId,
                UserId = userToNotifyId
            }
        };

        companiesUsersRepositorySub
            .GetAll()
            .Returns(companiesUsers.AsQueryable());

        List<AspNetUserRoles> userRoles = new()
        {
            new AspNetUserRoles()
            {
                UserId = userToNotifyId,
                RoleId = QueryHelpers.UserRolesToNotify[0].Guid
            }
        };

        userRolesRepositorySub
            .GetAll()
            .Returns(userRoles.AsQueryable());

        List<Notification> notifications = new()
        {
            new Notification()
            {
                CompanyId = companyToNotifyId,
                UserId = userToNotifyId
            }
        };

        notificationRepositorySub
            .GetAll()
            .Returns(notifications.AsQueryable());

        NotificationService service = new(
            changeLogNotificationRepositorySub,
            changeLogRepositorySub,
            companiesUsersRepositorySub,
            notificationRepositorySub,
            userRolesRepositorySub);

        // Act
        service.PopulateNotificationTable();

        // Assert
        notificationRepositorySub
            .DidNotReceive()
            .InsertRange(Arg.Is<IEnumerable<Notification>>(x => x.First().CompanyId == companyToNotifyId));

        notificationRepositorySub
            .DidNotReceive()
            .InsertRange(Arg.Is<IEnumerable<Notification>>(x => x.First().UserId == userToNotifyId));

        changeLogNotificationRepositorySub
            .Received()
            .Delete(changeLogNotifications[0]);
    }

    [Fact]
    public void PopulateNotificationTable_ChangeLogNotificationTableIsEmpty_NoError()
    {
        // Arrange
        List<ChangeLogNotification> changeLogNotifications = new();

        changeLogNotificationRepositorySub
            .GetAll()
            .Returns(changeLogNotifications.AsQueryable());

        var service = new NotificationService(
            changeLogNotificationRepositorySub,
            changeLogRepositorySub,
            companiesUsersRepositorySub,
            notificationRepositorySub,
            userRolesRepositorySub);

        // Act
        service.PopulateNotificationTable();

        // Assert (skipped on purpose, checking if no exceptions occurred)
    }
}