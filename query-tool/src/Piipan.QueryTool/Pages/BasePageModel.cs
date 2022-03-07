using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.Shared.Claims;

namespace Piipan.QueryTool.Pages
{
    public class BasePageModel : PageModel
    {
        private readonly IClaimsProvider _claimsProvider;

        public BasePageModel(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public string Email 
        { 
            get { return _claimsProvider.GetEmail(User); }
        }
        public string BaseUrl
        {
            get { return $"{Request.Scheme}://{Request.Host}"; }
        }
    }
} 