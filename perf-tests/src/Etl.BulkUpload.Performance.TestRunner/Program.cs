using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Etl.BulkUpload.Performance.TestRunner
{
    [ExcludeFromCodeCoverage]
    internal partial class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the desired number of participants you would like to upload for this performance test. ");
                return;
            }

            long numberofParticipantsToUpload = Convert.ToInt64(args[0]);

            var storageAccountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT");
            var storageAccountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_KEY");
            if (string.IsNullOrEmpty(storageAccountName) || string.IsNullOrEmpty(storageAccountKey))
            {
                //If environment variables have not been set, then attempt to use a local appsettings.json file
                IConfiguration config = InitAppSettingsConfiguration();
                storageAccountName = storageAccountName ?? config["AZURE_STORAGE_ACCOUNT"];
                storageAccountKey = storageAccountKey ?? config["AZURE_STORAGE_KEY"];
            }

            BulkUploadPerfTestRunner testFileCreator = new BulkUploadPerfTestRunner(storageAccountName, storageAccountKey);
            Console.WriteLine("Starting Test!");

            await testFileCreator.runTest(numberofParticipantsToUpload);

            Console.WriteLine("Test Completed");
        }


        /// <summary>
        /// This method is optional. The appsettings.json file is only used in cases where environment variables are not being used.
        /// </summary>
        /// <returns></returns>
        public static IConfiguration InitAppSettingsConfiguration()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            return config;
        }

    }
}
