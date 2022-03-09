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

namespace Piipan.QueryTool.Pages
{
    public class MatchModel : BasePageModel
    {

        private readonly ILogger<MatchModel> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchApi _matchApi;

        public MatchData Match = new MatchData();

        List<MatchData> Matches = new List<MatchData>();

        public MatchModel(ILogger<MatchModel> logger
                           , IClaimsProvider claimsProvider
                           , ILdsDeidentifier ldsDeidentifier
                           , IMatchApi matchApi)
                           : base(claimsProvider)

        {
            _logger = logger;
            _ldsDeidentifier = ldsDeidentifier;
            _matchApi = matchApi;

            GetFakeData();
        }

        // [HttpGet("{id}")]
        public IActionResult OnGet(string id)
        {   
            
            if(id != null) {
                //Prevents malicious user input
                //Reference: https://github.com/18F/piipan/pull/2692#issuecomment-1045071033
                Regex r = new Regex("^[a-zA-Z0-9]*$");
                if (r.IsMatch(id)) {

                    Match = Matches.Find(item => item.MatchId == id);

                    //Match ID length = 7 characters
                    //Reference: https://github.com/18F/piipan/pull/2692#issuecomment-1045071033
                    if(Match == null || id.Length != 7) {
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

        private void GetFakeData()
        {
            Matches.Add(new MatchData()
            {
                MatchId = "m123456",
                LdsHash = "",
                Status = "Open",
                MatchCreationDate = "",
                QueryingState = "Montana (MT)",
                QueryingStateVulnerableStatus = false,
                QueryingStateMatchValidity = true,
                QueryingStateInitialAction = "",
                QueryingStateDateInitialAction = null,
                QueryingStateFinalDisposition = "",
                QueryingStateDateFinalDisposition = null,
                QueryingStateCaseId = "MT-1234",
                QueryingStateParticipantId = "JKL1234",
                MatchState = "Iowa (IA)",
                MatchStateVulnerableStatus = false,
                MatchStateMatchValidity = true,
                MatchStateInitialAction = "",
                MatchStateDateInitialAction = null,
                MatchStateFinalDisposition = "",
                MatchStateDateFinalDisposition = null,
                MatchStateCaseId = "IA-5678",
                MatchStateParticipantId = "LMN5678",
                ProtectLocation = true
            });
        }

    }
}