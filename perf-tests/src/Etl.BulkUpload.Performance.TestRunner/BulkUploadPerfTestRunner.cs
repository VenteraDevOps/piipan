using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Shared.API.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                    Console.WriteLine($"Begin populating Mock Records - {DateTime.Now.ToLongTimeString()}");
                    await PopulateMemoryStreamWithMockRecords(writer, desiredParticipantCount);
                    Console.WriteLine($"Finish populating Mock Records - {DateTime.Now.ToLongTimeString()}");

                    string connectionString = $"DefaultEndpointsProtocol=https;AccountName={_azureStorageAccountName};AccountKey={_azureStorageAccountKey};EndpointSuffix=core.windows.net";
                    var blobClient = new BlobContainerClient(connectionString, UPLOAD_CONTAINER_NAME);

                    string datePostfix = DateTime.Now.ToString("MM-dd-yy_HH:mm:ss");

                    var blob = blobClient.GetBlobClient($"perfTestUpload-{datePostfix}.csv");

                    ms.Seek(0, SeekOrigin.Begin);

                    Console.WriteLine($"Begin upload to Azure Storage - {DateTime.Now.ToLongTimeString()}");
                    Task uploadResult = blob.UploadAsync(ms);
                    uploadResult.Wait();
                    Console.WriteLine($"Finish upload to Azure Storage - {DateTime.Now.ToLongTimeString()}");
                }



            }
        }


        private static async Task PopulateMemoryStreamWithMockRecords(StreamWriter writer, long desiredParticipantCount)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            //StringWriter stringWriter = new StringWriter();
            var csvwriter = new CsvWriter(writer, config);
            csvwriter.Context.RegisterClassMap<ParticipantMap>();

            csvwriter.WriteHeader<Participant>();
            csvwriter.NextRecord();

            var state = "EA";

            for (int i = 0; i < desiredParticipantCount; i++)
            {
                var p = new Participant();

                var recId = i + 1;
                var padRecId = recId.ToString("00000000");
                p.LdsHash = createMockHash(128);

                p.CaseId = $"case-{state}-{padRecId}";
                p.ParticipantId = $"part-{state}-{padRecId}";
                p.ParticipantClosingDate = DateTime.Now;

                var dr1 = new DateRange(new DateTime(2021, 04, 01), new DateTime(2021, 04, 15));
                var dr2 = new DateRange(new DateTime(2021, 03, 01), new DateTime(2021, 03, 30));
                var dr3 = new DateRange(new DateTime(2021, 02, 01), new DateTime(2021, 02, 28));
                p.RecentBenefitIssuanceDates = new List<DateRange>() { dr1, dr2, dr3 };
                p.VulnerableIndividual = true;

                csvwriter.WriteRecord(p);
                csvwriter.NextRecord();
            }

            csvwriter.Flush();

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
