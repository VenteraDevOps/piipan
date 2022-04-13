using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Func.Api.Extensions;
using Piipan.Metrics.Api;
using System.Threading.Tasks;

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
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity?.Name);

            try
            {
                var state = req.Query.ParseString("state");
                var perPage = req.Query.ParseInt("perPage", 50);
                var page = req.Query.ParseInt("page", 1);

                var response = await _participantUploadApi.GetUploads(state, perPage, page);

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
