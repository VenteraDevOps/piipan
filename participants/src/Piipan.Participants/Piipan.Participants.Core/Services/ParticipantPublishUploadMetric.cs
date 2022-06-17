using System;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

namespace Piipan.Participants.Core.Services
{
    /// <summary>
    /// Class responsible for publishing an EventGridEvent for status updates regarding BulkUpload metrics
    /// </summary>
    public class ParticipantPublishUploadMetric : IParticipantPublishUploadMetric
    {
        public EventGridPublisherClient _client;

        public ParticipantPublishUploadMetric(){

                //Create event grid client to publish metric data
                _client = new EventGridPublisherClient(
                    new Uri(Environment.GetEnvironmentVariable("EventGridEndPoint")),
                    new AzureKeyCredential(Environment.GetEnvironmentVariable("EventGridKeyString")),
                    default
                );

        }

        /// <summary>
        /// Publishes an event to EventGrid containing ParticipantUpload metrics
        /// </summary>
        /// <param name="metrics">Bulk Upload Metadata And Metrics</param>
        /// <returns></returns>
        public Task PublishUploadMetric(ParticipantUpload metrics)
        {

            var result = JsonConvert.SerializeObject(metrics);

            //We want to use pre-serialized verion of the ParticipantUpload.
            //Otherwise, EventGridEvent serializes it according to class property names rather than JsonProperty attributes
            var binaryData = BinaryData.FromString(result);

            // Add EventGridEvents to a list to publish to the topic
            EventGridEvent egEvent =
                new EventGridEvent(
                    "Add participation",
                    "Upload to the database",
                    "1.0",
                    binaryData);

            // Send the event
            _client.SendEventAsync(egEvent);

            return Task.CompletedTask;
        }
    }
}