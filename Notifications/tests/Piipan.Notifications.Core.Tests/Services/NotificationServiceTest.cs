using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Core.Services;
using Piipan.Notifications.Models;
using Piipan.Notifications.Services;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    public class NotificationServiceTest
    {

        [Fact]
        public async Task PublishMessageFromTemplateTest()
        {
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
        public async Task PublishMessageFromTemplateTest_Exception()
        {
            var emailTemplateInput = new EmailTemplateInput { };
            var notificatioPublish = new Mock<INotificationPublish>();
            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));
            var logger = new Mock<ILogger<NotificationService>>();
            var service = new NotificationService(notificatioPublish.Object, logger.Object);
            // Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => service.PublishMessageFromTemplate(emailTemplateInput));
        }
    }
}
