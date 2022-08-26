using Piipan.Notifications.Models;

namespace Piipan.Notifications.Api
{
    public interface INotificationApi
    {
        Task<bool> CreateMessageFromTemplate(EmailTemplateInput emailTemplateInput);
    }
}
