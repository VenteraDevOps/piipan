using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Metrics.Func.Api
{
    public class GetParticipantUploads
    {
        private readonly IParticipantUploadReaderApi _participantUploadApi;

        public GetParticipantUploads(IParticipantUploadReaderApi participantUploadApi)
        {
            _participantUploadApi = participantUploadApi;
        }

        [FunctionName("GetParticipantUploads")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {


            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            try
            {
                var query = req.Query; // this is IQueryCollection
                var json = JsonConvert.SerializeObject(query.ToDictionary(q => q.Key, q => q.Value.ToString()));
                var filter = JsonConvert.DeserializeObject<ParticipantUploadRequestFilter>(json);

                var response = await _participantUploadApi.GetUploads(filter);

                return (ActionResult)new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
