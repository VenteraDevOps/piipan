using Microsoft.Extensions.Logging;

using Piipan.Notification.Common;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationPublish _notificationPublish;
        public ILogger<NotificationService> _logger;
        private readonly IViewRenderService _viewRenderService;

        public NotificationService(INotificationPublish notificationPublish, IViewRenderService viewRenderService, ILogger<NotificationService> logger)
        {
            _notificationPublish = notificationPublish;
            _viewRenderService = viewRenderService;
            _logger = logger;
        }

        public async Task<bool> PublishNotificationOnMatchCreation(NotificationRecord notificationRecord)
        {
            //Send Notofication to Iniatiating State and Matching State.  Passing Iniatiating state and Matching state templete respectively.
            var emailbodyIS = _viewRenderService.GenerateMessageContent("MatchEmailIS.cshtml", notificationRecord.MatchRecord)?.Result.ToString();
            var emailsubjectIS = _viewRenderService.GenerateMessageContent("MatchEmailIS_Sub.cshtml", notificationRecord.MatchRecord)?.Result.ToString();

            var emailbodyMS = _viewRenderService.GenerateMessageContent("MatchEmailMS.cshtml", notificationRecord.MatchRecord)?.Result.ToString();
            var emailsubjectMS = _viewRenderService.GenerateMessageContent("MatchEmailMS_Sub.cshtml", notificationRecord.MatchRecord)?.Result.ToString();

            var resultIS = await PublishNotifications(notificationRecord.EmailToRecord, emailbodyIS, emailsubjectIS);
            var resultMS = await PublishNotifications(notificationRecord.EmailToRecordMS, emailbodyMS, emailsubjectMS);
            return resultMS || resultIS;
        }
        public async Task<bool> PublishNotificationOnMatchResEventsUpdate(NotificationRecord notificationRecord)
        {
            //Send Notofication to the other State. Todo  Refactor after getting the finialized template file .

            var emailbodyIS = _viewRenderService.GenerateMessageContent("DispositionEmail.cshtml", notificationRecord.MatchResEvent)?.Result.ToString();
            var emailsubjectIS = _viewRenderService.GenerateMessageContent("DispositionEmail_Sub.cshtml", notificationRecord.MatchResEvent)?.Result.ToString();
            return await PublishNotifications(notificationRecord.EmailToRecord, emailbodyIS, emailsubjectIS);

        }
        public async Task<bool> PublishNotifications(EmailToModel emailToRecord, string emailbody, string emailsubject)
        {
            try
            {
                var emailModel = new EmailModel
                {
                    ToList = emailToRecord?.EmailTo.Split(',').ToList(),
                    ToCCList = emailToRecord?.EmailCcTo?.Split(',').ToList(),
                    ToBCCList = emailToRecord?.EmailBccTo?.Split(',').ToList(),
                    Body = emailbody,
                    Subject = emailsubject,
                    From = string.Empty,
                };
                // Publish to the queue.
                await _notificationPublish.PublishEmail(emailModel);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, String.Format("Failed to publish Notification for the state {0} and email subject {1}", emailToRecord?.EmailTo, emailsubject));
                return false;
            }
        }

    }
}