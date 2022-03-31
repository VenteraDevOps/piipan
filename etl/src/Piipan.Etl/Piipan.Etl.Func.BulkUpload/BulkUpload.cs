// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
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
        private readonly IParticipantApi _participantApi;
        private readonly IParticipantStreamParser _participantParser;
        private readonly IBlobRetrievalService _blobRetrievalService;

        public BulkUpload(
            IParticipantApi participantApi,
            IParticipantStreamParser participantParser, 
            IBlobRetrievalService blobRetrievalService)
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
        //[EventGridTrigger] EventGridEvent eventGridEvent,
        //[Blob("{data.url}", FileAccess.Read, Connection = "BlobStorageConnectionString")] Stream input,
        //ILogger log)
        [QueueTrigger("filenames", Connection = "BlobStorageConnectionString")]
        string filename,
        ILogger log)
        {
            //log.LogInformation(eventGridEvent.Data.ToString());

            //add named values to APIM
            //add Application Setting to Azure Function
            //Assign a key vault access policy to the managed identity with permissions to get and list secrets from the vault.
            //Add role assignment to Azure Function for role assignment
            //Unit tests for BulkUpload.cs, BlobRetrievalService.cs

            log.LogWarning("Starting new ETL at {0}", DateTime.Now);
            Console.WriteLine("Starting new ETL at {0}", DateTime.Now);

            //https://dev.to/pwd9000/protect-secrets-in-azure-functions-using-key-vault-d2i
            //https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references#granting-your-app-access-to-key-vault
            string connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
            string uploadEncryptionKey = Environment.GetEnvironmentVariable("kv_UploadEncryptKey");

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

                log.LogWarning("Finished ETL at {0}", DateTime.Now);
                Console.WriteLine("Finished new ETL at {0}", DateTime.Now);
            }
        }
    }
}
