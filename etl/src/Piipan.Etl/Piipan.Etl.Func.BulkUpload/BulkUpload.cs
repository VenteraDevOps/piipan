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
using Microsoft.Azure.Storage.Blob;
using Azure.Storage.Blobs;





/**************************************
1. create the queue
2. copy [FunctionName("BulkUpload")] and change it (or create a new functiona) to [FunctionName("EventGridBlobTrigger")] from https://medium.com/@adrianivan/processing-files-from-azure-blob-storage-using-event-grid-and-storage-queue-in-a-reactive-way-ef3ad30cd081
3. copy [FunctionName("BulkUpload")] logic and create [FunctionName("QueueTriggerFunction")] puting the logic under //code for blob processing
**************************************/







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
        [return: Queue("qupload", Connection = "BlobStorageConnectionString")]
        public static string Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [Blob("{data.url}", FileAccess.Read, Connection = "BlobStorageConnectionString")] BlobClient blob,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());

		    log.LogInformation($"{blob.Name}");

  	        return blob.Name;
        }

    }
}
