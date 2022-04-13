// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Piipan.Etl.Func.BulkUpload
{
    /// <summary>
    /// Azure Function implementing basic Extract-Transform-Load of piipan
    /// bulk import CSV files via Storage Containers, Event Grid, and
    /// PostgreSQL.
    /// </summary>
    public class BulkUpload
    {
        private readonly IParticipantApi _participantApi;
        private readonly IParticipantStreamParser _participantParser;

        public BulkUpload(
            IParticipantApi participantApi,
            IParticipantStreamParser participantParser)
        {
            _participantApi = participantApi;
            _participantParser = participantParser;
        }

        /// <summary>
        /// Entry point for the state-specific Azure Function instance
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="input">handle to CSV file uploaded to a state-specific container</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// The function is expected to be executing as a managed identity that has read access
        /// to the per-state storage account and write access to the per-state database.
        /// </remarks>
        [FunctionName("BulkUpload")]
        public async Task Run(
            [QueueTrigger("qupload", Connection = "BlobStorageConnectionString")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation(myQueueItem);

            try
            {
                //parse queue event
                var queuedEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                var createdBlobEvent = JsonConvert.DeserializeObject<StorageBlobCreatedEventData>(queuedEvent.Data.ToString());

                //Get blob name from the blob url
                var blobUrl = new Uri(createdBlobEvent.Url);                 
                string[] urlParts = blobUrl.Segments;
                string blobName = urlParts[urlParts.Length-1];

                BlockBlobClient blob = new BlockBlobClient(Environment.GetEnvironmentVariable("BlobStorageConnectionString"), "upload", blobName);

                Stream input = blob.OpenRead();

                if (input != null)
                {
                    var participants = _participantParser.Parse(input);
                    await _participantApi.AddParticipants(participants);
                }
                else
                {
                    // Can get here if Function does not have
                    // permission to access blob URL
                    log.LogError("No input stream was provided");
                }
                
     
            }
            catch (Exception ex)
            {
                log.LogError("No grid event raised");
            }

        }
    }
}
