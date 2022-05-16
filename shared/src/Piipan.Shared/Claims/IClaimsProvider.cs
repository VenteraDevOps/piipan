using System.Security.Claims;

namespace Piipan.Shared.Claims
{
    public interface IClaimsProvider
    {
        string GetEmail(ClaimsPrincipal claimsPrincipal);
        string GetState(ClaimsPrincipal claimsPrincipal);
        string GetNACRole(ClaimsPrincipal claimsPrincipal);
    }
}