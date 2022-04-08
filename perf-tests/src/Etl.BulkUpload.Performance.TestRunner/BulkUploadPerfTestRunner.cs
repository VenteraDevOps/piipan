using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Etl.BulkUpload.Performance.TestRunner
{
    public class BulkUploadPerfTestRunner
    {
        private const string UPLOAD_CONTAINER_NAME = "upload";
        private string _azureStorageAccountName;
        private string _azureStorageAccountKey;

        public BulkUploadPerfTestRunner(string azureStorageAccountName, string azureStorageAccountKey)
        {
            _azureStorageAccountName = azureStorageAccountName;
            _azureStorageAccountKey = azureStorageAccountKey;
        }

        public async Task runTest(long desiredParticipantCount)
        {
            string headers = "lds_hash,case_id,participant_id,benefits_end_month,recent_benefit_issuance_dates,vulnerable_individual";

            using (MemoryStream ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                {
                    await PopulateMemoryStreamWithMockRecords(headers, writer, desiredParticipantCount);

                    string connectionString = $"DefaultEndpointsProtocol=https;AccountName={_azureStorageAccountName};AccountKey={_azureStorageAccountKey};EndpointSuffix=core.windows.net";
                    var blobClient = new BlobContainerClient(connectionString, UPLOAD_CONTAINER_NAME);

                    //string prefix = "venbgf";
                    //string environment = "bill";
                    //string state = "ea";
                    //TokenCredential cred = new AzureCliCredential();
                    //Uri uri = new Uri($"https://{prefix}st{state}upload{environment}.blob.core.windows.net/{UPLOAD_CONTAINER_NAME}");
                    //var client = new BlobServiceClient(uri, new AzureCliCredential());

                    string datePostfix = DateTime.Now.ToString("MM-dd-yy_HH:mm:ss");

                    var blob = blobClient.GetBlobClient($"perfTestUpload-{datePostfix}.csv");

                    ms.Seek(0, SeekOrigin.Begin);
                    Task uploadResult = blob.UploadAsync(ms);
                    uploadResult.Wait();
                }



            }
        }

        private static async Task PopulateMemoryStreamWithMockRecords(string headers, StreamWriter writer, long desiredParticipantCount)
        {
            writer.WriteLine(headers);

            for (int i = 0; i < desiredParticipantCount; i++)
            {
                String record = createMockRecord("EA", i + 1);
                writer.WriteLine(record);
            }

            await writer.FlushAsync();
        }

        private static String createMockRecord(String state, int recId)
        {
            StringBuilder record = new StringBuilder(200);
            var padRecId = recId.ToString("00000000");
            record.Append(createMockHash(128));
            record.Append(",case-" + state + "-" + padRecId);
            record.Append(",part-" + state + "-" + padRecId);
            record.Append(",,2021-12 2021-11 2021-10,false");
            return record.ToString();

        }

        private static String createMockHash(int len)
        {
            StringBuilder mockHash = new StringBuilder(128);
            Random random = new Random();

            for (int i = 0; i < 16; i++)
            {
                int randomNumber = random.Next();
                int digit = (int)Math.Floor((decimal)randomNumber);
                string hexStringInLowercase = digit.ToString("X8").ToLower();
                mockHash.Append(hexStringInLowercase);

            }

            return mockHash.ToString();

        }
    }
}
