using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;

namespace Piipan.QueryTool.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ApiController : BaseController
    {
        private readonly ILogger<ApiController> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchApi _matchApi;

        public ApiController(ILogger<ApiController> logger,
                          IClaimsProvider claimsProvider,
                          ILdsDeidentifier ldsDeidentifier,
                          IMatchApi matchApi)
                          : base(claimsProvider)
        {
            _logger = logger;
            _ldsDeidentifier = ldsDeidentifier;
            _matchApi = matchApi;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForm([FromBody] PiiRecord query)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Query form submitted");

                    string digest = _ldsDeidentifier.Run(
                        query.LastName,
                        query.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                        query.SocialSecurityNum
                    );

                    var request = new OrchMatchRequest
                    {
                        Data = new List<RequestPerson>
                        {
                            new RequestPerson { LdsHash = digest, CaseId = query.CaseId, ParticipantId = query.ParticipantId }
                        }
                    };

                    var response = await _matchApi.FindMatches(request, "ea");
                    var stringResponse = JsonConvert.SerializeObject(response);
                    return Content(stringResponse);
                    //NoResults = QueryResult.Data.Results.Count == 0 ||
                    //    QueryResult.Data.Results[0].Matches.Count() == 0;

                    //Title = "NAC Query Results";
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    if (ex.Message.ToLower().Contains("gregorian"))
                    {
                        //RequestError = "Date of birth must be a real date.";
                    }
                    else
                    {
                        //RequestError = $"{ex.Message}";
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                    //RequestError = "There was an error running your search. Please try again.";
                }
            }

            return null;
        }
    }
}
