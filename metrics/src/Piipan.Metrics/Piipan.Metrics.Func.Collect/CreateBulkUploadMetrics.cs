// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;
using Piipan.Participants.Core.Enums;

namespace Piipan.Metrics.Func.Collect
{
    /// <summary>
    /// Azure Function to capture initial metrics for for bulk uploads.
    /// Creates a ParticipantUpload record in the metrics database with initial meta info
    /// </summary>
    public class CreateBulkUploadMetrics
    {
        private readonly IParticipantUploadWriterApi _participantUploadWriterApi;

        public CreateBulkUploadMetrics(IParticipantUploadWriterApi participantUploadWriterApi)
        {
            _participantUploadWriterApi = participantUploadWriterApi;
        }

        /// <summary>
        /// Listens for BulkUpload events when users upload participants;
        /// write meta info to Metrics database
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("CreateBulkUploadMetrics")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            try
            {
                string state = ParseState(eventGridEvent);
                string uploadId = ParseUploadId(eventGridEvent);
                DateTime uploadedAt = eventGridEvent.EventTime.DateTime;
                
                ParticipantUpload newParticipantUpload = new ParticipantUpload()
                {
                    UploadedAt = uploadedAt,
                    State = state,
                    Status = UploadStatuses.UPLOADING.ToString(),
                    UploadIdentifier = uploadId
                };
                int nRows = await _participantUploadWriterApi.AddUploadMetrics(newParticipantUpload);

                log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        private string ParseState(EventGridEvent eventGridEvent)
        {
            try
            {
                var jsondata = eventGridEvent.Data.ToString(); ;
                var tmp = new { url = "" };
                var data = JsonConvert.DeserializeAnonymousType(jsondata, tmp);

                Regex regex = new Regex("^https://([a-z]+)upload");
                Match match = regex.Match(data.url);

                var val = match.Groups[1].Value;
                return val.Substring(val.Length - 2); // parses abbreviation from match value
            }
            catch (Exception ex)
            {
                throw new FormatException("State not found", ex);
            }
        }

        private string ParseUploadId(EventGridEvent eventGridEvent)
        {
            try
            {
                var jsondata = eventGridEvent.Data.ToString();
                var tmp = new { eTag = "" };
                var data = JsonConvert.DeserializeAnonymousType(jsondata, tmp);

                return data.eTag;
            }
            catch (Exception ex)
            {
                throw new FormatException("ETag (Upload Identifier) not found", ex);
            }
        }
    }
}
