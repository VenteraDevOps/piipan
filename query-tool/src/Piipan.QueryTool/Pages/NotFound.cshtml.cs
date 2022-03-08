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
    public class NotFoundModel : BasePageModel
    {
        private readonly ILogger<NotFoundModel> _logger;
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchApi _matchApi;

        public NotFoundModel(ILogger<NotFoundModel> logger,
                          IClaimsProvider claimsProvider,
                          ILdsDeidentifier ldsDeidentifier,
                          IMatchApi matchApi)
                          : base(claimsProvider)
        {
            _logger = logger;
            _ldsDeidentifier = ldsDeidentifier;
            _matchApi = matchApi;
        }

        public void OnGet()
        {
        }
    }
}