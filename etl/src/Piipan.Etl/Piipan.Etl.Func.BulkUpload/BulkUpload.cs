// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly IBlobClientStream _blobStream;

        public BulkUpload(
            IParticipantApi participantApi,
            IParticipantStreamParser participantParser,
            IBlobClientStream blobStream)
        {
            _participantApi = participantApi;
            _participantParser = participantParser;
            _blobStream = blobStream;
        }

        /// <summary>
        /// Entry point for the state-specific Azure Function instance
        /// </summary>
        /// <param name="myQueueItem">storage queue item</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// The function is expected to be executing as a managed identity that has read access
        /// to the per-state storage account and write access to the per-state database.
        /// </remarks>
        [FunctionName("BulkUpload")]
        public async Task Run(
            [QueueTrigger("upload", Connection = "BlobStorageConnectionString")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation(myQueueItem);
            try
            {
                if(myQueueItem == null || myQueueItem.Length == 0){
                    
                    log.LogError("No input stream was provided");
                }
                else {
                    var blockBlobClient =  _blobStream.Parse(myQueueItem, log);

                    Stream input = new System.IO.MemoryStream();

                    var response = blockBlobClient.DownloadTo(input);

                    var blobProperties = blockBlobClient.GetProperties().Value;

                    if (input != null)
                    {
                        var participants = _participantParser.Parse(input);
                        await _participantApi.AddParticipants(participants,  blobProperties.ETag.ToString())
                                .ContinueWith(t => _blobStream.DeleteBlobAfterProcessing(t, blockBlobClient, log));
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
