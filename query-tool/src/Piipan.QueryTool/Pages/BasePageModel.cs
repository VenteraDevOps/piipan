using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;
using System.Linq;

namespace Piipan.QueryTool.Pages
{
    public class BasePageModel : PageModel
    {
        private readonly IClaimsProvider _claimsProvider;

        public BasePageModel(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if ((string.IsNullOrEmpty(State) || string.IsNullOrEmpty(NACRole)) &&
                (!context.HandlerMethod?.MethodInfo.CustomAttributes.Any(n => n.AttributeType == typeof(IgnoreNACAuthorizationAttribute)) ?? false))
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = RedirectToPage("/NotAuthorized");
            }
        }

        public string Email
        {
            get { return _claimsProvider.GetEmail(User); }
        }

        public string State
        {
            get { return _claimsProvider.GetState(User); }
        }

        public string NACRole
        {
            get { return _claimsProvider.GetNACRole(User); }
        }
        public string BaseUrl
        {
            get { return $"{Request.Scheme}://{Request.Host}"; }
        }
    }
}