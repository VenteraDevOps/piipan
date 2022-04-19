using Newtonsoft.Json;
using System.Collections.Generic;

namespace Piipan.Participants.Api.Models
{
    /// <summary>
    /// Represents the top-level response body for a successful API response
    /// </summary>
    public class BulkUploadResponseData
    {
        [JsonProperty("results")]
        public BulkUploadResult Results { get; set; } = new BulkUploadResult();

        [JsonProperty("errors")]
        public List<BulkUploadError> Errors { get; set; } = new List<BulkUploadError>();
    }
}
