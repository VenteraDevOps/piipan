using System.Threading.Tasks;
using Piipan.Notifications.Models;

namespace Piipan.Match.Core.Services
{
    public interface INotificationPublish
    {
        Task PublishEmail(EmailModel metrics);
    }
}
