using System.Collections.Generic;
using System.IO;
using Piipan.Participants.Api.Models;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    public interface IBlobClientStream
    {
        BlockBlobClient Parse(string input, ILogger log);

        // BlobProperties BlobClientProperties(string input, ILogger log);

        // public Response<BlobProperties> GetBlobProperties(BlockBlobClient blob);
    }
}
