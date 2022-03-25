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

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchApi _matchApi;

        public IndexModel(ILogger<IndexModel> logger,
                          IClaimsProvider claimsProvider,
                          ILdsDeidentifier ldsDeidentifier,
                          IMatchApi matchApi)
                          : base(claimsProvider)
        {
            _logger = logger;
            _ldsDeidentifier = ldsDeidentifier;
            _matchApi = matchApi;
        }

        [BindProperty]
        public PiiRecord Query { get; set; } = new PiiRecord();
        public OrchMatchResponse QueryResult { get; private set; }
        public String RequestError { get; private set; }
        public bool NoResults = false;

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Query form submitted");

                    string digest = _ldsDeidentifier.Run(
                        Query.LastName,
                        Query.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                        Query.SocialSecurityNum
                    );

                    var request = new OrchMatchRequest
                    {
                        Data = new List<RequestPerson>
                        {
                            new RequestPerson { LdsHash = digest, CaseId = Query.CaseId, ParticipantId = Query.ParticipantId }
                        }
                    };

                    var response = await _matchApi.FindMatches(request, "ea");

                    QueryResult = response;
                    NoResults = QueryResult.Data.Results.Count == 0 ||
                        QueryResult.Data.Results[0].Matches.Count() == 0;

                    Title = "NAC Query Results";
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    if (ex.Message.ToLower().Contains("gregorian"))
                    {
                        RequestError = "Date of birth must be a real date.";
                    }
                    else
                    {
                        RequestError = $"{ex.Message}";
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                    RequestError = "There was an error running your search. Please try again.";
                }
            }

            return Page();
        }

        public string Title { get; private set; } = "";

        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
