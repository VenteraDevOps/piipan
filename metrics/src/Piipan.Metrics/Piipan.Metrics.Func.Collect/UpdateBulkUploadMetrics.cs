// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Func.Collect
{
    /// <summary>
    /// Azure Function implementing status update for bulk uploads.
    /// Updates the corresponding metrics record in the database with changed meta info
    /// </summary>
    public class UpdateBulkUploadMetrics
    {
        private readonly IParticipantUploadWriterApi _participantUploadWriterApi;

        public UpdateBulkUploadMetrics(IParticipantUploadWriterApi participantUploadWriterApi)
        {
            _participantUploadWriterApi = participantUploadWriterApi;
        }

        /// <summary>
        /// Listens for BulkUpload events when users upload participants;
        /// updates meta info in Metrics database
        /// </summary>
        /// <param name="eventGridEvent">bulk upload status event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("UpdateBulkUploadMetricsStatus")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            try
            {
                ParticipantUpload participantUpload = JsonConvert.DeserializeObject<ParticipantUpload>(eventGridEvent.Data.ToString());

                CheckParticipantUpload(participantUpload);

                int nRows = await _participantUploadWriterApi.UpdateUploadMetrics(participantUpload);

                log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
            }
            catch (ArgumentException ex)
            {
                log.LogError(ex, "Failed updating Bulk Upload status & metrics.");
                throw;
            }
        }

        private void CheckParticipantUpload(ParticipantUpload upload){
                if(upload.State == null) 
                    throw new ArgumentException("Error with ParticipantUpload");
        }
    }
}
