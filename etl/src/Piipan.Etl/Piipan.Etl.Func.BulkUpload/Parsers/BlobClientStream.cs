using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Piipan.Participants.Api.Models;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;


namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    public class BlobClientStream : IBlobClientStream
    {

        public virtual string GetBlobName(StorageBlobCreatedEventData blobEvent) {

                //Get blob name from the blob url
                var blobUrl = new Uri(blobEvent.Url);
                BlobUriBuilder blobUriBuilder = new BlobUriBuilder(blobUrl);

                var blobName = blobUriBuilder.BlobName;

                return blobName;
        }

        public virtual BlockBlobClient GetBlob(string blobName) {                

            return new BlockBlobClient(Environment.GetEnvironmentVariable("BlobStorageConnectionString"), "upload", blobName);
        }

        public virtual Response<BlobProperties> GetBlobProperties(BlockBlobClient blob) {                

            return blob.GetProperties();
        }

        public Stream Parse(string input, ILogger log) {
            try
            {
                //parse queue event
                var queuedEvent = Azure.Messaging.EventGrid.EventGridEvent.Parse(BinaryData.FromString(input));
                var createdBlobEvent = queuedEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();

                var blobName = GetBlobName(createdBlobEvent);

                BlockBlobClient blob = GetBlob(blobName);

                Stream returnStream = new System.IO.MemoryStream();

                var response = blob.DownloadTo(returnStream);

                return returnStream;
            }
            catch (System.Exception ex)
            {
                log.LogError("Error parsing blob event");
                throw ex;
            }
        }

        public BlobProperties BlobClientProperties(string input, ILogger log) {

            try
            {
                //parse queue event
                var queuedEvent = Azure.Messaging.EventGrid.EventGridEvent.Parse(BinaryData.FromString(input));
                var createdBlobEvent = queuedEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();

                var blobName = GetBlobName(createdBlobEvent);

                BlockBlobClient blob = GetBlob(blobName);

                var response = GetBlobProperties(blob);

                return response.Value;
            }
            catch (System.Exception ex)
            {
                log.LogError("Error parsing blob event");
                throw ex;
            }
        }
    }
}
