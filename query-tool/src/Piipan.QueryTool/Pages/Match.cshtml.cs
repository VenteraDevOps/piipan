using System;
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

        private readonly ILogger<IndexModel> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchApi _matchApi;

        public MatchData Match = new MatchData();

        List<MatchData> Matches = new List<MatchData>();

        public MatchModel(ILogger<IndexModel> logger
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
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (r.IsMatch(id)) {

                Match = Matches.Find(x => x.MatchId == id);

                if(Match == null) {
                    return RedirectToPage("/NotFound");
                }
                
                return Page();
            }
            else {
                return RedirectToPage("/NotFound");
            }

        }

        private void GetFakeData()
        {
            Matches.Add(new MatchData()
            {
                MatchId = "0",
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

            Matches.Add(new MatchData()
            {
                MatchId = "1",
                LdsHash = "",
                Status = "Open",
                MatchCreationDate = "",
                QueryingState = "Montana (MT)",
                QueryingStateVulnerableStatus = true,
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

            Matches.Add(new MatchData()
            {
                MatchId = "1",
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
                MatchStateMatchValidity = false,
                MatchStateInitialAction = "",
                MatchStateDateInitialAction = null,
                MatchStateFinalDisposition = "",
                MatchStateDateFinalDisposition = null,
                MatchStateCaseId = "IA-5678",
                MatchStateParticipantId = "LMN5678",
                ProtectLocation = true
            });

            Matches.Add(new MatchData()
            {
                MatchId = "3",
                LdsHash = null,
        
                ProtectLocation = null
            });

            Matches.Add(new MatchData()
            {
                MatchId = "m1",
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
                MatchStateMatchValidity = false,
                MatchStateInitialAction = "",
                MatchStateDateInitialAction = null,
                MatchStateFinalDisposition = "",
                MatchStateDateFinalDisposition = null,
                MatchStateCaseId = "IA-5678",
                MatchStateParticipantId = "LMN5678",
                ProtectLocation = true
            });

            Matches.Add(new MatchData()
            {
                MatchId = "m2",
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
                MatchStateMatchValidity = false,
                MatchStateInitialAction = "",
                MatchStateDateInitialAction = null,
                MatchStateFinalDisposition = "",
                MatchStateDateFinalDisposition = null,
                MatchStateCaseId = "IA-5678",
                MatchStateParticipantId = "LMN5678",
                ProtectLocation = true
            });

            Matches.Add(new MatchData()
            {
                MatchId = "m3",
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
                MatchStateMatchValidity = false,
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
