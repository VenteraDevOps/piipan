using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    /// <summary>
    /// Data Mapper for participant_uploads table in metrics database
    /// </summary>
    public class ParticipantSearchMetrics
    {
        [JsonProperty("data", Required = Required.Always)]
        public List<ParticipantSearch> Data { get; set; } = new List<ParticipantSearch>();
    }
}
