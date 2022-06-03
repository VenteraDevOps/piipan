using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Match.Api.Models
{
    public class StateInfoResponse
    {
        [JsonProperty("results")]
        public List<StateInfoResponseData> Results { get; set; } = new List<StateInfoResponseData>();
    }
}
