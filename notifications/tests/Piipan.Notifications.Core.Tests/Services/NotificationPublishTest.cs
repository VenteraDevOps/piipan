﻿using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Core.Services;
using Piipan.Notifications.Models;
using Piipan.Notifications.Services;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    public class NotificationPublishTest
    {
        [Fact]
        public async void NotificationPublish_Sucess()
        {
            //Environment.SetEnvironmentVariable("EventGridEndPoint", "http://someendpoint.gov");
            //Environment.SetEnvironmentVariable("EventGridKeyString", "example");
            //var logger = Mock.Of<ILogger<EmailModel>>();
            //var notificationPublish = new NotificationPublish(logger);
            //Mock<EventGridPublisherClient> publisherClientMock = new Mock<EventGridPublisherClient>();
            //notificationPublish._client = publisherClientMock.Object;
            //string emails = "test@test.com,test1@test.com";
            //var emailModel = new EmailModel
            //{

            //    ToList = emails.Split(',').ToList(),
            //    ToCCList = emails.Split(',').ToList(),
            //    ToBCCList = emails.Split(',').ToList(),
            //    Body = "Body of the Email",
            //    Subject = "Subject of the Email",
            //    From = "noreply@test.com",
            //};
            //await notificationPublish.PublishEmail(emailModel);
            //publisherClientMock.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(s => s.EventType == "Publish to the queue"), default));

            var emailTemplateInput = new EmailTemplateInput { EmailTo = "Test@test.com", Topic = "UPDATE_MATCH_RES" };
            var notificatioPublish = new Mock<INotificationPublish>();
            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));
            var logger = new Mock<ILogger<NotificationService>>();
            var service = new NotificationService(notificatioPublish.Object, logger.Object);
            var ret = await service.PublishMessageFromTemplate(emailTemplateInput);
            // Assert
            Assert.True(ret);
            notificatioPublish.Verify(m => m.PublishEmail(It.IsAny<EmailModel>()), Times.Once);


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