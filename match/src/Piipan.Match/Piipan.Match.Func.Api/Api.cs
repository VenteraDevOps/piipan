using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Func.Api.DataTypeHandlers;
using Piipan.Shared.Http;

namespace Piipan.Match.Func.Api
{
    /// <summary>
    /// Azure Function implementing orchestrator matching API.
    /// </summary>
    public class MatchApi
    {
        private readonly IMatchApi _matchApi;
        private readonly IStreamParser<OrchMatchRequest> _requestParser;
        private readonly IMatchEventService _matchEventService;

        public MatchApi(
            IMatchApi matchApi,
            IStreamParser<OrchMatchRequest> requestParser,
            IMatchEventService matchEventService)
        {
            _matchApi = matchApi;
            _requestParser = requestParser;
            _matchEventService = matchEventService;

            SqlMapper.AddTypeHandler(new DateTimeListHandler());
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        /// <summary>
        /// API endpoint for conducting matches across all participating states
        /// using de-identified data
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="logger">handle to the function log</param>
        /// <remarks>
        /// This function is expected to be executing as a resource with read
        /// access to the per-state participant databases.
        /// </remarks>
        [FunctionName("find_matches")]
        public async Task<IActionResult> Find(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            try
            {
                LogRequest(logger, req);

                var initiatingState = InitiatingState(req);
                var request = await _requestParser.Parse(req.Body);
                var response = await _matchApi.FindMatches(request, initiatingState);
                response = await _matchEventService.ResolveMatches(request, response, initiatingState);

                return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
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

        private string InitiatingState(HttpRequest request)
        {
            string state = request.Headers["X-Initiating-State"];

            if (String.IsNullOrEmpty(state))
            {
                throw new HttpRequestException("Request is missing required header: X-Inititating-State");
            }

            return state;
        }
    }
}
