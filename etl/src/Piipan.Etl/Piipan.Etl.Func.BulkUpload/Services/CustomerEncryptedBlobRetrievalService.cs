using Azure.Storage.Blobs;

namespace Piipan.Etl.Func.BulkUpload.Services
{
    public class CustomerEncryptedBlobRetrievalService : ICustomerEncryptedBlobRetrievalService
    {
        private const string UPLOAD_CONTAINER_NAME = "upload";

        public BlobClient RetrieveBlob(string storageAccountConnectionString, string blobName, string customerProvidedKey)
        {
            BlobClientOptions blobClientOptions = new BlobClientOptions() { CustomerProvidedKey = new Azure.Storage.Blobs.Models.CustomerProvidedKey(customerProvidedKey) };
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageAccountConnectionString, blobClientOptions);
            
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(UPLOAD_CONTAINER_NAME);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient;
        }
    }
}
