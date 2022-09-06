using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Notification.Common;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    public class NotificationPublishTest
    {
        [Fact]
        public async void NotificationPublish_Sucess()
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
        public async void NotificationPublish_Failed()
        {
            Environment.SetEnvironmentVariable("EventGridEndPoint", null);
            Environment.SetEnvironmentVariable("EventGridKeyString", null);
            var logger = new Mock<ILogger<EmailModel>>();
            var notificationPublish = new NotificationPublish(logger.Object);
            Mock<EventGridPublisherClient> publisherClientMock = new Mock<EventGridPublisherClient>();

            notificationPublish._client = publisherClientMock.Object;
            string emails = "test@test.com,test1@test.com";
            var emailModel = new EmailModel
            {

                ToList = emails.Split(',').ToList(),
                ToCCList = emails.Split(',').ToList(),
                ToBCCList = emails.Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };
            await notificationPublish.PublishEmail(emailModel);
            publisherClientMock.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(s => s.EventType == "Publish to the queue"), default));


            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EventGridNotificationEndPoint and EventGridNotificationKeyString environment variables are not set")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

    }

}
