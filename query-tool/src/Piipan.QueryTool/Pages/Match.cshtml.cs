using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Helpers;
using Piipan.Shared.Http;
using Piipan.Shared.Roles;

namespace Piipan.QueryTool.Pages
{
    public class MatchModel : BasePageModel
    {

        private readonly ILogger<MatchModel> _logger;
        private readonly IMatchResolutionApi _matchResolutionApi;
        private readonly IRolesProvider _rolesProvider;

        [BindProperty]
        public MatchSearchRequest Query { get; set; } = new MatchSearchRequest();

        public MatchResApiResponse Match { get; set; } = null;
        public List<MatchResApiResponse> AvailableMatches { get; set; } = null;
        public List<ServerError> RequestErrors { get; private set; } = new();
        public MatchDetailSaveResponseData MatchDetailSaveResponse { get; set; }
        public string UserState { get; set; } = "";
        public string[] RequiredRolesToEdit => _rolesProvider.GetMatchEditRoles();

        public MatchModel(ILogger<MatchModel> logger
                           , IMatchResolutionApi matchResolutionApi
                           , IServiceProvider serviceProvider)
                          : base(serviceProvider)

        {
            _logger = logger;
            _matchResolutionApi = matchResolutionApi;
            _rolesProvider = serviceProvider.GetRequiredService<IRolesProvider>();
        }

        public void InitializeUserState()
        {
            if (Match != null)
            {
                foreach (var state in StateInfo.Results)
                {
                    if (state.StateAbbreviation == Location)
                    {
                        UserState = state.State;
                    }
                }
            }
        }

        private RedirectToPageResult RedirectToNotFoundMatch()
        {
            return RedirectToPage("Error", new { message = "Requested Match Not Found" });
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
                        return RedirectToNotFoundMatch();
                    }

                    Match = await _matchResolutionApi.GetMatch(id, IsNationalOffice ? "*" : Location);
                    if (Match == null)
                    {
                        return RedirectToNotFoundMatch();
                    }
                }
                else
                {
                    return RedirectToPage("Error", new { message = "MatchId not valid" });
                }
            }
            return Page();
        }

        [BindProperty]
        public DispositionModel DispositionData { get; set; } = new DispositionModel();

        public async Task<IActionResult> OnPost([FromRoute] string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        AvailableMatches = new List<MatchResApiResponse>();
                        var match = await _matchResolutionApi.GetMatch(Query.MatchId, IsNationalOffice ? "*" : Location);
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
            }
            else
            {
                MatchDetailSaveResponse = new MatchDetailSaveResponseData() { SaveSuccess = false, FailedDispositionModel = DispositionData };
                if (!_rolesProvider.GetMatchEditRoles().Contains(Role))
                {
                    _logger.LogError($"User {Email} does not have permissions to edit match details.");
                    RequestErrors.Add(new("", "You do not have the role and permissions to edit match details."));
                }
                else
                {
                    // Remove binding errors from anything binding other than DispositionData (namely the Query property)
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        if (!modelStateKey.Contains(nameof(DispositionData)))
                        {
                            ModelState[modelStateKey].Errors.Clear();
                            ModelState[modelStateKey].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            AddEventRequest addEventRequest = new AddEventRequest
                            {
                                Data = new Disposition
                                {
                                    FinalDisposition = DispositionData.FinalDisposition,
                                    FinalDispositionDate = DispositionData.FinalDispositionDate,
                                    InitialActionAt = DispositionData.InitialActionAt,
                                    InitialActionTaken = DispositionData.InitialActionTaken,
                                    InvalidMatch = DispositionData.InvalidMatch,
                                    InvalidMatchReason = DispositionData.InvalidMatchReason,
                                    OtherReasoningForInvalidMatch = DispositionData.OtherReasoningForInvalidMatch,
                                    VulnerableIndividual = DispositionData.VulnerableIndividual
                                }
                            };
                            var (_, failResponse) = await _matchResolutionApi.AddMatchResEvent(id, addEventRequest, Location);
                            if (string.IsNullOrEmpty(failResponse))
                            {
                                MatchDetailSaveResponse.SaveSuccess = true;
                                MatchDetailSaveResponse.FailedDispositionModel = null;
                            }
                            else
                            {
                                ApiErrorResponse apiErrorResponse = JsonHelper.TryParse<ApiErrorResponse>(failResponse);
                                if (apiErrorResponse?.Errors?.Count > 0)
                                {
                                    foreach (var error in apiErrorResponse.Errors)
                                    {
                                        RequestErrors.Add(new("", error.Detail));
                                    }
                                }
                                else
                                {
                                    RequestErrors.Add(new("", "There was an error saving your data. Please try again."));
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception, exception.Message);
                            RequestErrors.Add(new("", "There was an error saving your data. Please try again."));
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
                }

                Match = await _matchResolutionApi.GetMatch(id, IsNationalOffice ? "*" : Location);
                if (Match == null)
                {
                    return RedirectToNotFoundMatch();
                }
            }
            return Page();
        }

    }
}