using System.Collections.Generic;
using System.IO;
using Piipan.Participants.Api.Models;
using Microsoft.Extensions.Logging;

namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    public interface IBlobClientStream
    {
        Stream Parse(string input, ILogger log);
    }
}