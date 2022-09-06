using Piipan.Notification.Common.Models;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Builders
{
    public interface INotificationRecordBuilder
    {
        INotificationRecordBuilder SetMatchModel(MatchModel matchModel);
        INotificationRecordBuilder SetDispositionModel(DispositionModel dispositionModel);
        INotificationRecordBuilder SetEmailToModel(string emailTo, string emailCC = null, string emailBCC = null);
        INotificationRecordBuilder SetEmailMatchingStateModel(string emailTo, string emailCC = null, string emailBCC = null);
        NotificationRecord GetRecord();
    }
}
