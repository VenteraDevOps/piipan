using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using FluentValidation;

namespace Piipan.Shared.Http
{
    /// <summary>
    /// Represents a generic error response for an API request
    /// </summary>
    public class ApiErrorResponse
    {
        [JsonProperty("errors", Required = Required.Always)]
        public List<ApiHttpError> Errors { get; set; } = new List<ApiHttpError>();
    }

    /// <summary>
    /// Represents http-level and other top-level errors for an API request
    /// <para> Use for items in the Errors list in the top-level API response</para>
    /// </summary>
    public class ApiHttpError
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }

    /// <summary>
    /// Represents different types of API error responses using JSON API error response schema
    /// </summary>
    public static class ApiErrors
    {
        /// <summary>
        /// Handles FluentValidation Errors
        /// </summary>
        public static ActionResult ValidationErrorResponse(ValidationException exception)
        {
            var errResponse = new ApiErrorResponse();
            foreach (var failure in exception.Errors)
            {
                errResponse.Errors.Add(new ApiHttpError()
                {
                    Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                    Title = failure.ErrorCode,
                    Detail = failure.ErrorMessage
                });
            }
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }

        /// <summary>
        /// Handles Deserialization Errors
        /// </summary>
        public static ActionResult DeserializationErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                Title = Convert.ToString(ex.GetType()),
                Detail = ex.Message
            });
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }

        /// <summary>
        /// Catch-all for internal server errors
        /// </summary>
        public static ActionResult InternalServerErrorResponse(Exception ex)
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

        /// <summary>
        /// Handles Bad Request Errors
        /// </summary>
        public static ActionResult BadRequestErrorResponse(Exception ex)
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
    }
}
