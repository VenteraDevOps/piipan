using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Func.ResolutionApi.Models
{
    /// <summary>
    /// API response schema for 200 response
    /// </summary>
    public class ApiResponse
    {
        [JsonProperty("data", Required = Required.Always)]
        public MatchResRecord Data { get; set; }
    }
}
