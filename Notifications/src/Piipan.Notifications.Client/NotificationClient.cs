using Piipan.Notifications.Api;
using Piipan.Notifications.Models;
using Piipan.Shared.Http;

namespace Piipan.Notifications.Client
{

    public class NotificationClient : INotificationApi
    {
        private readonly IAuthorizedApiClient<NotificationClient> _apiClient;

        /// <summary>
        /// Intializes a new instance of StatesClient
        /// </summary>
        /// <param name="apiClient">an API client instance scoped to StatesClient</param>
        public NotificationClient(IAuthorizedApiClient<NotificationClient> apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Sends a GET request to the /states endpoint using the API client's configured base URL.
        /// </summary>
        /// <returns></returns>

        public Task<bool> CreateMessageFromTemplate(EmailTemplateInput emailTemplateInput)
        {
            return Task.FromResult(true);
        }
    }
}
