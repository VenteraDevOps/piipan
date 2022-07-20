using Newtonsoft.Json;

namespace Piipan.States.Api.Models
{
    public class StatesInfoResponse
    {
        [JsonProperty("data")]
        public IEnumerable<StateInfoResponseData> Results { get; set; } = Enumerable.Empty<StateInfoResponseData>();
    }
}
