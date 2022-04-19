using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Participants.Api.Models
{
    /// <summary>
    /// Represents the item-level error object for a person in an API request
    /// <para> Use for items in the Errors list in the API response data</para>
    /// </summary>
    public class BulkUploadError
    {

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }
}
