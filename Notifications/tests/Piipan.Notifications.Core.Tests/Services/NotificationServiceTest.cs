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
        public async Task CreateMessageFromTemplateTest()
        {
            var emailTemplateInput = new EmailTemplateInput { EmailTo = "Test@test.com", Topic = "UPDATE_MATCH_RES" };
            var notificatioPublish = new Mock<INotificationPublish>();
            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));
            var service = new NotificationService(notificatioPublish.Object);
            var ret = await service.CreateMessageFromTemplate(emailTemplateInput);
            // Assert
            Assert.True(ret);
            notificatioPublish.Verify(m => m.PublishEmail(It.IsAny<EmailModel>()), Times.Once);
        }
    }
}
