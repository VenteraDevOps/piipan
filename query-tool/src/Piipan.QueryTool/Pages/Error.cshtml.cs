using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;

namespace Piipan.QueryTool.Pages
{
    public class ErrorModel : BasePageModel
    {
        public string Message = "";

        public ErrorModel(IClaimsProvider claimsProvider)
                          : base(claimsProvider)
        {
        }

        [IgnoreNACAuthorization]
        public void OnGet(string message)
        {
            if (message != null)
            {
                Message = message;
            }
        }
    }
}
