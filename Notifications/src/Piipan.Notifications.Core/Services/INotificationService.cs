using Piipan.Notifications.Models;

namespace Piipan.Notifications.Services
{
    public interface INotificationService
    {
        Task<bool> CreateMessageFromTemplate(EmailTemplateInput emailTemplateInput);

    }
}
