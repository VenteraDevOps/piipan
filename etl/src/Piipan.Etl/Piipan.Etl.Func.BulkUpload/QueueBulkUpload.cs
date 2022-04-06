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
using Azure.Storage.Blobs;


namespace Piipan.Etl.Func.BulkUpload
{
    /// <summary>
    /// Azure Function implementing basic Extract-Transform-Load of piipan
    /// bulk import CSV files via Storage Containers, Event Grid, and
    /// PostgreSQL.
    /// </summary>
    public class QueueBulkUpload
    {
        private readonly IParticipantApi _participantApi;
        private readonly IParticipantStreamParser _participantParser;

        public QueueBulkUpload(
            IParticipantApi participantApi,
            IParticipantStreamParser participantParser)
        {
            _participantApi = participantApi;
            _participantParser = participantParser;
        }

        [FunctionName("QueueBulkUpload")]
        public async Task Run(
            [QueueTrigger("qupload", Connection = "BlobStorageConnectionString")] string myQueueItem,
            [Blob("upload/{queueTrigger}", FileAccess.Read, Connection = "BlobStorageConnectionString")] Stream input,
            ILogger log)
        {

            try
            {
                if (input != null)
                {
                    log.LogError("Check this: " + input.ToString());
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
        }

    }
}