using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;

namespace Piipan.Shared.Claims
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly ClaimsOptions _options;
        private readonly ILogger<ClaimsProvider> _logger;

        public ClaimsProvider(IOptions<ClaimsOptions> options,
            ILogger<ClaimsProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string GetEmail(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal
                .Claims
                .SingleOrDefault(c => c.Type == _options.Email)?
                .Value;
        }

        public string GetState(ClaimsPrincipal claimsPrincipal)
        {
            foreach (var identity in claimsPrincipal.Identities)
            {
                var stateClaim = identity.Claims.FirstOrDefault(c => c.Type == _options.Role && c.Value.StartsWith(_options.NACLocationPrefix));
                if (stateClaim != null)
                {
                    return stateClaim.Value.Substring(_options.NACLocationPrefix.Length);
                }
            }
            return null;
        }
        public string GetNACRole(ClaimsPrincipal claimsPrincipal)
        {
            foreach (var identity in claimsPrincipal.Identities)
            {
                var nacRoleClaim = identity.Claims.FirstOrDefault(c => c.Type == _options.Role && c.Value.StartsWith(_options.NACRolePrefix));
                if (nacRoleClaim != null)
                {
                    return nacRoleClaim.Value.Substring(_options.NACRolePrefix.Length);
                }
            }
            return null;
        }
    }
}