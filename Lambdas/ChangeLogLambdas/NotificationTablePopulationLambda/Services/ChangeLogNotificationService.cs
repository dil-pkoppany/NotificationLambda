using Amazon.Lambda.Core;
using NotificationTablePopulationLambda.Data;
using NotificationTablePopulationLambda.Models;

namespace NotificationTablePopulationLambda.Services
{
    public interface IChangeLogNotificationService
    {
        void RemoveWithNullCompanyId();
    }

    public class ChangeLogNotificationService : IChangeLogNotificationService
    {
        private readonly IRepository<ChangeLogNotification> _changeLogNotificationRepository;
        private readonly IRepository<ChangeLog> _changeLogRepository;

        public ChangeLogNotificationService(
            IRepository<ChangeLogNotification> changeLogNotificationRepository,
            IRepository<ChangeLog> changeLogRepository)
        {
            _changeLogNotificationRepository = changeLogNotificationRepository;
            _changeLogRepository = changeLogRepository;
        }

        /// <summary>
        /// Removes <c>ChangeLogNotification</c> table entries that have corresponding <c>ChangeLog</c>
        /// table entry with null <c>CompanyId</c>
        /// </summary>
        public void RemoveWithNullCompanyId()
        {
            var recordsToDelete = from cn in _changeLogNotificationRepository.GetAll()
                                  join cl in _changeLogRepository.GetAll() on cn.ChangeId equals cl.ChangeId
                                  where cl.CompanyId == null
                                  select cn;

            if (recordsToDelete.Any())
            {
                var deletedRecordsCount = recordsToDelete.Count();
                _changeLogNotificationRepository.DeleteRange(recordsToDelete);
                LambdaLogger.Log($"Found and removed {deletedRecordsCount} changeLogNotification entries with null CompanyId ChangeLog table entry");
            }
        }
    }
}
