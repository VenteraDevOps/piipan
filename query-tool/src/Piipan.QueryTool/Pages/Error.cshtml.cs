using System;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Authorization;

namespace Piipan.QueryTool.Pages
{
    public class ErrorModel : BasePageModel
    {
        private readonly ILogger<ErrorModel> _logger;
        public string Message = "";

        public ErrorModel(ILogger<ErrorModel> logger,
            IServiceProvider serviceProvider)
                          : base(serviceProvider)
        {
            _logger = logger;
        }

        [IgnoreAuthorization]
        public void OnGet(string message)
        {
            _logger.LogError($"Arrived at error page with message {message}");
            if (message != null)
            {
                Message = message;
            }
        }
    }
}
