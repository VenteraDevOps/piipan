// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Piipan.Etl.Func.BulkUpload.Parsers;
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
        //[EventGridTrigger] EventGridEvent eventGridEvent,
        //[Blob("{data.url}", FileAccess.Read, Connection = "BlobStorageConnectionString")] Stream input,
        //ILogger log)
        [QueueTrigger("filenames", Connection = "BlobStorageConnectionString")]
        string filename,
        ILogger log)
        {
            //log.LogInformation(eventGridEvent.Data.ToString());

            //x- Verify decryption works
            //x- Pull from KeyVault instead - grant access to Azure Function with links below
            //x (not possible!) - Try RSA or alternative instead of customer provided key?
            //Research other options for Blob input bindings & decryption
            //Update APIM to enforce customer provided key encryption
            //Switch APIM to pull secrets from Azure Key Vault
            //script to rotate/change keys in Key Vault -> generate console application. Spits values out when called. Potentially even updates Key vault using Azure SDK. Would be separate JIRA ticket

            log.LogWarning("Starting new ETL at {0}", DateTime.Now);
            Console.WriteLine("Starting new ETL at {0}", DateTime.Now);

            //https://dev.to/pwd9000/protect-secrets-in-azure-functions-using-key-vault-d2i
            //https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references#granting-your-app-access-to-key-vault
            string connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
            string myKey = Environment.GetEnvironmentVariable("kv_MyAzEncryptKey");

            BlobClientOptions blobClientOptions = new BlobClientOptions() { CustomerProvidedKey = new Azure.Storage.Blobs.Models.CustomerProvidedKey(myKey) };
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString,
                blobClientOptions);

            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("upload");
            BlobClient blobClient = blobContainerClient.GetBlobClient(filename);
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
