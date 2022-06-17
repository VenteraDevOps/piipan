using System;
using Azure.Messaging.EventGrid;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Services;
using Xunit;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantPublishUploadMetricTests
    {

        [Fact]
        public async void ParticipantPublishUploadMetric_Sucess()
        {
            // Arrange

            Environment.SetEnvironmentVariable("EventGridEndPoint","http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString","example");
            var participantPublishUploadMetric = new ParticipantPublishUploadMetric();
            Mock<EventGridPublisherClient> publisherClientMock = new Mock<EventGridPublisherClient>();
            participantPublishUploadMetric._client = publisherClientMock.Object;

            ParticipantUpload metric = new ParticipantUpload()
            {
                State = "ea",
                Status = UploadStatuses.COMPLETE.ToString(),
                UploadIdentifier = "UploadIdentifier",
                UploadedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                ParticipantsUploaded = 50

            };

            // Act
            await participantPublishUploadMetric.PublishUploadMetric(metric);
            publisherClientMock.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(s=>s.EventType == "Upload to the database"), default));
        }

    }
}
