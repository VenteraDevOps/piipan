using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Deidentification;

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchApi _matchApi;

        public IndexModel(ILogger<IndexModel> logger,
                          ILdsDeidentifier ldsDeidentifier,
                          IMatchApi matchApi,
                          IServiceProvider serviceProvider)
                          : base(serviceProvider)
        {
            _logger = logger;
            _ldsDeidentifier = ldsDeidentifier;
            _matchApi = matchApi;
        }

        [BindProperty]
        public QueryFormModel QueryFormData { get; set; } = new QueryFormModel();

        public async Task<IActionResult> OnPostAsync()
        {
            if (Location.Length != 2 || States?.Length != 1)
            {
                QueryFormData.ServerErrors.Add(new("", "Search performed with an invalid location"));
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Query form submitted");

                    string digest = _ldsDeidentifier.Run(
                        QueryFormData.Query.LastName,
                        QueryFormData.Query.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                        QueryFormData.Query.SocialSecurityNum
                    );

                    var request = new OrchMatchRequest
                    {
                        Data = new List<RequestPerson>
                        {
                            new RequestPerson
                            {
                                LdsHash = digest,
                                CaseId = QueryFormData.Query.CaseId,
                                ParticipantId = QueryFormData.Query.ParticipantId,
                                SearchReason = "other"
                            }
                        }
                    };

                    var response = await _matchApi.FindMatches(request, States[0].ToLower());

                    QueryFormData.QueryResult = response?.Data;

                    Title = "NAC Query Results";
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    if (ex.Message.ToLower().Contains("gregorian"))
                    {
                        QueryFormData.ServerErrors.Add(new("", "Date of birth must be a real date."));
                    }
                    else
                    {
                        QueryFormData.ServerErrors.Add(new("", ex.Message));
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                    QueryFormData.ServerErrors.Add(new("", "There was an error running your search. Please try again."));
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
                        QueryFormData.ServerErrors.Add(new(key, error.ErrorMessage));
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
