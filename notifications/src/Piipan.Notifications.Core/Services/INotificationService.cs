﻿using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    public interface INotificationService
    {
        Task<bool> PublishNotificationOnMatchCreation(NotificationRecord notificationRecord);
        Task<bool> PublishNotificationOnMatchResEventsUpdate(NotificationRecord notificationRecord);

    }
}
