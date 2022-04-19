using Newtonsoft.Json;
using System.Collections.Generic;

namespace Piipan.Participants.Api.Models
{
    /// <summary>
    /// Represents the top-level response body for a successful API response
    /// </summary>
    public class BulkUploadResponse
    {
        [JsonProperty("data")]
        public BulkUploadResponseData Data { get; set; } = new BulkUploadResponseData();
    }
}
