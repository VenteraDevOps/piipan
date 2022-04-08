using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using System.Text.RegularExpressions;
using Piipan.Match.Api.Models.Resolution;

namespace Piipan.QueryTool.Pages
{
    public class MatchModel : BasePageModel
    {

        private readonly ILogger<MatchModel> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchResolutionApi _matchResolutionApi;

        public MatchResApiResponse Match = new MatchResApiResponse();

        public MatchModel(ILogger<MatchModel> logger
                           , IClaimsProvider claimsProvider
                           , ILdsDeidentifier ldsDeidentifier
                           , IMatchResolutionApi matchResolutionApi)
                           : base(claimsProvider)

        {
            _logger = logger;
            _ldsDeidentifier = ldsDeidentifier;
            _matchResolutionApi = matchResolutionApi;
        }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {   
            
            if(id != null) {
                //Prevents malicious user input
                //Reference: https://github.com/18F/piipan/pull/2692#issuecomment-1045071033
                Regex r = new Regex("^[a-zA-Z0-9]*$");
                if (r.IsMatch(id)) {
                    //Match ID length = 7 characters
                    //Reference: https://github.com/18F/piipan/pull/2692#issuecomment-1045071033
                    if(id.Length != 7) {
                        return RedirectToPage("Error", new { message = "MatchId not found" });
                    }

                    Match = await _matchResolutionApi.GetMatch(id);
                    if (Match == null) {
                        return RedirectToPage("Error", new { message = "MatchId not found" });
                    }

                    return Page();
                }
                else {
                    return RedirectToPage("Error", new { message = "MatchId not valid" });
                }
            }
            else {
                //TODO: Once we have the Match search, this should point to that page
                return RedirectToPage("Error", new { message = "MatchId not valid" });
            }


        }
    }
}