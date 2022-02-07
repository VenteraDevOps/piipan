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

        public ParticipantMatch Match = new ParticipantMatch();

        private ParticipantMatch[] Participants = Enumerable.Range(0, 10)
                                                            .Select(x => new ParticipantMatch())
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
            Participants[0] = new ParticipantMatch()
            {
                MatchId = "0",
                LdsHash = null,
                State = "VI",
                CaseId = "1",
                ParticipantId = "1",
                BenefitsEndDate = null,
                RecentBenefitMonths = null,
                ProtectLocation = null
            };

            Participants[1] = new ParticipantMatch()
            {
                MatchId = "1",
                LdsHash = null,
                State = "VI",
                CaseId = "1",
                ParticipantId = "1",
                BenefitsEndDate = null,
                RecentBenefitMonths = null,
                ProtectLocation = null
            };

            Participants[2] = new ParticipantMatch()
            {
                MatchId = "1",
                LdsHash = null,
                State = "VI",
                CaseId = "1",
                ParticipantId = "1",
                BenefitsEndDate = null,
                RecentBenefitMonths = null,
                ProtectLocation = null
            };
        }

    }
}
