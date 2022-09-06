using Moq;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Common.Tests.Services
{
    public class ViewRenderServiceTest : BasePageTest
    {

        [Fact]
        public async Task GenerateMessageContentCreateMatchBodyISTest()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchRecord = new MatchModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("MatchEmailIS.cshtml", notification.MatchRecord);
            // Assert
            Assert.Contains("foo", emailBody);

        }
        [Fact]
        public async Task GenerateMessageContentCreateMatchBodyMSTest()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchRecord = new MatchModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("MatchEmailMS.cshtml", notification.MatchRecord);
            // Assert
            Assert.Contains("foo", emailBody);

        }
        [Fact]
        public async Task GenerateMessageContentCreateMatchSubjectMSTest()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchRecord = new MatchModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("MatchEmailMS_Sub.cshtml", notification.MatchRecord);
            // Assert
            Assert.Contains("Matching State:", emailBody);

        }
        [Fact]
        public async Task GenerateMessageContentCreateMatchSubjectISTest()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchRecord = new MatchModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("MatchEmailIS_sub.cshtml", notification.MatchRecord);
            // Assert
            Assert.Contains("Initiating State:", emailBody);

        }
        [Fact]
        public async Task GenerateMessageContentUpdateMatchResEventRecoredBodyTest()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchResEvent = new DispositionModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("DispositionEmail.cshtml", notification.MatchResEvent);
            // Assert
            Assert.Contains("foo", emailBody);

        }
        [Fact]
        public async Task GenerateMessageContentUpdateMatchResEventRecoredSubjectTest()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchResEvent = new DispositionModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("DispositionEmail_Sub.cshtml", notification.MatchResEvent);
            // Assert
            Assert.Contains(String.Format("Updates made on to NAC match {0}", "foo"), emailBody);

        }
        [Fact]
        public async Task GenerateMessageContentEmailTemplateNotFoundTest()
        {
            var notification = new NotificationRecord()
            {
                MatchRecord = new MatchModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => viewRenderService.GenerateMessageContent("TestEmail.cshtml", notification.MatchRecord));

        }
        [Fact]
        public async Task GenerateMessageContentPageModelNotFoundTest()
        {
            var notification = new NotificationRecord()
            {
                MatchRecord = new MatchModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                EmailToRecordMS = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => viewRenderService.GenerateMessageContent("TestEmail.cshtml", notification.MatchResEvent));

        }
    }
}
