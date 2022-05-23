using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piipan.QueryTool.Pages
{
    public class MatchModel : BasePageModel
    {

        private readonly ILogger<MatchModel> _logger;
        private readonly IMatchResolutionApi _matchResolutionApi;

        [BindProperty]
        public MatchSearchRequest Query { get; set; } = new MatchSearchRequest();

        public MatchResApiResponse Match { get; set; } = null;
        public List<MatchResApiResponse> AvailableMatches { get; set; } = null;
        public List<ServerError> RequestErrors { get; private set; } = new();

        public MatchModel(ILogger<MatchModel> logger
                           , IMatchResolutionApi matchResolutionApi
                           , IServiceProvider serviceProvider)
                          : base(serviceProvider)

        {
            _logger = logger;
            _matchResolutionApi = matchResolutionApi;
        }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                //Prevents malicious user input
                //Reference: https://github.com/18F/piipan/pull/2692#issuecomment-1045071033
                Regex r = new Regex("^[a-zA-Z0-9]*$");
                if (r.IsMatch(id))
                {
                    //Match ID length = 7 characters
                    //Reference: https://github.com/18F/piipan/pull/2692#issuecomment-1045071033
                    if (id.Length != 7)
                    {
                        return RedirectToPage("Error", new { message = "MatchId not found" });
                    }

                    Match = await _matchResolutionApi.GetMatch(id);
                    if (Match == null)
                    {
                        return RedirectToPage("Error", new { message = "MatchId not found" });
                    }
                }
                else
                {
                    return RedirectToPage("Error", new { message = "MatchId not valid" });
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AvailableMatches = new List<MatchResApiResponse>();
                    var match = await _matchResolutionApi.GetMatch(Query.MatchId);
                    if (match != null)
                    {
                        AvailableMatches.Add(match);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                    RequestErrors.Add(new("", "There was an error running your search. Please try again."));
                }
            }
            else
            {
                var keys = ModelState.Keys;
                foreach (var key in keys)
                {
                    if (ModelState[key]?.Errors?.Count > 0)
                    {
                        var error = ModelState[key].Errors[0];
                        RequestErrors.Add(new(key, error.ErrorMessage));
                    }
                }
            }
            return Page();
        }
    }
}