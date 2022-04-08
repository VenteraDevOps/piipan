using Newtonsoft.Json;

namespace Piipan.Match.Api.Models.Resolution
{
    public class MatchResApiResponse
    {
        [JsonProperty("data", Required = Required.Always)]
        public MatchResRecord Data { get; set; }
    }
}
