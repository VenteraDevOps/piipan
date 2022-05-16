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

        [IgnoreNACAuthorization]
        public void OnGet()
        {
            Message = "You do not have sufficient NAC roles or a NAC Location associated with your account";
        }
    }
}
