using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Dapper;
using FluentValidation;
using Newtonsoft.Json;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Shared.Http;

namespace Piipan.Match.Res.Func.Api
{
    public class MatchResApi
    {
        [FunctionName("GetMatchDetails")]
        public async Task<IActionResult> GetMatchDetails(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "matches/{matchId}")] HttpRequest req,
            string matchId,
            ILogger logger)
        {
            LogRequest(logger, req);

            try
            {
                // business logic here
            }
            catch (HttpRequestException ex)
            {
                return ApiErrors.BadRequestErrorResponse(ex);
            }
            catch (Exception ex)
            {
                return ApiErrors.InternalServerErrorResponse(ex);
            }

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("UpdateDisposition")]
        public async Task<IActionResult> UpdateDisposition(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "matches/{matchId}/disposition")] HttpRequest req,
            string matchId,
            ILogger logger)
        {
            LogRequest(logger, req);

            try
            {

            }
            catch (StreamParserException ex)
            {
                return ApiErrors.DeserializationErrorResponse(ex);
            }
            catch (ValidationException ex)
            {
                return ApiErrors.ValidationErrorResponse(ex);
            }
            catch (HttpRequestException ex)
            {
                return ApiErrors.BadRequestErrorResponse(ex);
            }
            catch (Exception ex)
            {
                return ApiErrors.InternalServerErrorResponse(ex);
            }

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        private void LogRequest(ILogger logger, HttpRequest request)
        {
            logger.LogInformation("Executing request from user {User}", request.HttpContext?.User.Identity.Name);

            string subscription = request.Headers["Ocp-Apim-Subscription-Name"];
            if (subscription != null)
            {
                logger.LogInformation("Using APIM subscription {Subscription}", subscription);
            }

            string username = request.Headers["From"];
            if (username != null)
            {
                logger.LogInformation("on behalf of {Username}", username);
            }
        }
    }
}
