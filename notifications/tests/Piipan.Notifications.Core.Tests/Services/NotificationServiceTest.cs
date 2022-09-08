using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Notification.Common;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    public class NotificationServiceTest
    {

        [Fact]
        public async Task PublishMessageFromTemplateTest()
        {
            var notificationRecord = new NotificationRecord()
            {
                MatchRecord = new MatchModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = "http://test.com",
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@nac.com"
                }
            };
            string subIS = "IS: Test Subject";
            string bodyIS = "IS: Test Body";
            string subMS = "MS: Test Subject";
            string bodyMS = "MS: Test Subject";

            var emailModelIS = new EmailModel
            {

                ToList = "ea@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyIS,
                Subject = subIS,
                From = "",
            };
            var emailModelMS = new EmailModel
            {

                ToList = "eb@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyMS,
                Subject = subMS,
                From = "",
            };

            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmailIS.cshtml", notificationRecord.MatchRecord))
                .Returns(Task.FromResult(bodyIS));
            viewRenderService
               .Setup(m => m.GenerateMessageContent("MatchEmailIS_Sub.cshtml", notificationRecord.MatchRecord))
               .Returns(Task.FromResult(subIS));

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmailMS_Sub.cshtml", notificationRecord.MatchRecord))
                .Returns(Task.FromResult(subMS));

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmailMS.cshtml", notificationRecord.MatchRecord))
                .Returns(Task.FromResult(bodyMS));

            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));

            var logger = new Mock<ILogger<NotificationService>>();

            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            var ret = await service.PublishNotificationOnMatchCreation(notificationRecord);
            // Assert
            Assert.True(ret);

            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelIS.Body && p.Subject == emailModelIS.Subject && p.ToList[0] == "ea@nac.com")), Times.Once);
            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelMS.Body && p.Subject == emailModelMS.Subject && p.ToList[0] == "eb@nac.com")), Times.Once);

        }
        [Fact]
        public async Task PublishMessageFromTemplateTest_Exception()
        {
            var notificationRecord = new NotificationRecord { };
            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();
            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));
            var logger = new Mock<ILogger<NotificationService>>();
            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            // Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => service.PublishNotificationOnMatchCreation(notificationRecord));
        }
    }
}
