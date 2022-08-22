using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Core.Services;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    public class NotificationPublishTest
    {
        [Fact]
        public async void NotificationPublish_Sucess()
        {
            Environment.SetEnvironmentVariable("EventGridEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString", "example");
            var logger = Mock.Of<ILogger<EmailModel>>();
            var notificationPublish = new NotificationPublish(logger);
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

        }

    }

}
