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
// using Piipan.Match.Core.DataAccessObjects;
// using Piipan.Match.Core.Exceptions;
using Piipan.Match.Api.Models;

namespace Piipan.QueryTool.Pages
{
    public class MatchModel : BasePageModel
    {

        private readonly ILogger<IndexModel> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchApi _matchApi;

        public MatchData Match = new MatchData();

        private MatchData[] Participants = Enumerable.Range(0, 10)
                                                            .Select(x => new MatchData())
                                                            .ToArray();

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
        public void OnGet(string id)
        {   
            int selectedId = 0;
            Int32.TryParse(id, out selectedId);
            Match = Participants[selectedId];
        }

        private void GetFakeData()
        {
            Participants[0] = new MatchData()
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
            };

            Participants[1] = new MatchData()
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
            };

            Participants[2] = new MatchData()
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
            };
            Participants[3] = new MatchData()
            {
                MatchId = "3",
                LdsHash = null,
        
                ProtectLocation = null
            };
        }

    }
}
