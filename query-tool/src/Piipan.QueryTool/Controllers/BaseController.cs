using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.Shared.Claims;

namespace Piipan.QueryTool.Controllers
{
    public class BaseController : ControllerBase
    {
        private readonly IClaimsProvider _claimsProvider;

        public BaseController(IClaimsProvider claimsProvider)
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
