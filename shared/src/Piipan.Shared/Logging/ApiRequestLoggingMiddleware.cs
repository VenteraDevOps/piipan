using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Piipan.Shared.Logging
{
    public class ApiRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiRequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<RequestLoggingMiddleware> logger)
        {
            logger.LogInformation("Executing request from user {User}", context.User?.Identity.Name);

            HttpRequest request = context.Request;
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
            await _next(context);
        }
    }
}
