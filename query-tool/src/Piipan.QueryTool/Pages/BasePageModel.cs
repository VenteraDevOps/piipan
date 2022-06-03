using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Services;
using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;

namespace Piipan.QueryTool.Pages
{
    public class BasePageModel : PageModel
    {
        private const string NotAuthorizedPageName = "/NotAuthorized";
        private readonly IClaimsProvider _claimsProvider;
        private readonly ILocationsProvider _locationsProvider;
        private readonly IStateInfoService _stateInfoService;

        public BasePageModel(IServiceProvider serviceProvider)
        {
            _claimsProvider = serviceProvider.GetRequiredService<IClaimsProvider>();
            _locationsProvider = serviceProvider.GetRequiredService<ILocationsProvider>();
            _stateInfoService = serviceProvider.GetRequiredService<IStateInfoService>();
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            StateInfo = await _stateInfoService.GetStateInfoAsync();
            if ((string.IsNullOrEmpty(Location) || string.IsNullOrEmpty(Role)) &&
                (!context.HandlerMethod?.MethodInfo.CustomAttributes.Any(n => n.AttributeType == typeof(IgnoreAuthorizationAttribute)) ?? false))
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = RedirectToUnauthorized();
            }
            await next();
        }

        protected IActionResult RedirectToUnauthorized()
        {
            return RedirectToPage(NotAuthorizedPageName);
        }

        public string Email
        {
            get { return _claimsProvider.GetEmail(User); }
        }

        public string Location
        {
            get { return _claimsProvider.GetLocation(User); }
        }

        public string[] States
        {
            get { return _locationsProvider.GetStates(Location); }
        }

        public bool IsNationalOffice => States?.Contains("*") ?? false;

        public string Role
        {
            get { return _claimsProvider.GetRole(User); }
        }
        public string BaseUrl
        {
            get { return $"{Request.Scheme}://{Request.Host}"; }
        }

        public StateInfoResponse StateInfo { get; set; }
    }
}