using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Piipan.Notifications.Func.Api
{
    public class NotificationApi
    {
        public NotificationApi()
        {
        }

        [FunctionName("NotificationRequestProcessor")]
        public async Task Run([QueueTrigger("emailbucket", Connection = "")] string emailQueue, ILogger log)
        {
            log.LogInformation($"Email Queue trigger function processed: {emailQueue}");
            try
            {
                if (emailQueue == null || emailQueue.Length == 0)
                {

                    log.LogError("No input was provided");
                }
                else
                {
                    // Need to Get the Email Model and call the Mail client.
                    log.LogInformation("Email Queue trigger function processed");

                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
