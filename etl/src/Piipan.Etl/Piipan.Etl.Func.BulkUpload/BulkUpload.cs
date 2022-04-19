// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;

namespace Piipan.Etl.Func.BulkUpload
{
    /// <summary>
    /// Azure Function implementing basic Extract-Transform-Load of piipan
    /// bulk import CSV files via Storage Containers, Event Grid, and
    /// PostgreSQL.
    /// </summary>
    public class BulkUpload
    {
        private readonly IParticipantApi _participantApi;
        private readonly IParticipantStreamParser _participantParser;

        public BulkUpload(
            IParticipantApi participantApi,
            IParticipantStreamParser participantParser)
        {
            _participantApi = participantApi;
            _participantParser = participantParser;
        }

        /// <summary>
        /// Entry point for the state-specific Azure Function instance
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="input">handle to CSV file uploaded to a state-specific container</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// The function is expected to be executing as a managed identity that has read access
        /// to the per-state storage account and write access to the per-state database.
        /// </remarks>
        [FunctionName("BulkUpload")]
        public async Task<IActionResult> Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [Blob("{data.url}", FileAccess.Read,Connection = "BlobStorageConnectionString")] Stream input,
            [Blob("{data.url}", FileAccess.Read, Connection = "BlobStorageConnectionString")] BlobClient blobClient,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            try
            {
                if (input != null)
                {
                    var participants = _participantParser.Parse(input);
                    BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
                    var response = await _participantApi.AddParticipants(participants,  blobProperties.ETag.ToString());
                    return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
                }
                else
                {
                    // Can get here if Function does not have
                    // permission to access blob URL
                    log.LogError("No input stream was provided");
                    return BadRequestErrorresponse("No input stream was provided");
                }
            }
            catch (HttpRequestException ex)
            {
                log.LogError(ex.Message);
                return BadRequestErrorresponse(ex);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return InternalServerErrorResponse(ex);
            }
        }
        private ActionResult InternalServerErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.InternalServerError),
                Title = ex.GetType().Name,
                Detail = ex.Message
            });
            return (ActionResult)new JsonResult(errResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }

        private ActionResult BadRequestErrorresponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                Title = "Bad request",
                Detail = ex.Message
            });
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }
        private ActionResult BadRequestErrorresponse(string Message)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                Title = "Bad request",
                Detail = Message
            });
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }
    }
}
