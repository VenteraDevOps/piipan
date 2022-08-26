using Piipan.Notifications.Models;

namespace Piipan.Match.Core.Services
{
    public interface INotificationPublish
    {
        Task PublishEmail(EmailModel emailModel);
    }
}
