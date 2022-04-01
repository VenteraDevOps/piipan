// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Etl.Func.BulkUpload.Services;
using Piipan.Participants.Api;

namespace Piipan.Etl.Func.BulkUpload
{
    /// <summary>
    /// Azure Function implementing basic Extract-Transform-Load of piipan
    /// bulk import CSV files via Storage Containers, Event Grid, and
    /// PostgreSQL.
    /// </summary>
    public class BulkUpload
    {
        private const string UPLOAD_PAYLOAD_KEY = "upload-payload-key";
        private const string BLOB_STORAGE_CONNECTION_STRING = "BlobStorageConnectionString";

        private readonly IParticipantApi _participantApi;
        private readonly IParticipantStreamParser _participantParser;
        private readonly ICustomerEncryptedBlobRetrievalService _blobRetrievalService;

        public BulkUpload(
            IParticipantApi participantApi,
            IParticipantStreamParser participantParser,
            ICustomerEncryptedBlobRetrievalService blobRetrievalService)
        {
            _participantApi = participantApi;
            _participantParser = participantParser;
            _blobRetrievalService = blobRetrievalService;
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
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            StorageBlobCreatedEventData blobCreatedEvent = eventGridEvent.Data as StorageBlobCreatedEventData;
            BlobUriBuilder blobUriBuilder = new BlobUriBuilder(new Uri(blobCreatedEvent.Url));
            var filename = blobUriBuilder.BlobName;

            log.LogInformation($"ETL triggered by filename: {filename}");         

            log.LogInformation("Starting new ETL at {0}", DateTime.Now);
            Console.WriteLine("Starting new ETL at {0}", DateTime.Now);

            string connectionString = Environment.GetEnvironmentVariable(BLOB_STORAGE_CONNECTION_STRING);
            string uploadEncryptionKey = Environment.GetEnvironmentVariable(UPLOAD_PAYLOAD_KEY);

            var blobClient = _blobRetrievalService.RetrieveBlob(connectionString, filename, uploadEncryptionKey);

            using (Stream input = blobClient.OpenRead())
            {
                try
                {
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
                    log.LogError(ex.Message);
                    throw;
                }

                log.LogInformation("Finished ETL at {0}", DateTime.Now);
                Console.WriteLine("Finished new ETL at {0}", DateTime.Now);
            }
        }
    }
}
