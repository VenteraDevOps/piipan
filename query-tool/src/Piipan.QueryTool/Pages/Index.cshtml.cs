using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public List<ServerError> RequestErrors { get; } = new();
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
                            new RequestPerson { LdsHash = digest, CaseId = Query.CaseId, ParticipantId = Query.ParticipantId, SearchReason = "other" }
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
                        RequestErrors.Add(new("", "Date of birth must be a real date."));
                    }
                    else
                    {
                        RequestErrors.Add(new("", ex.Message));
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

        public string Title { get; private set; } = "";

        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
