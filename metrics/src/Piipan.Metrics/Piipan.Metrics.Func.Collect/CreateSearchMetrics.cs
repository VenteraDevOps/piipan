using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Func.Collect
{
    public class CreateSearchMetrics
    {
        private readonly IParticipantSearchWriterApi _participantSearchWriterApi;

        public CreateSearchMetrics(IParticipantSearchWriterApi participantSearchWriterApi)
        {
            _participantSearchWriterApi = participantSearchWriterApi;
        }
        /// <summary>
        /// Listens for Find_Matches events when users searche participants;
        /// write meta info to Metrics database
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("CreateSearchMetrics")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            try
            {
                ParticipantSearch newParticipantSearch = JsonConvert.DeserializeObject<ParticipantSearch>(eventGridEvent.Data.ToString());
                CheckParticipantSearch(newParticipantSearch);
                int nRows = await _participantSearchWriterApi.AddSearchMetrics(newParticipantSearch);

                log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
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
