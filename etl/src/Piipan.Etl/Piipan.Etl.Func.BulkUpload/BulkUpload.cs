// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.IO;
using System.Threading.Tasks;
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
                if (myQueueItem == null || myQueueItem.Length == 0)
                {

                    log.LogError("No input stream was provided");
                }
                else
                {
                    var blockBlobClient = _blobStream.Parse(myQueueItem, log);

                    using Stream input = await blockBlobClient.OpenReadAsync();

                    log.LogInformation($"Input lenght: {input.Length} Position: {input.Position}");

                    var blobProperties = blockBlobClient.GetProperties().Value;

                    if (input != null)
                    {
                        var participants = _participantParser.Parse(input);
                        DateTime startUploadTime = DateTime.UtcNow;

                        //Azure documents that ETags are quoted. So we need to remove the quotes in order to get the upload_id
                        var upload_id = blobProperties.ETag.ToString().Replace("\"", "");

                        string state = Environment.GetEnvironmentVariable("State");

                        await _participantApi.AddParticipants(participants, upload_id, state, (ex) =>
                            {
                                // reset the participants and input stream. If you only reset the input stream you start with the header row, 
                                // and if you don't reset it you're missing participants that have already been read
                                input.Seek(0, SeekOrigin.Begin);
                                participants = _participantParser.Parse(input);

                                _participantApi.LogParticipantsUploadError(
                                    new(state, startUploadTime, DateTime.UtcNow, ex, blockBlobClient.Name),
                                    participants);
                            })
                                .ContinueWith(t => _blobStream.DeleteBlobAfterProcessing(t, blockBlobClient, log))
                                .ContinueWith(t => _participantApi.DeleteOldParticpants());

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
