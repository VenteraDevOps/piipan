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
    public class UpdateMatchMetrics
    {
        private readonly IParticipantMatchWriterApi _participantMatchWriterApi;

        public UpdateMatchMetrics(IParticipantMatchWriterApi participantSearchWriterApi)
        {
            _participantMatchWriterApi = participantSearchWriterApi;
        }
        /// <summary>
        /// Listens for Find_Matches events when users searche participants;
        /// updates meta info to Metrics database for the match.
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("UpdateMatchMetrics")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            try
            {
                ParticipantMatchMetrics newParticipantSearch = JsonConvert.DeserializeObject<ParticipantMatchMetrics>(eventGridEvent.Data.ToString());
                //  CheckParticipantSearch(newParticipantSearch);
                int nRows = await _participantMatchWriterApi.UpdateMatchMetrics(newParticipantSearch);

                log.LogInformation(String.Format("Number of rows updated={0}", nRows));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
        private void CheckParticipantSearch(ParticipantSearch metric)
        {
            if (metric.State == null)
                throw new ArgumentException("Error with ParticipantSearch");
        }
    }
}
