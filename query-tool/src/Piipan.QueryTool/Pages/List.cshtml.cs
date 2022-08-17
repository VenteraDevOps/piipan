using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;

namespace Piipan.QueryTool.Pages
{
    public class ListModel : BasePageModel
    {

        private readonly ILogger<ListModel> _logger;
        private readonly IMatchResolutionApi _matchResolutionApi;

        public MatchResListApiResponse AvailableMatches { get; set; } = null;
        public List<ServerError> RequestErrors { get; private set; } = new();

        public ListModel(ILogger<ListModel> logger
                           , IMatchResolutionApi matchResolutionApi
                           , IServiceProvider serviceProvider)
                          : base(serviceProvider)

        {
            _logger = logger;
            _matchResolutionApi = matchResolutionApi;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!IsNationalOffice)
            {
                return UnauthorizedResult();
            }
            AvailableMatches = await _matchResolutionApi.GetMatches();

            return Page();
        }
    }
}