using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;
using System;
using System.Linq;

namespace Piipan.QueryTool.Pages
{
    public class BasePageModel : PageModel
    {
        private const string NotAuthorizedPageName = "/NotAuthorized";
        private readonly IClaimsProvider _claimsProvider;
        private readonly ILocationsProvider _locationsProvider;

        public BasePageModel(IServiceProvider serviceProvider)
        {
            _claimsProvider = serviceProvider.GetRequiredService<IClaimsProvider>();
            _locationsProvider = serviceProvider.GetRequiredService<ILocationsProvider>();
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if ((string.IsNullOrEmpty(Location) || string.IsNullOrEmpty(Role)) &&
                (!context.HandlerMethod?.MethodInfo.CustomAttributes.Any(n => n.AttributeType == typeof(IgnoreAuthorizationAttribute)) ?? false))
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = RedirectToPage(NotAuthorizedPageName);
            }
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

        public string Role
        {
            get { return _claimsProvider.GetRole(User); }
        }
        public string BaseUrl
        {
            get { return $"{Request.Scheme}://{Request.Host}"; }
        }
    }
}