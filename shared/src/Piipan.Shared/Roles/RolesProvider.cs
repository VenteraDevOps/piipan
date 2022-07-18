using System;
using Microsoft.Extensions.Options;
using Piipan.Shared.Locations;

namespace Piipan.Shared.Roles
{
    public class RolesProvider : IRolesProvider
    {
        private readonly RoleOptions _options;

        public RolesProvider(IOptions<RoleOptions> options)
        {
            _options = options.Value;
        }

        public string[] GetMatchEditRoles()
        {
            return _options.EditMatch ?? Array.Empty<string>();
        }
    }
}
