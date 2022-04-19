using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Participants.Api.Models
{
    /// <summary>
    /// Represents the entire result for Bulk upload from an API request
    /// <para> UPload Identifier for the Upload states.</para>
    /// </summary>
    public class BulkUploadResult
    {
        [JsonProperty("upload_identifier")]
        public string  UploadIdentifier { get; set; }
      
    }
}
