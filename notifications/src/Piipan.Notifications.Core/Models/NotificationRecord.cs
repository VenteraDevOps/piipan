using Piipan.Notification.Common.Models;

namespace Piipan.Notifications.Models
{
    public class NotificationRecord
    {
        public MatchModel MatchRecord { get; set; }
        public EmailToModel EmailToRecord { get; set; }
        public EmailToModel EmailToRecordMS { get; set; }
        public DispositionModel MatchResEvent { get; set; }
        public NotificationRecord()
        {
        }
    }
}