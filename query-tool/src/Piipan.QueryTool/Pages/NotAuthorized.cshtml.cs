using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;

namespace Piipan.QueryTool.Pages
{
    public class NotAuthorizedModel : BasePageModel
    {
        public string Message = "";

        public NotAuthorizedModel(IClaimsProvider claimsProvider)
                          : base(claimsProvider)
        {
        }

        [IgnoreAuthorization]
        public void OnGet()
        {
            Message = "You do not have sufficient roles or a location associated with your account";
        }
    }
}
