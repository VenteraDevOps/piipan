using Azure.Storage.Blobs;

namespace Piipan.Etl.Func.BulkUpload.Services
{
    public interface ICustomerEncryptedBlobRetrievalService
    {
        BlobClient RetrieveBlob(string storageAccountConnectionString, string blobName, string customerProvidedKey);
    }
}
