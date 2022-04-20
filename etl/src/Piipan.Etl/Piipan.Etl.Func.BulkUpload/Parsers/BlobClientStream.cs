using System;
using System.Collections.Generic;
using System.IO;
using Piipan.Participants.Api.Models;
using Microsoft.Extensions.Logging;
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
        public Stream Parse(string input, ILogger log) {

            try
            {
                //parse queue event
                var queuedEvent = JsonConvert.DeserializeObject<EventGridEvent>(input);
                var createdBlobEvent = JsonConvert.DeserializeObject<StorageBlobCreatedEventData>(queuedEvent.Data.ToString());

                //Get blob name from the blob url
                var blobUrl = new Uri(createdBlobEvent.Url);                 
                string[] urlParts = blobUrl.Segments;
                string blobName = urlParts[urlParts.Length-1];

                BlockBlobClient blob = new BlockBlobClient(Environment.GetEnvironmentVariable("BlobStorageConnectionString"), "upload", blobName);

                Stream returnStream = new System.IO.MemoryStream();

                var response = blob.DownloadTo(returnStream);

                return returnStream;
            }
            catch (System.Exception)
            {
                log.LogError("Error parsing blob event");
                throw;
            }
        }

        public BlobProperties BlobClientProperties(string input, ILogger log) {

            try
            {
                //parse queue event
                var queuedEvent = JsonConvert.DeserializeObject<EventGridEvent>(input);
                var createdBlobEvent = JsonConvert.DeserializeObject<StorageBlobCreatedEventData>(queuedEvent.Data.ToString());

                //Get blob name from the blob url
                var blobUrl = new Uri(createdBlobEvent.Url);                 
                string[] urlParts = blobUrl.Segments;
                string blobName = urlParts[urlParts.Length-1];

                BlockBlobClient blob = new BlockBlobClient(Environment.GetEnvironmentVariable("BlobStorageConnectionString"), "upload", blobName);

                return blob.GetProperties();
            }
            catch (System.Exception)
            {
                log.LogError("Error parsing blob event");
                throw;
            }
        }
    }
}